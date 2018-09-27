using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json;

namespace GraphQL.Client.Http.Internal {

	internal class GraphQLHttpHandler : IDisposable {

		public GraphQLHttpClientOptions Options { get; set; }

		public HttpClient HttpClient { get; set; }

		public GraphQLHttpHandler(GraphQLHttpClientOptions options) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.HttpClient = new HttpClient(this.Options.HttpMessageHandler);
		}

		public GraphQLHttpHandler(GraphQLHttpClientOptions options, HttpClient httpClient) {
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.HttpClient = httpClient;
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via GET
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> GetAsync(GraphQLRequest request, CancellationToken cancellationToken = default) {
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			var queryParamsBuilder = new StringBuilder($"query={request.Query}", 3);
			if (request.OperationName != null) { queryParamsBuilder.Append($"&operationName={request.OperationName}"); }
			if (request.Variables != null) { queryParamsBuilder.Append($"&variables={JsonConvert.SerializeObject(request.Variables)}"); }
			using (var httpResponseMessage = await this.HttpClient.GetAsync($"{this.Options.EndPoint}?{queryParamsBuilder.ToString()}", cancellationToken).ConfigureAwait(false)) {
				return await this.ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Send a <see cref="GraphQLRequest"/> via POST
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>The Response</returns>
		public async Task<GraphQLResponse> PostAsync(GraphQLRequest request, CancellationToken cancellationToken = default) {
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			var graphQLString = JsonConvert.SerializeObject(request, this.Options.JsonSerializerSettings);
			using (var httpContent = new StringContent(graphQLString)) {
				httpContent.Headers.ContentType = this.Options.MediaType;
				using (var httpResponseMessage = await this.HttpClient.PostAsync(this.Options.EndPoint, httpContent, cancellationToken).ConfigureAwait(false)) {
					return await this.ReadHttpResponseMessageAsync(httpResponseMessage).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Reads the <see cref="HttpResponseMessage"/>
		/// </summary>
		/// <param name="httpResponseMessage">The Response</param>
		/// <returns>The GraphQLResponse</returns>
		public async Task<GraphQLResponse> ReadHttpResponseMessageAsync(HttpResponseMessage httpResponseMessage) {
			using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
			using (var streamReader = new StreamReader(stream))
			using (var jsonTextReader = new JsonTextReader(streamReader)) {
				var jsonSerializer = new JsonSerializer	{
					ContractResolver = this.Options.JsonSerializerSettings.ContractResolver
				};
				if (!httpResponseMessage.IsSuccessStatusCode) {
					GraphQLResponse response;
					try {
						response = jsonSerializer.Deserialize<GraphQLResponse>(jsonTextReader);
					}
					catch (JsonReaderException) {
						throw new GraphQLHttpException(httpResponseMessage);
					}

					if (response == null || response.Data == null) {
						throw new GraphQLHttpException(httpResponseMessage);
					}

					return response;
				}

				try {
					return jsonSerializer.Deserialize<GraphQLResponse>(jsonTextReader);
				}
				catch (JsonReaderException)	{
					throw new GraphQLHttpException(httpResponseMessage);
				}
			}
		}

		public void Dispose() {
			this.HttpClient.Dispose();
			this.Options.HttpMessageHandler.Dispose();
		}

	}

}
