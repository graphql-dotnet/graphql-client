using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace GraphQL.Client.Tests.Common.Helpers {
	public class ObservableTester<TSubscriptionPayload> : IDisposable {
		private readonly IDisposable subscription;
		private readonly EventLoopScheduler scheduler;
		private readonly ManualResetEventSlim updateReceived = new ManualResetEventSlim();
		private readonly ManualResetEventSlim completed = new ManualResetEventSlim();
		private readonly ManualResetEventSlim error = new ManualResetEventSlim();

		/// <summary>
		/// The timeout for <see cref="ShouldHaveReceivedUpdate"/>. Defaults to 1 s
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Indicates that an update has been received since the last <see cref="_reset"/>
		/// </summary>
		public bool UpdateReceived => updateReceived.IsSet;
		/// <summary>
		/// The last payload which was received.
		/// </summary>
		public TSubscriptionPayload LastPayload { get; private set; }

		public Exception Error { get; private set; }

		/// <summary>
		/// Creates a new <see cref="ObservableTester{T}"/> which subscribes to the supplied <see cref="IObservable{T}"/>
		/// </summary>
		/// <param name="observable">the <see cref="IObservable{T}"/> under test</param>
		public ObservableTester(IObservable<TSubscriptionPayload> observable) {
			scheduler = new EventLoopScheduler();
			subscription = observable.SubscribeOn(Scheduler.CurrentThread).ObserveOn(scheduler).Subscribe(
				obj => {
					Debug.WriteLine($"observable tester {GetHashCode()}: payload received");
					LastPayload = obj;
					updateReceived.Set();
				},
				ex => {
					Debug.WriteLine($"observable tester {GetHashCode()} error received: {ex}");
					Error = ex;
					error.Set();
				},
				() => completed.Set()
			);
		}

		/// <summary>
		/// Resets the tester class. Should be called before triggering the potential update
		/// </summary>
		private void Reset() {
			updateReceived.Reset();
		}

		/// <inheritdoc />
		public void Dispose() {
			subscription?.Dispose();
			scheduler?.Dispose();
		}

		public SubscriptionAssertions<TSubscriptionPayload> Should() {
			return new SubscriptionAssertions<TSubscriptionPayload>(this);
		}

		public class SubscriptionAssertions<TPayload> : ReferenceTypeAssertions<ObservableTester<TPayload>, SubscriptionAssertions<TPayload>> {
			public SubscriptionAssertions(ObservableTester<TPayload> tester) {
				Subject = tester;
			}

			protected override string Identifier => "Subscription";

			public AndWhichConstraint<SubscriptionAssertions<TPayload>, TPayload> HaveReceivedPayload(TimeSpan timeout,
				string because = "", params object[] becauseArgs) {
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.Given(() => Subject.updateReceived.Wait(timeout))
					.ForCondition(isSet => isSet)
					.FailWith("Expected {context:Subscription} to receive new payload{reason}, but did not receive an update within {0}", timeout);

				Subject.updateReceived.Reset();
				return new AndWhichConstraint<SubscriptionAssertions<TPayload>, TPayload>(this, Subject.LastPayload);
			}
			public AndWhichConstraint<SubscriptionAssertions<TPayload>, TPayload> HaveReceivedPayload(string because = "", params object[] becauseArgs)
				=> HaveReceivedPayload(Subject.Timeout, because, becauseArgs);

			public AndConstraint<SubscriptionAssertions<TPayload>> NotHaveReceivedPayload(TimeSpan timeout,
				string because = "", params object[] becauseArgs) {
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.Given(() => Subject.updateReceived.Wait(timeout))
					.ForCondition(isSet => !isSet)
					.FailWith("Expected {context:Subscription} to not receive a new payload{reason}, but did receive an update: {0}", Subject.LastPayload);

				Subject.updateReceived.Reset();
				return new AndConstraint<SubscriptionAssertions<TPayload>>(this);
			}
			public AndConstraint<SubscriptionAssertions<TPayload>> NotHaveReceivedPayload(string because = "", params object[] becauseArgs)
				=> NotHaveReceivedPayload(TimeSpan.FromMilliseconds(100), because, becauseArgs);

			public AndWhichConstraint<SubscriptionAssertions<TPayload>, Exception> HaveReceivedError(TimeSpan timeout,
				string because = "", params object[] becauseArgs) {
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.Given(() => Subject.error.Wait(timeout))
					.ForCondition(isSet => isSet)
					.FailWith("Expected {context:Subscription} to fail{reason}, but did not receive an error within {0}", timeout);

				return new AndWhichConstraint<SubscriptionAssertions<TPayload>, Exception>(this, Subject.Error);
			}
			public AndWhichConstraint<SubscriptionAssertions<TPayload>, Exception> HaveReceivedError(string because = "", params object[] becauseArgs)
				=> HaveReceivedError(Subject.Timeout, because, becauseArgs);


			public AndConstraint<SubscriptionAssertions<TPayload>> HaveCompleted(TimeSpan timeout,
				string because = "", params object[] becauseArgs) {
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.Given(() => Subject.completed.Wait(timeout))
					.ForCondition(isSet => isSet)
					.FailWith("Expected {context:Subscription} to complete{reason}, but did not complete within {0}", timeout);

				return new AndConstraint<SubscriptionAssertions<TPayload>>(this);
			}
			public AndConstraint<SubscriptionAssertions<TPayload>> HaveCompleted(string because = "", params object[] becauseArgs)
				=> HaveCompleted(Subject.Timeout, because, becauseArgs);
		}
	}

	public static class ObservableExtensions {
		public static ObservableTester<T> Monitor<T>(this IObservable<T> observable) {
			return new ObservableTester<T>(observable);
		}
	}

}
