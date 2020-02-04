using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema {
	public class MessageFromType : ObjectGraphType<MessageFrom> {
		public MessageFromType() {
			Field(o => o.Id);
			Field(o => o.DisplayName);
		}
	}
}
