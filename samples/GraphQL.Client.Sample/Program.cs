using System;
using System.Threading;

namespace GraphQL.Client.Sample {

	class Program {

		static void Main(string[] args) {
			using (var graphQLClient = new GraphQLClient("http://localhost:60341/graphql")) {
				var subscriptionResult = graphQLClient.SubscribeAsync(@"subscription { messageAdded{content}}").Result;
				subscriptionResult.OnReceive += (res) => { Console.WriteLine(res.Data.messageAdded.content); };
				Console.ReadKey();
			}
		}

	}

}
