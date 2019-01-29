using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using Xunit;

namespace GraphQL.Client.Tests.Http
{

	public class GraphQLHttpClientSendQueryAsyncTest{

		// Relates to an issue with the 1.x versions.
		// See: https://github.com/graphql-dotnet/graphql-client/issues/53
		[Fact]
		public async void SendQueryAsyncShouldPreserveUriParametersFact(){
			var endpoint = new Uri("http://localhost/api/graphql?code=my-secret-api-key");

			var handlerStub = new HttpHandlerStub();
			var options = new GraphQLHttpClientOptions(){
				EndPoint = endpoint,
				HttpMessageHandler = handlerStub
			};
			var systemUnderTest = new GraphQLHttpClient(options);

			var response = await systemUnderTest.SendQueryAsync(new GraphQLRequest(@"
				{
					person(personID: ""1"") {
						name
					}
				}"
			));

			var actualRequestUri = handlerStub.LastRequest.RequestUri;
			var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(actualRequestUri.Query);
			Assert.True(queryParams.ContainsKey("code"), "Expected code query parameter to be preserved");
			Assert.Equal("my-secret-api-key", queryParams["code"]);
		}


		private class HttpHandlerStub : HttpMessageHandler{

			public HttpRequestMessage LastRequest { get; private set; }

			public CancellationToken LastCancellationToken { get; private set; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken){
				this.LastRequest = request;
				this.LastCancellationToken = cancellationToken;
				var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK){
					Content = new StringContent("{}")
				};
				return Task.FromResult(response);
			}

		}

	}

}
