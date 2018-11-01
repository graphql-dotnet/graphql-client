using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace IntegrationTestServer.ChatSchema
{
    public interface IChat
    {
        ConcurrentStack<Message> AllMessages { get; }

        Message AddMessage(Message message);

        IObservable<Message> Messages(string user);

        Message AddMessage(ReceivedMessage message);
    }

    public class Chat : IChat
    {
        private readonly ISubject<Message> _messageStream = new ReplaySubject<Message>(1);

        public Chat()
        {
            AllMessages = new ConcurrentStack<Message>();
            Users = new ConcurrentDictionary<string, string>
            {
                ["1"] = "developer",
                ["2"] = "tester"
            };
        }

        public ConcurrentDictionary<string, string> Users { get; set; }

        public ConcurrentStack<Message> AllMessages { get; }

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

        public IObservable<Message> Messages(string user)
        {
            return _messageStream
                .Select(message =>
                {
                    message.Sub = user;
                    return message;
                })
                .AsObservable();
        }

        public void AddError(Exception exception)
        {
            _messageStream.OnError(exception);
        }
    }
}
