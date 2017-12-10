using GraphQL.Common.Request;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientPostTests : BaseGraphQLClientTest {

		[Fact]
		public async void PostAsyncFact() {
			var graphQLRequest = new GraphQLRequest { Query = @"
				{
					person(personID: ""1"") {
						name
					}
				}"
			};
			var response=await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
		}

	}

}
