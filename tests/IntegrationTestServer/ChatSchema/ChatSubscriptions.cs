using System;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Claims;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Server.Transports.Subscriptions.Abstractions;
using GraphQL.Subscription;
using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema {
	public class ChatSubscriptions : ObjectGraphType<object> {
		private readonly IChat _chat;

		public ChatSubscriptions(IChat chat) {
			_chat = chat;
			AddField(new EventStreamFieldType {
				Name = "messageAdded",
				Type = typeof(MessageType),
				Resolver = new FuncFieldResolver<Message>(ResolveMessage),
				Subscriber = new EventStreamResolver<Message>(Subscribe)
			});

			AddField(new EventStreamFieldType {
				Name = "contentAdded",
				Type = typeof(MessageType),
				Resolver = new FuncFieldResolver<Message>(ResolveMessage),
				Subscriber = new EventStreamResolver<Message>(Subscribe)
			});

			AddField(new EventStreamFieldType {
				Name = "messageAddedByUser",
				Arguments = new QueryArguments(
					new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }
				),
				Type = typeof(MessageType),
				Resolver = new FuncFieldResolver<Message>(ResolveMessage),
				Subscriber = new EventStreamResolver<Message>(SubscribeById)
			});

			AddField(new EventStreamFieldType {
				Name = "userJoined",
				Type = typeof(MessageFromType),
				Resolver = new FuncFieldResolver<MessageFrom>(context => context.Source as MessageFrom),
				Subscriber = new EventStreamResolver<MessageFrom>(context => _chat.UserJoined())
			});


			AddField(new EventStreamFieldType {
				Name = "failImmediately",
				Type = typeof(MessageType),
				Resolver = new FuncFieldResolver<Message>(ResolveMessage),
				Subscriber = new EventStreamResolver<Message>(context => throw new NotSupportedException("this is supposed to fail"))
			});
		}

		private IObservable<Message> SubscribeById(ResolveEventStreamContext context) {
			var messageContext = context.UserContext.As<MessageHandlingContext>();
			var user = messageContext.Get<ClaimsPrincipal>("user");

			var sub = "Anonymous";
			if (user != null)
				sub = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

			var messages = _chat.Messages(sub);

			var id = context.GetArgument<string>("id");
			return messages.Where(message => message.From.Id == id);
		}

		private Message ResolveMessage(ResolveFieldContext context) {
			var message = context.Source as Message;

			return message;
		}

		private IObservable<Message> Subscribe(ResolveEventStreamContext context) {
			var messageContext = context.UserContext.As<MessageHandlingContext>();
			var user = messageContext.Get<ClaimsPrincipal>("user");

			var sub = "Anonymous";
			if (user != null)
				sub = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

			return _chat.Messages(sub);
		}
	}
}
