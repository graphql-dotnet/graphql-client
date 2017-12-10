using GraphQL.Common.Request;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests {

	public class GraphQLClientPostTests : BaseGraphQLClientTest {

		[Fact]
		public async void QueryPostAsyncFact() {
			var graphQLRequest = new GraphQLRequest { Query = @"
				{
					person(personID: ""1"") {
						name
					}
				}"
			};
			var response=await this.GraphQLClient.PostAsync(graphQLRequest).ConfigureAwait(false);

			Assert.Equal("Luke Skywalker", response.Data.person.name.Value);
			Assert.Equal("Luke Skywalker", response.GetDataFieldAs<Person>("person").Name);
		}

	}

}
