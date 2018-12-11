using System;
using GraphQL.Client.Http;
using GraphQL.Common.Request;

namespace SubsccriptionIntegrationTest.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("configuring client ...");
			using (var client = new GraphQLHttpClient("http://localhost:5000/graphql/"))
			{
#pragma warning disable 618
				var stream = client.CreateSubscriptionStream(new GraphQLRequest
					{
						Query = @"
							subscription {
							  messageAdded{
							    content
								from {
								  displayName
								}
							  }
							}"
					},
					e => Console.WriteLine($"WebSocketException: {e.Message} (WebSocketError {e.WebSocketErrorCode}, ErrorCode {e.ErrorCode}, NativeErrorCode {e.NativeErrorCode}"));
#pragma warning restore 618

				Console.WriteLine("subscribing to message stream ...");
				using (var subscription = stream.Subscribe(
					response => Console.WriteLine($"new message from \"{response.Data.messageAdded.from.displayName.Value}\": {response.Data.messageAdded.content.Value}"),
					exception => Console.WriteLine($"message subscription stream failed: {exception}"),
					() => Console.WriteLine($"message subscription stream completed")))
				{
					Console.WriteLine("client setup complete");
					Console.WriteLine("press any key to exit");
					Console.Read();
					Console.WriteLine("shutting down ...");
				}
			}
		}
	}
}
