using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema {
	public class ChatQuery : ObjectGraphType {

		public static readonly Dictionary<string, object> TestExtensions = new Dictionary<string, object> {
			{"extension1", "hello world"},
			{"another extension", 4711}
		};

		public ChatQuery(IChat chat) {
			Name = "ChatQuery";

			Field<ListGraphType<MessageType>>("messages", resolve: context => chat.AllMessages.Take(100));

			Field<StringGraphType>()
				.Name("extensionsTest")
				.Resolve(context => {
					context.Errors.Add(new ExecutionError("this error contains extension fields", TestExtensions));
					return null;
				});
		}
	}
}
