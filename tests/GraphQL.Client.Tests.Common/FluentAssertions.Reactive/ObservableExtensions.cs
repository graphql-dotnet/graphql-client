using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Reactive.Testing;

namespace GraphQL.Client.Tests.Common.FluentAssertions.Reactive
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Create a new <see cref="FluentTestObserver{TPayload}"/> subscribed to this <paramref name="observable"/>
        /// </summary>
        public static FluentTestObserver<T> Observe<T>(this IObservable<T> observable) => new FluentTestObserver<T>(observable);

        /// <summary>
        /// Asserts that the recorded messages contain at lease one item which matches the <paramref name="predicate"/>
        /// </summary>
        public static AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> WithMessage<TPayload>(
            this AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> recorderConstraint, Expression<Func<TPayload, bool>> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            
            var compiledPredicate = predicate.Compile();
            bool match = recorderConstraint.GetMessages().Any(compiledPredicate);
            
            Execute.Assertion
                .ForCondition(match)
                .FailWith("Expected at least one message from {0} to match {1}, but found none.", recorderConstraint.And.Subject, predicate.Body);

            return recorderConstraint;
        }

        /// <summary>
        /// Asserts that the last recorded message matches the <paramref name="predicate"/>
        /// </summary>
        public static AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> WithLastMessage<TPayload>(
            this AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>> recorderConstraint, Expression<Func<TPayload, bool>> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            bool match = predicate.Compile().Invoke(recorderConstraint.GetLastMessage());

            Execute.Assertion
                .ForCondition(match)
                .FailWith("Expected the last message from {0} to match {1}, but it did not.", recorderConstraint.And.Subject, predicate.Body);
            
            return recorderConstraint;
        }

        /// <summary>
        /// Extracts the last recorded message
        /// </summary>
        public static TPayload GetLastMessage<TPayload>(
            this AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>>
                recorderConstraint) =>
            recorderConstraint.GetMessages().LastOrDefault();

        /// <summary>
        /// Extracts the recorded messages
        /// </summary>
        public static IEnumerable<TPayload> GetMessages<TPayload>(
            this AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>>
            recorderConstraint) => recorderConstraint.Subject.GetMessages();

        /// <summary>
        /// Extracts the recorded messages from a number od recorded notifications
        /// </summary>
        public static IEnumerable<TPayload> GetMessages<TPayload>(
            this IEnumerable<Recorded<Notification<TPayload>>> recordedNotifications) => recordedNotifications
            .Where(r => r.Value.Kind == NotificationKind.OnNext)
            .Select(recorded => recorded.Value.Value);


        /// <summary>
        /// Clears the recorded notifications on the underlying <see cref="FluentTestObserver{TPayload}"/>
        /// </summary>
        public static void Clear<TPayload>(
            this AndWhichConstraint<ObservableAssertions<TPayload>, IEnumerable<Recorded<Notification<TPayload>>>>
                recorderConstraint) => recorderConstraint.And.Observer.Clear();
    }
}
