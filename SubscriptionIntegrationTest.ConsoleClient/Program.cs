using System;
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
			using (var client = new GraphQLHttpClient("http://localhost:5000/graphql/"))
			{

				Console.WriteLine("subscribing to message stream ...");

				var subscriptions = new CompositeDisposable();

				subscriptions.Add(CreateSubscription("1", client));
				await Task.Delay(1000);
				subscriptions.Add(CreateSubscription("2", client));
				await Task.Delay(1000);
				subscriptions.Add(CreateSubscription("3", client));
				await Task.Delay(1000);
				subscriptions.Add(CreateSubscription("4", client));
				await Task.Delay(1000);
				subscriptions.Add(CreateSubscription("5", client));
				await Task.Delay(1000);
				subscriptions.Add(CreateSubscription("6", client));
				await Task.Delay(1000);
				subscriptions.Add(CreateSubscription("7", client));
				await Task.Delay(1000);

				using (subscriptions)
				{
					Console.WriteLine("client setup complete");
					Console.WriteLine("press any key to exit");
					Console.Read();
					Console.WriteLine("shutting down ...");
				}
			}
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
			//{ Variables = new { id } }
			,
				e => Console.WriteLine($"WebSocketException: {e.Message} (WebSocketError {e.WebSocketErrorCode}, ErrorCode {e.ErrorCode}, NativeErrorCode {e.NativeErrorCode}"));
#pragma warning restore 618

			return stream.Subscribe(
				response => Console.WriteLine($"{id}: new message from \"{response.Data.messageAdded.from.displayName.Value}\": {response.Data.messageAdded.content.Value}"),
				exception => Console.WriteLine($"{id}: message subscription stream failed: {exception}"),
				() => Console.WriteLine($"{id}: message subscription stream completed"));
			
		}
	}
}
