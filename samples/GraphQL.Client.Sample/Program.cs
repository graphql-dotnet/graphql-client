using System;
using GraphQL.Client.Http;

namespace GraphQL.Client.Sample {

	public class Program {

		public static void Main(string[] args) {
			using (var graphQLHttpClient = new GraphQLHttpClient("http://localhost:60341/graphql")) {
				var subscriptionResult = graphQLHttpClient.SendSubscribeAsync(@"subscription { messageAdded{content}}").Result;
				subscriptionResult.OnReceive += (res) => { Console.WriteLine(res.Data.messageAdded.content); };
				Console.ReadKey();
			}
		}

	}

}
