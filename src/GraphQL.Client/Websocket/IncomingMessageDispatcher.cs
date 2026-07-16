using System.Diagnostics;
using System.Reactive.Disposables;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Http.Websocket;

/// <summary>
/// Routes messages from the single, shared incoming websocket message stream to the
/// individual GraphQL subscription that requested them, keyed by request id.
/// </summary>
/// <remarks>
/// Without this, every active GraphQL subscription (<see cref="GraphQLTransportWSProtocolHandler.CreateSubscriptionObservable{TResponse}"/>)
/// independently subscribes to the shared incoming message stream and filters it with its
/// own <c>Where(response.Id == myId)</c> predicate. Every incoming message is then broadcast
/// to, and evaluated by, all N active subscriptions instead of only the one that actually
/// wants it, so the per-message cost grows with the number of concurrent subscriptions
/// (observed to scale worse than linearly, since <see cref="System.Reactive.Subjects.Subject{T}"/>
/// dispatch, safeguarding and Where-filtering overhead is paid once per subscriber for every
/// message). This class keeps exactly one subscription to the shared stream alive (for as
/// long as at least one subscriber is registered) and dispatches each message directly to its
/// owner via a dictionary lookup, so the per-message cost no longer depends on the number of
/// concurrent subscriptions.
/// </remarks>
internal sealed class IncomingMessageDispatcher
{
    private sealed class Registration
    {
        public Registration(Action<WebsocketMessageWrapper> onNext, Action<Exception> onError, Action onCompleted)
        {
            OnNext = onNext;
            OnError = onError;
            OnCompleted = onCompleted;
        }

        public Action<WebsocketMessageWrapper> OnNext { get; }
        public Action<Exception> OnError { get; }
        public Action OnCompleted { get; }
    }

    private readonly IObservable<WebsocketMessageWrapper> _incomingMessages;
    private readonly object _gate = new();
    private readonly Dictionary<string, Registration> _registrations = new();
    private IDisposable? _subscription;
    private bool _sourceTransitionInProgress;
    private int _sourceGeneration;

    public IncomingMessageDispatcher(IObservable<WebsocketMessageWrapper> incomingMessages)
    {
        _incomingMessages = incomingMessages;
    }

    /// <summary>
    /// Registers a subscriber for messages carrying the given <paramref name="id"/>.
    /// Lazily (re-)establishes the single shared subscription to the incoming message
    /// stream if this is the first active registration.
    /// </summary>
    public IDisposable Register(string id, Action<WebsocketMessageWrapper> onNext, Action<Exception> onError, Action onCompleted)
    {
        var registration = new Registration(onNext, onError, onCompleted);
        lock (_gate)
        {
            if (_registrations.ContainsKey(id))
                throw new InvalidOperationException($"A websocket operation with id '{id}' is already registered.");

            _registrations.Add(id, registration);
        }

        EnsureSubscribed();
        return Disposable.Create(() => Unregister(id, registration));
    }

    private void EnsureSubscribed()
    {
        while (true)
        {
            int generation;
            lock (_gate)
            {
                if (_subscription != null || _sourceTransitionInProgress || _registrations.Count == 0)
                    return;

                _sourceTransitionInProgress = true;
                generation = _sourceGeneration;
            }

            IDisposable subscription;
            try
            {
                subscription = _incomingMessages.Subscribe(
                    message => Dispatch(generation, message),
                    exception => DispatchError(generation, exception),
                    () => DispatchCompleted(generation));
            }
            catch (Exception exception)
            {
                DispatchError(generation, exception);
                lock (_gate)
                    _sourceTransitionInProgress = false;

                EnsureSubscribed();
                return;
            }

            var keepSubscription = false;
            lock (_gate)
            {
                if (generation == _sourceGeneration && _subscription == null && _registrations.Count > 0)
                {
                    _subscription = subscription;
                    keepSubscription = true;
                    _sourceTransitionInProgress = false;
                }
            }

            if (keepSubscription)
                return;

            try
            {
                subscription.Dispose();
            }
            finally
            {
                lock (_gate)
                    _sourceTransitionInProgress = false;
            }
        }
    }

    private void Unregister(string id, Registration registration)
    {
        IDisposable? subscription = null;
        lock (_gate)
        {
            if (_registrations.TryGetValue(id, out var currentRegistration) &&
                ReferenceEquals(currentRegistration, registration))
                _registrations.Remove(id);

            if (_registrations.Count == 0 && !_sourceTransitionInProgress && _subscription != null)
            {
                _sourceTransitionInProgress = true;
                subscription = _subscription;
                _subscription = null;
                _sourceGeneration++;
            }
        }

        if (subscription == null)
            return;

        try
        {
            subscription.Dispose();
        }
        finally
        {
            lock (_gate)
                _sourceTransitionInProgress = false;

            EnsureSubscribed();
        }
    }

    private void Dispatch(int generation, WebsocketMessageWrapper? message)
    {
        if (message?.Id == null)
            return;

        Registration? registration;
        lock (_gate)
        {
            if (generation != _sourceGeneration)
                return;

            _registrations.TryGetValue(message.Id, out registration);
        }

        if (registration == null)
            return;

        try
        {
            registration.OnNext(message);
        }
        catch (Exception exception)
        {
            Unregister(message.Id, registration);
            NotifySafely(() => registration.OnError(exception));
        }
    }

    private void DispatchError(int generation, Exception exception)
    {
        foreach (var registration in DrainRegistrations(generation))
            NotifySafely(() => registration.OnError(exception));
    }

    private void DispatchCompleted(int generation)
    {
        foreach (var registration in DrainRegistrations(generation))
            NotifySafely(registration.OnCompleted);
    }

    /// <summary>
    /// Atomically takes ownership of all currently registered subscribers and clears the
    /// shared subscription, so that terminal notifications (error/completed) from the
    /// underlying stream are only ever delivered once per subscriber.
    /// </summary>
    private Registration[] DrainRegistrations(int generation)
    {
        lock (_gate)
        {
            if (generation != _sourceGeneration)
                return [];

            var registrations = new Registration[_registrations.Count];
            _registrations.Values.CopyTo(registrations, 0);
            _registrations.Clear();
            _subscription = null;
            _sourceGeneration++;
            return registrations;
        }
    }

    private static void NotifySafely(Action notification)
    {
        try
        {
            notification();
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"websocket operation observer threw while handling a notification: {exception}");
        }
    }
}
