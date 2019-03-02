using System;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Common.Request;

namespace SubsccriptionIntegrationTest.ConsoleClient
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("configuring client ...");
			using (var client = new GraphQLHttpClient("http://localhost:5000/graphql/", new GraphQLHttpClientOptions{ UseWebSocketForQueriesAndMutations = true }))
			{

				Console.WriteLine("subscribing to message stream ...");

				var subscriptions = new CompositeDisposable();

				subscriptions.Add(client.WebSocketReceiveErrors.Subscribe(e => {
					if(e is WebSocketException we)
						Console.WriteLine($"WebSocketException: {we.Message} (WebSocketError {we.WebSocketErrorCode}, ErrorCode {we.ErrorCode}, NativeErrorCode {we.NativeErrorCode}");
					else
						Console.WriteLine($"Exception in websocket receive stream: {e.ToString()}");
				}));

				subscriptions.Add(CreateSubscription("1", client));
				await Task.Delay(200);
				subscriptions.Add(CreateSubscription2("2", client));
				await Task.Delay(200);
				subscriptions.Add(CreateSubscription("3", client));
				await Task.Delay(200);
				subscriptions.Add(CreateSubscription("4", client));
				await Task.Delay(200);
				subscriptions.Add(CreateSubscription("5", client));
				await Task.Delay(200);
				subscriptions.Add(CreateSubscription("6", client));
				await Task.Delay(200);
				subscriptions.Add(CreateSubscription("7", client));

				using (subscriptions)
				{
					Console.WriteLine("client setup complete");
					var quit = false;
					do
					{
						Console.WriteLine("write message and press enter...");
						var message = Console.ReadLine();
						var graphQLRequest = new GraphQLRequest(@"
							mutation($input: MessageInputType){
							  addMessage(message: $input){
								content
							  }
							}")
						{
							Variables = new
							{
								input = new
								{
									fromId = "2",
									content = message,
									sentAt = DateTime.Now
								}
							}
						};
						var result = await client.SendMutationAsync(graphQLRequest).ConfigureAwait(false);

						if(result.Errors != null && result.Errors.Length > 0)
						{
							Console.WriteLine($"request returned {result.Errors.Length} errors:");
							foreach (var item in result.Errors)
							{
								Console.WriteLine($"{item.Message}");
							}
						}
					}
					while(!quit);
					Console.WriteLine("shutting down ...");
				}
				Console.WriteLine("subscriptions disposed ...");
			}
			Console.WriteLine("client disposed ...");
		}

		private static IDisposable CreateSubscription(string id, GraphQLHttpClient client)
		{
#pragma warning disable 618
			var stream = client.CreateSubscriptionStream(new GraphQLRequest(@"
					subscription {
						messageAdded{
						content
							from {
								displayName
							}
						}
					}"
				)
			{ Variables = new { id } });
#pragma warning restore 618

			return stream.Subscribe(
				response => Console.WriteLine($"{id}: new message from \"{response.Data.messageAdded.from.displayName.Value}\": {response.Data.messageAdded.content.Value}"),
				exception => Console.WriteLine($"{id}: message subscription stream failed: {exception}"),
				() => Console.WriteLine($"{id}: message subscription stream completed"));
			
		}


		private static IDisposable CreateSubscription2(string id, GraphQLHttpClient client)
		{
#pragma warning disable 618
			var stream = client.CreateSubscriptionStream(new GraphQLRequest(@"
					subscription {
						contentAdded{
						content
							from {
								displayName
							}
						}
					}"
				)
			{ Variables = new { id } });
#pragma warning restore 618

			return stream.Subscribe(
				response => Console.WriteLine($"{id}: new content from \"{response.Data.contentAdded.from.displayName.Value}\": {response.Data.contentAdded.content.Value}"),
				exception => Console.WriteLine($"{id}: content subscription stream failed: {exception}"),
				() => Console.WriteLine($"{id}: content subscription stream completed"));

		}
	}
}
