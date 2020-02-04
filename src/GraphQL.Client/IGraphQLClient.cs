using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Client {

	public interface IGraphQLClient : IDisposable {

		Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default);

		Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a subscription to a GraphQL server. The connection is not established until the first actual subscription is made.<br/>
		/// All subscriptions made to this stream share the same hot observable.<br/>
		/// The stream must be recreated completely after an error has occured within its logic (i.e. a <see cref="WebSocketException"/>)
		/// </summary>
		/// <param name="request">the GraphQL request for this subscription</param>
		/// <returns>an observable stream for the specified subscription</returns>
		IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request);
		
		/// <summary>
		/// Creates a subscription to a GraphQL server. The connection is not established until the first actual subscription is made.<br/>
		/// All subscriptions made to this stream share the same hot observable.<br/>
		/// All <see cref="Exception"/>s are passed to the <paramref name="exceptionHandler"/> to be handled externally.<br/>
		/// If the <paramref name="exceptionHandler"/> completes normally, the subscription is recreated with a new connection attempt.<br/>
		/// Any exception thrown by <paramref name="exceptionHandler"/> will cause the sequence to fail.
		/// </summary>
		/// <param name="request">the GraphQL request for this subscription</param>
		/// <param name="exceptionHandler">an external handler for all <see cref="Exception"/>s occuring within the sequence</param>
		/// <returns>an observable stream for the specified subscription</returns>
		IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception> exceptionHandler);

		/// <summary>
		/// Publishes all exceptions which occur inside the websocket receive stream (i.e. for logging purposes)
		/// </summary>
		IObservable<Exception> WebSocketReceiveErrors { get; }
	}

}
