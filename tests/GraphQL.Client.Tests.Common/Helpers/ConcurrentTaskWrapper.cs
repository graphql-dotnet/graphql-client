namespace GraphQL.Client.Tests.Common.Helpers
{
    public class ConcurrentTaskWrapper
    {
        public static ConcurrentTaskWrapper<TResult> New<TResult>(Func<Task<TResult>> createTask) => new ConcurrentTaskWrapper<TResult>(createTask);

        private readonly Func<Task> _createTask;
        private Task _internalTask = null;

        public ConcurrentTaskWrapper(Func<Task> createTask)
        {
            _createTask = createTask;
        }

        public Task Invoke()
        {
            if (_internalTask != null)
                return _internalTask;

            return _internalTask = _createTask();
        }
    }

    public class ConcurrentTaskWrapper<TResult>
    {
        private readonly Func<Task<TResult>> _createTask;
        private Task<TResult> _internalTask = null;

        public ConcurrentTaskWrapper(Func<Task<TResult>> createTask)
        {
            _createTask = createTask;
        }

        public Task<TResult> Invoke()
        {
            if (_internalTask != null)
                return _internalTask;

            return _internalTask = _createTask();
        }

        public void Start()
        {
            if (_internalTask == null)
                _internalTask = _createTask();
        }

        public Func<Task<TResult>> Invoking() => Invoke;

        public void Clear() => _internalTask = null;
    }
}
