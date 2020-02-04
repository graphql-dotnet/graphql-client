using System;
using System.Net.WebSockets;

namespace GraphQL.Client.Http {
	public static class GraphQLHttpClientExtensions {
		/// <summary>
		/// Creates a subscription to a GraphQL server. The connection is not established until the first actual subscription is made.<br/>
		/// All subscriptions made to this stream share the same hot observable.<br/>
		/// All <see cref="WebSocketException"/>s are passed to the <paramref name="webSocketExceptionHandler"/> to be handled externally.<br/>
		/// If the <paramref name="webSocketExceptionHandler"/> completes normally, the subscription is recreated with a new connection attempt.<br/>
		/// Other <see cref="Exception"/>s or any exception thrown by <paramref name="webSocketExceptionHandler"/> will cause the sequence to fail.
		/// </summary>
		/// <param name="client">the GraphQL client</param>
		/// <param name="request">the GraphQL request for this subscription</param>
		/// <param name="webSocketExceptionHandler">an external handler for all <see cref="WebSocketException"/>s occuring within the sequence</param>
		/// <returns>an observable stream for the specified subscription</returns>
		public static IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(this IGraphQLClient client,
			GraphQLRequest request, Action<WebSocketException> webSocketExceptionHandler) {
			return client.CreateSubscriptionStream<TResponse>(request, e => {
				if (e is WebSocketException webSocketException)
					webSocketExceptionHandler(webSocketException);
				else
					throw e;
			});
		}

		public static IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(
			this IGraphQLClient client, GraphQLRequest request, Func<TResponse> defineResponseType, Action<WebSocketException> webSocketExceptionHandler)
			=> client.CreateSubscriptionStream<TResponse>(request, webSocketExceptionHandler);
	}
}