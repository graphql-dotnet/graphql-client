using System;
using System.Threading.Tasks;

namespace GraphQL.Client.Tests.Common.Helpers {

	public class ConcurrentTaskWrapper {
		public static ConcurrentTaskWrapper<TResult> New<TResult>(Func<Task<TResult>> createTask) {
			return  new ConcurrentTaskWrapper<TResult>(createTask);
		}

		private readonly Func<Task> createTask;
		private Task internalTask = null;

		public ConcurrentTaskWrapper(Func<Task> createTask) {
			this.createTask = createTask;
		}

		public Task Invoke() {
			if (internalTask != null)
				return internalTask;

			return internalTask = createTask();
		}
	}

	public class ConcurrentTaskWrapper<TResult> {
		private readonly Func<Task<TResult>> createTask;
		private Task<TResult> internalTask = null;

		public ConcurrentTaskWrapper(Func<Task<TResult>> createTask) {
			this.createTask = createTask;
		}

		public Task<TResult> Invoke() {
			if (internalTask != null)
				return internalTask;

			return internalTask = createTask();
		}

		public void Start() {
			if (internalTask == null)
				internalTask = createTask();
		}

		public Func<Task<TResult>> Invoking() {
			return Invoke;
		}

		public void Clear() {
			internalTask = null;
		}
	}
}
