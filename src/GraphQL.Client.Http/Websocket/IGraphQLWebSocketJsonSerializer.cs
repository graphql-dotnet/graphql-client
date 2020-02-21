namespace GraphQL.Client.Http.Websocket {
	public interface IGraphQLWebSocketJsonSerializer: IGraphQLJsonSerializer {
		GraphQLWebSocketResponse<TResponse> DeserializeWebSocketResponse<TResponse>(byte[] utf8bytes);
	}
}
