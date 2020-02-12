using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    public class SystemTextJsonSerializer: IGraphQLWebsocketJsonSerializer
    {
	    public SystemTextJsonSerializerOptions Options { get; }

	    public SystemTextJsonSerializer() : this(new SystemTextJsonSerializerOptions()) { }

	    public SystemTextJsonSerializer(Action<SystemTextJsonSerializerOptions> configure) : this(configure.New()) { }

		public SystemTextJsonSerializer(SystemTextJsonSerializerOptions options) {
		    Options = options;
		    ConfigureMandatorySerializerOptions();
		}

		private void ConfigureMandatorySerializerOptions() {
			// deserialize extensions to Dictionary<string, object>
			Options.JsonSerializerOptions.Converters.Insert(0, new GraphQLExtensionsConverter());
			// allow the JSON field "data" to match the property "Data" even without JsonNamingPolicy.CamelCase
			Options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
		}

	    public string SerializeToString(GraphQLRequest request) {
		    return JsonSerializer.Serialize(new STJGraphQLRequest(request), Options.JsonSerializerOptions);
	    }

	    public Task<GraphQLResponse<TResponse>> DeserializeFromUtf8StreamAsync<TResponse>(Stream stream, CancellationToken cancellationToken) {
		    return JsonSerializer.DeserializeAsync<GraphQLResponse<TResponse>>(stream, Options.JsonSerializerOptions, cancellationToken).AsTask();
	    }

	    public byte[] SerializeToBytes(GraphQLWebSocketRequest request) {
		    return JsonSerializer.SerializeToUtf8Bytes(new STJGraphQLWebSocketRequest(request), Options.JsonSerializerOptions);
	    }

	    public Task<WebsocketResponseWrapper> DeserializeToWebsocketResponseWrapperAsync(Stream stream) {
		    return JsonSerializer.DeserializeAsync<WebsocketResponseWrapper>(stream, Options.JsonSerializerOptions).AsTask();
		}

	    public GraphQLWebSocketResponse<GraphQLResponse<TResponse>> DeserializeToWebsocketResponse<TResponse>(byte[] bytes) {
		    return JsonSerializer.Deserialize<GraphQLWebSocketResponse<GraphQLResponse<TResponse>>>(new ReadOnlySpan<byte>(bytes),
			    Options.JsonSerializerOptions);
	    }

    }
}
