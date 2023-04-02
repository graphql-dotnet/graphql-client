namespace GraphQL.Client.Tests.Common.Helpers;

public class ConcurrentTaskWrapper
{
    public static ConcurrentTaskWrapper<TResult> New<TResult>(Func<Task<TResult>> createTask) => new(createTask);

    private readonly Func<Task> _createTask;
    private Task _internalTask;

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
    private Task<TResult> _internalTask;

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

    public void Start() => _internalTask ??= _createTask();

    public Func<Task<TResult>> Invoking() => Invoke;

    public void Clear() => _internalTask = null;
}
