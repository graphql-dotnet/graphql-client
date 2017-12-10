using GraphQL.Common.Request;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientTests : BaseGraphQLClientTest {

		[Fact]
		public async void GetAsyncFact() {
			var graphQLRequest = new GraphQLRequest { Query = @"
				{
					person(personID: ""1"") {
						name
					}
				}"
			};
			var response=await this.GraphQLClient.GetAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
		}

	}

}
