using System;
using GraphQL.Client.Http;
using GraphQL.Common.Request;

namespace GraphQL.Client.Sample {

	public class Program {

		public static void Main(string[] args) {
			using (var graphQLHttpClient = new GraphQLHttpClient("http://localhost:60341/graphql")) {
				var subscription = graphQLHttpClient.CreateSubscriptionStream(new GraphQLRequest(@"subscription { messageAdded{content}}"));
				subscription.Subscribe(res => Console.WriteLine(res.Data.messageAdded.content));
			}
		}

	}

}
