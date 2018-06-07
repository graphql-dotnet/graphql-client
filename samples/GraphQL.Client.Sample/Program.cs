using System;

namespace GraphQL.Client.Sample {

	class Program {

		static void Main(string[] args) {
			using (var graphQLClient = new GraphQLClient("http://localhost:60341/graphql")) {
				var subscriptionResult = graphQLClient.SendSubscribeAsync(@"subscription { messageAdded{content}}").Result as GraphQL.Client.Http.GraphQLHttpSubscriptionResult;
				subscriptionResult.OnReceive += (res) => { Console.WriteLine(res.Data.messageAdded.content); };
				Console.ReadKey();
			}
		}

	}

}
