using GraphQL.Common.Request;
using GraphQL.Common.Tests.Model;
using Xunit;

namespace GraphQL.Client.Tests {

	public class IGraphQLClientTest : BaseGraphQLClientTest {

		[Fact]
		public async void QueryGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				{
					repository(owner: ""graphql-dotnet"", name: ""graphql-client"") {
						name
					}
				}"
			);
			var response = await this.GraphQLClient.SendQueryAsync(graphQLRequest);
			Assert.Equal("graphql-client", response.Data.repository.name.Value);
		}

		[Fact]
		public async void OperationNameGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				query Repository{
					repository(owner: ""graphql-dotnet"", name: ""graphql-client"") {
						name
					}
				}

				query NotRepository{
					repository(owner: """", name: """") {
						name
					}
				}"
			) {
				OperationName = "Repository"
			};
			var response = await this.GraphQLClient.SendQueryAsync(graphQLRequest);
			Assert.Equal("graphql-client", response.Data.repository.name.Value);
		}

		[Fact]
		public async void VariablesGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				query Repository($owner: String!, $name: String!){
					repository(owner: $owner, name: $name) {
						name
					}
				}"
			) {
				Variables = new {
					owner = "graphql-dotnet",
					name = "graphql-client"
				}
			};
			var response = await this.GraphQLClient.SendQueryAsync(graphQLRequest);
			Assert.Equal("graphql-client", response.Data.repository.name.Value);
		}

		[Fact]
		public async void OperationNameVariableGetAsyncFact() {
			var graphQLRequest = new GraphQLRequest(@"
				query Repository($owner: String!, $name: String!){
					repository(owner: $owner, name: $name) {
						name
					}
				}
				query NotRepository{
					repository(owner: """", name: """") {
						name
					}
				}"
			) {
				OperationName = "Repository",
				Variables = new {
					owner = "graphql-dotnet",
					name = "graphql-client"
				}
			};
			var response = await this.GraphQLClient.SendQueryAsync(graphQLRequest);
			Assert.Equal("graphql-client", response.Data.repository.name.Value);
		}

	}

}
