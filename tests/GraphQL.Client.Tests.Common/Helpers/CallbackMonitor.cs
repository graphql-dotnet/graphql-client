using System.Diagnostics;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace GraphQL.Client.Tests.Common.Helpers;

public class CallbackMonitor<T>
{
    private readonly ManualResetEventSlim _callbackInvoked = new ManualResetEventSlim();

    /// <summary>
    /// The timeout for <see cref="CallbackAssertions{T}.HaveBeenInvokedWithPayload(TimeSpan, string, object[])"/>. Defaults to 1 second.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Indicates that an update has been received since the last <see cref="Reset"/>
    /// </summary>
    public bool CallbackInvoked => _callbackInvoked.IsSet;

    /// <summary>
    /// The last payload which was received.
    /// </summary>
    public T LastPayload { get; private set; }

    public void Invoke(T param)
    {
        LastPayload = param;
        Debug.WriteLine($"CallbackMonitor invoke handler thread id: {Thread.CurrentThread.ManagedThreadId}");
        _callbackInvoked.Set();
    }

    /// <summary>
    /// Resets the tester class. Should be called before triggering the potential update
    /// </summary>
    public void Reset()
    {
        LastPayload = default;
        _callbackInvoked.Reset();
    }

    public CallbackAssertions<T> Should() => new CallbackAssertions<T>(this);

    public class CallbackAssertions<TPayload> : ReferenceTypeAssertions<CallbackMonitor<TPayload>, CallbackAssertions<TPayload>>
    {
        public CallbackAssertions(CallbackMonitor<TPayload> tester) : base(tester)
        { }

        protected override string Identifier => "callback";

        public AndWhichConstraint<CallbackAssertions<TPayload>, TPayload> HaveBeenInvokedWithPayload(TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() =>
                {
                    Debug.WriteLine($"HaveBeenInvokedWithPayload thread id: {Thread.CurrentThread.ManagedThreadId}");
                    return Subject._callbackInvoked.Wait(timeout);
                })
                .ForCondition(isSet => isSet)
                .FailWith("Expected {context:callback} to be invoked{reason}, but did not receive a call within {0}", timeout);

            Subject._callbackInvoked.Reset();
            return new AndWhichConstraint<CallbackAssertions<TPayload>, TPayload>(this, Subject.LastPayload);
        }
        public AndWhichConstraint<CallbackAssertions<TPayload>, TPayload> HaveBeenInvokedWithPayload(string because = "", params object[] becauseArgs)
            => HaveBeenInvokedWithPayload(Subject.Timeout, because, becauseArgs);

        public AndConstraint<CallbackAssertions<TPayload>> HaveBeenInvoked(TimeSpan timeout, string because = "", params object[] becauseArgs)
            => HaveBeenInvokedWithPayload(timeout, because, becauseArgs);
        public AndConstraint<CallbackAssertions<TPayload>> HaveBeenInvoked(string because = "", params object[] becauseArgs)
            => HaveBeenInvokedWithPayload(Subject.Timeout, because, becauseArgs);

        public AndConstraint<CallbackAssertions<TPayload>> NotHaveBeenInvoked(TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject._callbackInvoked.Wait(timeout))
                .ForCondition(isSet => !isSet)
                .FailWith("Expected {context:callback} to not be invoked{reason}, but did receive a call: {0}", Subject.LastPayload);

            Subject._callbackInvoked.Reset();
            return new AndConstraint<CallbackAssertions<TPayload>>(this);
        }
        public AndConstraint<CallbackAssertions<TPayload>> NotHaveBeenInvoked(string because = "", params object[] becauseArgs)
            => NotHaveBeenInvoked(TimeSpan.FromMilliseconds(100), because, becauseArgs);
    }
}
