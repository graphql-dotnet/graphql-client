using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GraphQL.Client.Tests.Common.Chat.Schema {
	public interface IChat {
		ConcurrentStack<Message> AllMessages { get; }

		Message AddMessage(Message message);

		MessageFrom Join(string userId);

		IObservable<Message> Messages(string user);
		IObservable<MessageFrom> UserJoined();

		Message AddMessage(ReceivedMessage message);
	}

	public class Chat : IChat {
		private readonly RollingReplaySubject<Message> _messageStream = new RollingReplaySubject<Message>();
		private readonly ISubject<MessageFrom> _userJoined = new Subject<MessageFrom>();

		public Chat() {
			Reset();
		}

		public void Reset() {
			AllMessages = new ConcurrentStack<Message>();
			Users = new ConcurrentDictionary<string, string> {
				["1"] = "developer",
				["2"] = "tester"
			};
			_messageStream.Clear();
		}

		public ConcurrentDictionary<string, string> Users { get; private set; }

		public ConcurrentStack<Message> AllMessages { get; private set; }

		public Message AddMessage(ReceivedMessage message) {
			if (!Users.TryGetValue(message.FromId, out var displayName)) {
				displayName = "(unknown)";
			}

			return AddMessage(new Message {
				Content = message.Content,
				SentAt = message.SentAt,
				From = new MessageFrom {
					DisplayName = displayName,
					Id = message.FromId
				}
			});
		}

		public Message AddMessage(Message message) {
			AllMessages.Push(message);
			_messageStream.OnNext(message);
			return message;
		}

		public MessageFrom Join(string userId) {
			if (!Users.TryGetValue(userId, out var displayName)) {
				displayName = "(unknown)";
			}

			var joinedUser = new MessageFrom {
				Id = userId,
				DisplayName = displayName
			};

			_userJoined.OnNext(joinedUser);
			return joinedUser;
		}

		public IObservable<Message> Messages(string user) {
			return _messageStream
				.Select(message => {
					message.Sub = user;
					return message;
				})
				.AsObservable();
		}

		public void AddError(Exception exception) {
			_messageStream.OnError(exception);
		}

		public IObservable<MessageFrom> UserJoined() {
			return _userJoined.AsObservable();
		}
	}

	public class User {
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class RollingReplaySubject<T> : ISubject<T> {
		private readonly ReplaySubject<IObservable<T>> _subjects;
		private readonly IObservable<T> _concatenatedSubjects;
		private ISubject<T> _currentSubject;

		public RollingReplaySubject() {
			_subjects = new ReplaySubject<IObservable<T>>(1);
			_concatenatedSubjects = _subjects.Concat();
			_currentSubject = new ReplaySubject<T>();
			_subjects.OnNext(_currentSubject);
		}

		public void Clear() {
			_currentSubject.OnCompleted();
			_currentSubject = new ReplaySubject<T>();
			_subjects.OnNext(_currentSubject);
		}

		public void OnNext(T value) {
			_currentSubject.OnNext(value);
		}

		public void OnError(Exception error) {
			_currentSubject.OnError(error);
		}

		public void OnCompleted() {
			_currentSubject.OnCompleted();
			_subjects.OnCompleted();
			// a quick way to make the current ReplaySubject unreachable
			// except to in-flight observers, and not hold up collection
			_currentSubject = new Subject<T>();
		}

		public IDisposable Subscribe(IObserver<T> observer) {
			return _concatenatedSubjects.Subscribe(observer);
		}
	}
}
