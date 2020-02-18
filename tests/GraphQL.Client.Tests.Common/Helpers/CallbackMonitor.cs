using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace GraphQL.Client.Tests.Common.Helpers {
	public class CallbackMonitor<T> {
		private readonly ManualResetEventSlim callbackInvoked = new ManualResetEventSlim();

		/// <summary>
		/// The timeout for <see cref="ShouldHaveReceivedUpdate"/>. Defaults to 1 s
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Indicates that an update has been received since the last <see cref="Reset"/>
		/// </summary>
		public bool CallbackInvoked => callbackInvoked.IsSet;
		/// <summary>
		/// The last payload which was received.
		/// </summary>
		public T LastPayload { get; private set; }

		public void Invoke(T param) {
			LastPayload = param;
			callbackInvoked.Set();
		}

		/// <summary>
		/// Asserts that a new update has been pushed to the <see cref="IObservable{T}"/> within the configured <see cref="Timeout"/> since the last <see cref="Reset"/>.
		/// If supplied, the <paramref name="assertPayload"/> action is executed on the submitted payload.
		/// </summary>
		/// <param name="assertPayload">action to assert the contents of the payload</param>
		public void CallbackShouldHaveBeenInvoked(Action<T> assertPayload = null, TimeSpan? timeout = null) {
			try {
				callbackInvoked.Wait(timeout ?? Timeout).Should().BeTrue("because the callback method should have been invoked (timeout: {0} s)",
					(timeout ?? Timeout).TotalSeconds);

				assertPayload?.Invoke(LastPayload);
			}
			finally {
				Reset();
			}
		}

		/// <summary>
		/// Asserts that no new update has been pushed within the given <paramref name="millisecondsTimeout"/> since the last <see cref="Reset"/>
		/// </summary>
		/// <param name="millisecondsTimeout">the time in ms in which no new update must be pushed to the <see cref="IObservable{T}"/>. defaults to 100</param>
		public void CallbackShouldNotHaveBeenInvoked(TimeSpan? timeout = null) {
			if (!timeout.HasValue) timeout = TimeSpan.FromMilliseconds(100);
			try {
				callbackInvoked.Wait(timeout.Value).Should().BeFalse("because the callback method should not have been invoked");
			}
			finally {
				Reset();
			}
		}

		/// <summary>
		/// Resets the tester class. Should be called before triggering the potential update
		/// </summary>
		public void Reset() {
			LastPayload = default(T);
			callbackInvoked.Reset();
		}


		public CallbackAssertions<T> Should() {
			return new CallbackAssertions<T>(this);
		}

		public class CallbackAssertions<TPayload> : ReferenceTypeAssertions<CallbackMonitor<TPayload>, CallbackAssertions<TPayload>> {
			public CallbackAssertions(CallbackMonitor<TPayload> tester) {
				Subject = tester;
			}

			protected override string Identifier => "callback";

			public AndWhichConstraint<CallbackAssertions<TPayload>, TPayload> HaveBeenInvokedWithPayload(TimeSpan timeout,
				string because = "", params object[] becauseArgs) {
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.Given(() => Subject.callbackInvoked.Wait(timeout))
					.ForCondition(isSet => isSet)
					.FailWith("Expected {context:callback} to be invoked{reason}, but did not receive a call within {0}", timeout);

				Subject.callbackInvoked.Reset();
				return new AndWhichConstraint<CallbackAssertions<TPayload>, TPayload>(this, Subject.LastPayload);
			}
			public AndWhichConstraint<CallbackAssertions<TPayload>, TPayload> HaveBeenInvokedWithPayload(string because = "", params object[] becauseArgs)
				=> HaveBeenInvokedWithPayload(Subject.Timeout, because, becauseArgs);

			public AndConstraint<CallbackAssertions<TPayload>> HaveBeenInvoked(TimeSpan timeout, string because = "", params object[] becauseArgs)
				=> HaveBeenInvokedWithPayload(timeout, because, becauseArgs);
			public AndConstraint<CallbackAssertions<TPayload>> HaveBeenInvoked(string because = "", params object[] becauseArgs)
				=> HaveBeenInvokedWithPayload(Subject.Timeout, because, becauseArgs);

			public AndConstraint<CallbackAssertions<TPayload>> NotHaveBeenInvoked(TimeSpan timeout,
				string because = "", params object[] becauseArgs) {
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.Given(() => Subject.callbackInvoked.Wait(timeout))
					.ForCondition(isSet => !isSet)
					.FailWith("Expected {context:callback} to not be invoked{reason}, but did receive a call: {0}", Subject.LastPayload);

				Subject.callbackInvoked.Reset();
				return new AndConstraint<CallbackAssertions<TPayload>>(this);
			}
			public AndConstraint<CallbackAssertions<TPayload>> NotHaveBeenInvoked(string because = "", params object[] becauseArgs)
				=> NotHaveBeenInvoked(TimeSpan.FromMilliseconds(100), because, becauseArgs);
		}
	}
}
