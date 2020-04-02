using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.Reactive.Testing;

namespace GraphQL.Client.Tests.Common.FluentAssertions.Reactive
{
    /// <summary>
    /// Provides methods to assert an <see cref="IObservable{T}"/> observed by a <see cref="FluentTestObserver{TPayload}"/>
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class ObservableAssertions<TPayload> : ReferenceTypeAssertions<IObservable<TPayload>, ObservableAssertions<TPayload>>
    {
        public FluentTestObserver<TPayload> Observer { get; }

        protected internal ObservableAssertions(FluentTestObserver<TPayload> observer): base(observer.Subject)
        {
            Observer = observer;
        }

        protected override string Identifier => "Subscription";
        
        /// <summary>
        /// Asserts that at least <paramref name="numberOfNotifications"/> notifications were pushed to the <see cref="FluentTestObserver{TPayload}"/> within the specified <paramref name="timeout"/>.<br />
        /// This includes any previously recorded notifications since it has been created or cleared. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> Push(int numberOfNotifications, TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {
            var notifications = Observer.RecordedNotificationStream
                .Where(recorded => recorded.Value.Kind == NotificationKind.OnNext)
                .Take(numberOfNotifications)
                .Timeout(timeout)
                .Catch(Observable.Empty<Recorded<Notification<TPayload>>>())
                .ToList()
                .ToTask()
                .ExecuteInDefaultSynchronizationContext();
            
            Execute.Assertion
                .ForCondition(notifications.Any())
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {0} to push at least one notification within {1}{reason}, but it did not.", Observer.Subject, timeout);

            return new AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>>(this, notifications);
        }

        /// <summary>
        /// Asserts that at least <paramref name="numberOfNotifications"/> notifications are pushed to the <see cref="FluentTestObserver{TPayload}"/> within the next 1 second.<br />
        /// This includes any previously recorded notifications since it has been created or cleared. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> Push(int numberOfNotifications, string because = "", params object[] becauseArgs)
            => Push(numberOfNotifications, TimeSpan.FromSeconds(1), because, becauseArgs);

        /// <summary>
        /// Asserts that at least 1 notification is pushed to the <see cref="FluentTestObserver{TPayload}"/> within the next 1 second.<br />
        /// This includes any previously recorded notifications since it has been created or cleared. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> Push(string because = "", params object[] becauseArgs)
            => Push(1, TimeSpan.FromSeconds(1), because, becauseArgs);


        /// <summary>
        /// Asserts that the <see cref="FluentTestObserver{TPayload}"/> does not receive any notifications within the specified <paramref name="timeout"/>.<br />
        /// This includes any previously recorded notifications since it has been created or cleared. 
        /// </summary>
        public AndConstraint<ObservableAssertions<TPayload>> NotPush(TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {
            bool anyNotifications = Observer.RecordedNotificationStream
                .Any(recorded => recorded.Value.Kind == NotificationKind.OnNext)
                .Timeout(timeout)
                .Catch(Observable.Return(false))
                .ToTask()
                .ExecuteInDefaultSynchronizationContext();

            Execute.Assertion
                .ForCondition(!anyNotifications)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {0} to not push any notifications{reason}, but it did.", Observer.Subject);
            
            return new AndConstraint<ObservableAssertions<TPayload>>(this);
        }

        /// <summary>
        /// Asserts that the <see cref="FluentTestObserver{TPayload}"/> does not receive any notifications within the next 100 milliseconds.<br />
        /// This includes any previously recorded notifications since it has been created or last cleared. 
        /// </summary>
        public AndConstraint<ObservableAssertions<TPayload>> NotPush(string because = "", params object[] becauseArgs)
            => NotPush(TimeSpan.FromMilliseconds(100), because, becauseArgs);


        /// <summary>
        /// Asserts that the <see cref="IObservable{T}"/> observed by the <see cref="FluentTestObserver{TPayload}"/> fails within the specified <paramref name="timeout"/>. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, Exception> Fail(TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {
            var exception = Observer.RecordedNotificationStream
                .Timeout(timeout)
                .Catch(Observable.Empty<Recorded<Notification<TPayload>>>())
                .FirstOrDefaultAsync(recorded => recorded.Value.Kind == NotificationKind.OnError)
                .Select(recorded => recorded.Value.Exception)
                .ToTask()
                .ExecuteInDefaultSynchronizationContext();

            Execute.Assertion
                .ForCondition(exception != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {0} to fail within {1}{reason}, but it did not.", Observer.Subject, timeout);
            
            return new AndWhichConstraint<ObservableAssertions<TPayload>, Exception>(this, exception);
        }

        /// <summary>
        /// Asserts that the <see cref="IObservable{T}"/> observed by the <see cref="FluentTestObserver{TPayload}"/> fails within the next 1 second. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, Exception> Fail(string because = "", params object[] becauseArgs)
            => Fail(TimeSpan.FromSeconds(1), because, becauseArgs);


        /// <summary>
        /// Asserts that the <see cref="IObservable{T}"/> observed by the <see cref="FluentTestObserver{TPayload}"/> completes within the specified <paramref name="timeout"/>. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> Complete(TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {

            bool completed = Observer.RecordedNotificationStream
                .Any(recorded => recorded.Value.Kind == NotificationKind.OnCompleted)
                .Timeout(timeout)
                .Catch(Observable.Return(false))
                .ToTask()
                .ExecuteInDefaultSynchronizationContext();
            
            Execute.Assertion
                .ForCondition(completed)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {0} to complete within {1}{reason}, but it did not.", Observer.Subject, timeout);

            return new AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>>(this, Observer.RecordedNotifications);
        }

        /// <summary>
        /// Asserts that the <see cref="IObservable{T}"/> observed by the <see cref="FluentTestObserver{TPayload}"/> completes within the next 1 second. 
        /// </summary>
        public AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> Complete(string because = "", params object[] becauseArgs)
            => Complete(TimeSpan.FromSeconds(1), because, becauseArgs);


        /// <summary>
        /// Asserts that the <see cref="IObservable{T}"/> observed by the <see cref="FluentTestObserver{TPayload}"/> does not complete within the specified <paramref name="timeout"/>. 
        /// </summary>
        public AndConstraint<ObservableAssertions<TPayload>> NotComplete(TimeSpan timeout,
            string because = "", params object[] becauseArgs)
        {
            bool completed = Observer.RecordedNotificationStream
                .Any(recorded => recorded.Value.Kind == NotificationKind.OnCompleted)
                .Timeout(timeout)
                .Catch(Observable.Return(false))
                .ToTask()
                .ExecuteInDefaultSynchronizationContext();

            Execute.Assertion
                .ForCondition(!completed)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {0} to not complete{reason}, but it did.", Observer.Subject);
            
            return new AndConstraint<ObservableAssertions<TPayload>>(this);
        }

        /// <summary>
        /// Asserts that the <see cref="IObservable{T}"/> observed by the <see cref="FluentTestObserver{TPayload}"/> does not complete within the next 100 milliseconds. 
        /// </summary>
        public AndConstraint<ObservableAssertions<TPayload>> NotComplete(string because = "", params object[] becauseArgs)
            => NotComplete(TimeSpan.FromMilliseconds(100), because, becauseArgs);
    }
}
