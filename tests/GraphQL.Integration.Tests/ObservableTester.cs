using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GraphQL.Integration.Tests
{
	public class ObservableTester<T> : IDisposable
	{
		private readonly IDisposable _subscription;
		private ManualResetEventSlim _updateReceived { get; } = new ManualResetEventSlim();
		private ManualResetEventSlim _completed { get; } = new ManualResetEventSlim();
		private ManualResetEventSlim _error { get; } = new ManualResetEventSlim();

		/// <summary>
		/// The timeout for <see cref="ShouldHaveReceivedUpdate"/>. Defaults to 1 s
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Indicates that an update has been received since the last <see cref="_reset"/>
		/// </summary>
		public bool UpdateReceived => _updateReceived.IsSet;
		/// <summary>
		/// The last payload which was received.
		/// </summary>
		public T LastPayload { get; private set; }

		public Exception Error { get; private set; }

		/// <summary>
		/// Creates a new <see cref="ObservableTester{T}"/> which subscribes to the supplied <see cref="IObservable{T}"/>
		/// </summary>
		/// <param name="observable">the <see cref="IObservable{T}"/> under test</param>
		public ObservableTester(IObservable<T> observable)
		{
			_subscription = observable.Subscribe(
				obj => {
					LastPayload = obj;
					_updateReceived.Set();
				},
				ex =>
				{
					Error = ex;
					_error.Set();
				},
				() => _completed.Set()
			);
		}

		/// <summary>
		/// Asserts that a new update has been pushed to the <see cref="IObservable{T}"/> within the configured <see cref="Timeout"/> since the last <see cref="_reset"/>.
		/// If supplied, the <paramref name="assertPayload"/> action is executed on the submitted payload.
		/// </summary>
		/// <param name="assertPayload">action to assert the contents of the payload</param>
		public void ShouldHaveReceivedUpdate(Action<T> assertPayload = null, TimeSpan? timeout = null)
		{
			try
			{
				if (!_updateReceived.Wait(timeout ?? Timeout))
					Assert.True(false, $"no update received within {(timeout ?? Timeout).TotalSeconds} s!");

				assertPayload?.Invoke(LastPayload);
			}
			finally
			{
				_reset();
			}
		}

		/// <summary>
		/// Asserts that no new update has been pushed within the given <paramref name="millisecondsTimeout"/> since the last <see cref="_reset"/>
		/// </summary>
		/// <param name="millisecondsTimeout">the time in ms in which no new update must be pushed to the <see cref="IObservable{T}"/>. defaults to 100</param>
		public void ShouldNotHaveReceivedUpdate(TimeSpan? timeout = null)
		{
			if(!timeout.HasValue) timeout = TimeSpan.FromMilliseconds(100);
			try
			{
				if (_updateReceived.Wait(timeout.Value))
					Assert.True(false, "update was inadvertently pushed!");
			}
			finally
			{
				_reset();
			}
		}

		/// <summary>
		/// Asserts that the subscription has completed within the configured <see cref="Timeout"/> since the last <see cref="_reset"/>
		/// </summary>
		public void ShouldHaveCompleted(TimeSpan? timeout = null)
		{
			try
			{
				if (!_completed.Wait(timeout ?? Timeout))
					Assert.True(false, $"subscription did not complete within {(timeout ?? Timeout).TotalSeconds} s!");
			}
			finally
			{
				_reset();
			}
		}

		/// <summary>
		/// Asserts that the subscription has completed within the configured <see cref="Timeout"/> since the last <see cref="_reset"/>
		/// </summary>
		public void ShouldHaveThrownError(Action<Exception> assertError = null, TimeSpan? timeout = null)
		{
			try
			{
				if (!_error.Wait(timeout ?? Timeout))
					Assert.True(false, $"subscription did not throw an error within {(timeout ?? Timeout).TotalSeconds} s!");

				assertError?.Invoke(Error);
			}
			finally
			{
				_reset();
			}
		}

		/// <summary>
		/// Resets the tester class. Should be called before triggering the potential update
		/// </summary>
		private void _reset()
		{
			//if (_completed.IsSet)
			//	throw new InvalidOperationException(
			//		"the subscription sequence has completed. this tester instance cannot be reused");

			LastPayload = default(T);
			_updateReceived.Reset();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_subscription?.Dispose();
		}
	}

	public static class ObservableExtensions
	{
		public static ObservableTester<T> SubscribeTester<T>(this IObservable<T> observable)
		{
			return new ObservableTester<T>(observable);
		}
	}
}
