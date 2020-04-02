using System;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client.Tests.Common.FluentAssertions.Reactive
{
    internal static class NoSynchronizationContextScope
    {
        public static T ExecuteInDefaultSynchronizationContext<T>(this Task<T> task)
        {
            using (NoSynchronizationContextScope.Enter())
            {
                task.Wait();
                return task.Result;
            }
        }

        public static DisposingAction Enter()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            return new DisposingAction(() => SynchronizationContext.SetSynchronizationContext(context));
        }

        internal class DisposingAction : IDisposable
        {
            private readonly Action action;

            public DisposingAction(Action action)
            {
                this.action = action;
            }

            public void Dispose()
            {
                action();
            }
        }
    }
}
