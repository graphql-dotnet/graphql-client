using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GraphQL.Client.Tests.Common.Chat.Schema;

public interface IChat
{
    ConcurrentStack<Message> AllMessages { get; }

    Message AddMessage(Message message);

    MessageFrom Join(string userId);

    IObservable<Message> Messages(string user);
    IObservable<MessageFrom> UserJoined();

    Message AddMessage(ReceivedMessage message);
}

public class Chat : IChat
{
    private readonly ISubject<Message> _messageStream = new ReplaySubject<Message>(1);
    private readonly ISubject<MessageFrom> _userJoined = new Subject<MessageFrom>();

    public Chat()
    {
        AllMessages = new ConcurrentStack<Message>();
        Users = new ConcurrentDictionary<string, string>
        {
            ["1"] = "developer",
            ["2"] = "tester"
        };
    }

    public ConcurrentDictionary<string, string> Users { get; private set; }

    public ConcurrentStack<Message> AllMessages { get; private set; }

    public Message AddMessage(ReceivedMessage message)
    {
        if (!Users.TryGetValue(message.FromId, out var displayName))
        {
            displayName = "(unknown)";
        }

        return AddMessage(new Message
        {
            Content = message.Content,
            SentAt = message.SentAt,
            From = new MessageFrom
            {
                DisplayName = displayName,
                Id = message.FromId
            }
        });
    }

    public Message AddMessage(Message message)
    {
        AllMessages.Push(message);
        _messageStream.OnNext(message);
        return message;
    }

    public MessageFrom Join(string userId)
    {
        if (!Users.TryGetValue(userId, out var displayName))
        {
            displayName = "(unknown)";
        }

        var joinedUser = new MessageFrom
        {
            Id = userId,
            DisplayName = displayName
        };

        _userJoined.OnNext(joinedUser);
        return joinedUser;
    }

    public IObservable<Message> Messages(string user) =>
        Observable.Create<Message>(observer =>
        {
            Debug.WriteLine($"creating messages stream for user '{user}' on thread {Thread.CurrentThread.ManagedThreadId}");
            return new CompositeDisposable
            {
                _messageStream.Select(message =>
                    {
                        message.Sub = user;
                        return message;
                    })
                    .Subscribe(observer),
                Disposable.Create(() => Debug.WriteLine($"disposing messages stream for user '{user}' on thread {Thread.CurrentThread.ManagedThreadId}"))
            };
        });

    public void AddError(Exception exception) => _messageStream.OnError(exception);

    public IObservable<MessageFrom> UserJoined() => _userJoined.AsObservable();
}

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
}
