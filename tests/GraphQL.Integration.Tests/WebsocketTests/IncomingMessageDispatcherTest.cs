using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http.Websocket;
using Xunit;

namespace GraphQL.Integration.Tests.WebsocketTests;

public class IncomingMessageDispatcherTest
{
    [Fact]
    public void RoutesMessageOnlyToMatchingRegistration()
    {
        using var source = new Subject<WebsocketMessageWrapper>();
        var dispatcher = new IncomingMessageDispatcher(source);
        var firstMessages = 0;
        var secondMessages = 0;
        using var first = dispatcher.Register("first", _ => firstMessages++, _ => { }, () => { });
        using var second = dispatcher.Register("second", _ => secondMessages++, _ => { }, () => { });

        source.OnNext(new WebsocketMessageWrapper { Id = "second" });

        firstMessages.Should().Be(0);
        secondMessages.Should().Be(1);
    }

    [Fact]
    public void IsolatesRegistrationCallbackFailure()
    {
        using var source = new Subject<WebsocketMessageWrapper>();
        var dispatcher = new IncomingMessageDispatcher(source);
        var expectedException = new InvalidOperationException("invalid payload");
        Exception? receivedException = null;
        var secondMessages = 0;
        using var first = dispatcher.Register("first", _ => throw expectedException, exception => receivedException = exception, () => { });
        using var second = dispatcher.Register("second", _ => secondMessages++, _ => { }, () => { });

        var dispatchFirst = () => source.OnNext(new WebsocketMessageWrapper { Id = "first" });
        dispatchFirst.Should().NotThrow();
        source.OnNext(new WebsocketMessageWrapper { Id = "second" });

        receivedException.Should().BeSameAs(expectedException);
        secondMessages.Should().Be(1);
    }

    [Fact]
    public void SupportsRegistrationAfterSynchronousCompletion()
    {
        var dispatcher = new IncomingMessageDispatcher(Observable.Empty<WebsocketMessageWrapper>());
        var completions = 0;

        using var first = dispatcher.Register("first", _ => { }, _ => { }, () => completions++);
        using var second = dispatcher.Register("second", _ => { }, _ => { }, () => completions++);

        completions.Should().Be(2);
    }

    [Fact]
    public void DoesNotInvokeSynchronousTerminalCallbackWhileHoldingDispatcherLock()
    {
        var dispatcher = new IncomingMessageDispatcher(Observable.Empty<WebsocketMessageWrapper>());
        IDisposable? nestedRegistration = null;

        using var first = dispatcher.Register("first", _ => { }, _ => { }, () =>
        {
            var registerNested = Task.Run(() => dispatcher.Register("second", _ => { }, _ => { }, () => { }));
            registerNested.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue();
            nestedRegistration = registerNested.Result;
        });

        nestedRegistration.Should().NotBeNull();
        nestedRegistration!.Dispose();
    }

    [Fact]
    public void RejectsDuplicateOperationId()
    {
        using var source = new Subject<WebsocketMessageWrapper>();
        var dispatcher = new IncomingMessageDispatcher(source);
        using var first = dispatcher.Register("duplicate", _ => { }, _ => { }, () => { });

        var registerDuplicate = () => dispatcher.Register("duplicate", _ => { }, _ => { }, () => { });

        registerDuplicate.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SharesSourceSubscriptionAndReestablishesItAfterLastRegistrationIsDisposed()
    {
        var subscriptions = 0;
        var disposals = 0;
        var source = Observable.Create<WebsocketMessageWrapper>(_ =>
        {
            subscriptions++;
            return Disposable.Create(() => disposals++);
        });
        var dispatcher = new IncomingMessageDispatcher(source);

        var first = dispatcher.Register("first", _ => { }, _ => { }, () => { });
        var second = dispatcher.Register("second", _ => { }, _ => { }, () => { });
        subscriptions.Should().Be(1);

        first.Dispose();
        disposals.Should().Be(0);
        second.Dispose();
        disposals.Should().Be(1);

        using var third = dispatcher.Register("third", _ => { }, _ => { }, () => { });
        subscriptions.Should().Be(2);
    }

    [Fact]
    public void IgnoresNotificationsFromDisposedSourceGeneration()
    {
        var source = new RetainingObservable();
        var dispatcher = new IncomingMessageDispatcher(source);
        var first = dispatcher.Register("first", _ => { }, _ => { }, () => { });
        first.Dispose();

        var messages = 0;
        var completions = 0;
        using var second = dispatcher.Register("second", _ => messages++, _ => { }, () => completions++);

        source.Observers[0].OnNext(new WebsocketMessageWrapper { Id = "second" });
        source.Observers[0].OnCompleted();
        source.Observers[1].OnNext(new WebsocketMessageWrapper { Id = "second" });

        messages.Should().Be(1);
        completions.Should().Be(0);
    }

    private sealed class RetainingObservable : IObservable<WebsocketMessageWrapper>
    {
        public List<IObserver<WebsocketMessageWrapper>> Observers { get; } = new();

        public IDisposable Subscribe(IObserver<WebsocketMessageWrapper> observer)
        {
            Observers.Add(observer);
            return Disposable.Empty;
        }
    }
}
