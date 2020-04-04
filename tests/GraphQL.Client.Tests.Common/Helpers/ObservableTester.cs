using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace GraphQL.Client.Tests.Common.Helpers
{
    public class ObservableTester<TSubscriptionPayload> : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly ManualResetEventSlim _updateReceived = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _completed = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _error = new ManualResetEventSlim();
        private readonly EventLoopScheduler _observeScheduler = new EventLoopScheduler();

        /// <summary>
        /// The timeout for SubscriptionAssertions.***Have*** methods. Defaults to 3 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Indicates that an update has been received since the last reset of <see cref="_updateReceived"/>
        /// </summary>
        public bool UpdateReceived => _updateReceived.IsSet;

        /// <summary>
        /// The last payload which was received.
        /// </summary>
        public TSubscriptionPayload LastPayload { get; private set; }

        public Exception Error { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ObservableTester{T}"/> which subscribes to the supplied <see cref="IObservable{T}"/>
        /// </summary>
        /// <param name="observable">the <see cref="IObservable{T}"/> under test</param>
        public ObservableTester(IObservable<TSubscriptionPayload> observable)
        {

            _observeScheduler.Schedule(() =>
                Debug.WriteLine($"Observe scheduler thread id: {Thread.CurrentThread.ManagedThreadId}"));

            _subscription = observable.ObserveOn(_observeScheduler).Subscribe(
                obj =>
                {
                    Debug.WriteLine($"observable tester {GetHashCode()}: payload received");
                    LastPayload = obj;
                    _updateReceived.Set();
                },
                ex =>
                {
                    Debug.WriteLine($"observable tester {GetHashCode()} error received: {ex}");
                    Error = ex;
                    _error.Set();
                },
                () =>
                {
                    Debug.WriteLine($"observable tester {GetHashCode()}: completed");
                    _completed.Set();
                });
        }

        /// <summary>
        /// Resets the tester class. Should be called before triggering the potential update
        /// </summary>
        private void Reset() => _updateReceived.Reset();

        /// <inheritdoc />
        public void Dispose()
        {
            _subscription?.Dispose();
            _observeScheduler.Dispose();
        }

        public SubscriptionAssertions<TSubscriptionPayload> Should() => new SubscriptionAssertions<TSubscriptionPayload>(this);

        public class SubscriptionAssertions<TPayload> : ReferenceTypeAssertions<ObservableTester<TPayload>, SubscriptionAssertions<TPayload>>
        {
            public SubscriptionAssertions(ObservableTester<TPayload> tester)
            {
                Subject = tester;
            }

            protected override string Identifier => "Subscription";

            public AndWhichConstraint<SubscriptionAssertions<TPayload>, TPayload> HaveReceivedPayload(TimeSpan timeout,
                string because = "", params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() =>
                    {
                        var isSet = Subject._updateReceived.Wait(timeout);
                        if (!isSet)
                            Debug.WriteLine($"waiting for payload on thread {Thread.CurrentThread.ManagedThreadId} timed out!");
                        return isSet;
                    })
                    .ForCondition(isSet => isSet)
                    .FailWith("Expected {context:Subscription} to receive new payload{reason}, but did not receive an update within {0}", timeout);

                Subject._updateReceived.Reset();
                return new AndWhichConstraint<SubscriptionAssertions<TPayload>, TPayload>(this, Subject.LastPayload);
            }

            public AndWhichConstraint<SubscriptionAssertions<TPayload>, TPayload> HaveReceivedPayload(string because = "", params object[] becauseArgs)
                => HaveReceivedPayload(Subject.Timeout, because, becauseArgs);

            public AndConstraint<SubscriptionAssertions<TPayload>> NotHaveReceivedPayload(TimeSpan timeout,
                string because = "", params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() => Subject._updateReceived.Wait(timeout))
                    .ForCondition(isSet => !isSet)
                    .FailWith("Expected {context:Subscription} to not receive a new payload{reason}, but did receive an update: {0}", Subject.LastPayload);

                Subject._updateReceived.Reset();
                return new AndConstraint<SubscriptionAssertions<TPayload>>(this);
            }
            public AndConstraint<SubscriptionAssertions<TPayload>> NotHaveReceivedPayload(string because = "", params object[] becauseArgs)
                => NotHaveReceivedPayload(TimeSpan.FromMilliseconds(100), because, becauseArgs);

            public AndWhichConstraint<SubscriptionAssertions<TPayload>, Exception> HaveReceivedError(TimeSpan timeout,
                string because = "", params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() => Subject._error.Wait(timeout))
                    .ForCondition(isSet => isSet)
                    .FailWith("Expected {context:Subscription} to fail{reason}, but did not receive an error within {0}", timeout);

                return new AndWhichConstraint<SubscriptionAssertions<TPayload>, Exception>(this, Subject.Error);
            }

            public AndWhichConstraint<SubscriptionAssertions<TPayload>, Exception> HaveReceivedError(string because = "", params object[] becauseArgs)
                => HaveReceivedError(Subject.Timeout, because, becauseArgs);

            public AndConstraint<SubscriptionAssertions<TPayload>> HaveCompleted(TimeSpan timeout,
                string because = "", params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() => Subject._completed.Wait(timeout))
                    .ForCondition(isSet => isSet)
                    .FailWith("Expected {context:Subscription} to complete{reason}, but did not complete within {0}", timeout);

                return new AndConstraint<SubscriptionAssertions<TPayload>>(this);
            }
            public AndConstraint<SubscriptionAssertions<TPayload>> HaveCompleted(string because = "", params object[] becauseArgs)
                => HaveCompleted(Subject.Timeout, because, becauseArgs);

            public AndConstraint<SubscriptionAssertions<TPayload>> NotHaveCompleted(TimeSpan timeout,
                string because = "", params object[] becauseArgs)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .Given(() => Subject._completed.Wait(timeout))
                    .ForCondition(isSet => !isSet)
                    .FailWith("Expected {context:Subscription} not to complete within {0}{reason}, but it did!", timeout);

                return new AndConstraint<SubscriptionAssertions<TPayload>>(this);
            }
            public AndConstraint<SubscriptionAssertions<TPayload>> NotHaveCompleted(string because = "", params object[] becauseArgs)
                => NotHaveCompleted(Subject.Timeout, because, becauseArgs);
        }
    }

    public static class ObservableExtensions
    {
        public static ObservableTester<T> Monitor<T>(this IObservable<T> observable) => new ObservableTester<T>(observable);
    }
}
