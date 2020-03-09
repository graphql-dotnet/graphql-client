using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphQL.Types;

namespace GraphQL.Client.Tests.Common.Chat.Schema {
	public class ChatQuery : ObjectGraphType {

		public static readonly Dictionary<string, object> TestExtensions = new Dictionary<string, object> {
			{"extension1", "hello world"},
			{"another extension", 4711}
		};

		// properties for unit testing

		public readonly ManualResetEventSlim LongRunningQueryBlocker = new ManualResetEventSlim();
		public readonly ManualResetEventSlim WaitingOnQueryBlocker = new ManualResetEventSlim();


		public ChatQuery(IChat chat) {
			Name = "ChatQuery";

			Field<ListGraphType<MessageType>>("messages", resolve: context => chat.AllMessages.Take(100));

			Field<StringGraphType>()
				.Name("extensionsTest")
				.Resolve(context => {
					context.Errors.Add(new ExecutionError("this error contains extension fields", TestExtensions));
					return null;
				});

			Field<StringGraphType>()
				.Name("longRunning")
				.Resolve(context => {
					WaitingOnQueryBlocker.Set();
					LongRunningQueryBlocker.Wait();
					WaitingOnQueryBlocker.Reset();
					return "finally returned";
				});
		}
	}
}
