using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema {
	public class ChatMutation : ObjectGraphType<object> {
		public ChatMutation(IChat chat) {
			Field<MessageType>("addMessage",
				arguments: new QueryArguments(
					new QueryArgument<MessageInputType> { Name = "message" }
				),
				resolve: context => {
					var receivedMessage = context.GetArgument<ReceivedMessage>("message");
					var message = chat.AddMessage(receivedMessage);
					return message;
				});

			Field<MessageFromType>("join",
				arguments: new QueryArguments(
					new QueryArgument<StringGraphType> { Name = "userId" }
				),
				resolve: context => {
					var userId = context.GetArgument<string>("userId");
					var userJoined = chat.Join(userId);
					return userJoined;
				});
		}
	}

	public class MessageInputType : InputObjectGraphType {
		public MessageInputType() {
			Field<StringGraphType>("fromId");
			Field<StringGraphType>("content");
			Field<DateGraphType>("sentAt");
		}
	}
}
