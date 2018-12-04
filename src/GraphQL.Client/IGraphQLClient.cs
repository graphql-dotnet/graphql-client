using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	/// <summary>
	/// Represents the Interface that any GraphQL Client must implement
	/// </summary>
	public interface IGraphQLClient : IDisposable {

		/// <summary>
		/// Send a Query async
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">The Cancellation Token</param>
		/// <returns>The Response</returns>
		Task<GraphQLResponse> SendQueryAsync(GraphQLRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Send a Mutation Async
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">The Cancellation Token</param>
		/// <returns>The Response</returns>
		Task<GraphQLResponse> SendMutationAsync(GraphQLRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Send a Subscription async
		/// </summary>
		/// <param name="request">The Request</param>
		/// <param name="cancellationToken">The Cancellation Token</param>
		/// <returns>The Subscription Response</returns>
		[Obsolete("EXPERIMENTAL")]
		Task<IGraphQLSubscriptionResult> SendSubscribeAsync(GraphQLRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a subscription to a GraphQL server. The connection is not established until the first actual subscription is made.<br/>
		/// All subscriptions made to this stream share the same hot observable.<br/>
		/// The stream must be recreated completely after an error has occured within its logic (i.e. a <see cref="WebSocketException"/>)
		/// </summary>
		/// <param name="request">the GraphQL request for this subscription</param>
		/// <returns>an observable stream for the specified subscription</returns>
		[Obsolete("EXPERIMENTAL")]
		IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request);

		/// <summary>
		/// Creates a subscription to a GraphQL server. The connection is not established until the first actual subscription is made.<br/>
		/// All subscriptions made to this stream share the same hot observable.<br/>
		/// All <see cref="WebSocketException"/>s are passed to the <paramref name="webSocketExceptionHandler"/> to be handled externally.<br/>
		/// If the <paramref name="webSocketExceptionHandler"/> completes normally, the subscription is recreated with a new connection attempt.<br/>
		/// Other <see cref="Exception"/>s or any exception thrown by <paramref name="webSocketExceptionHandler"/> will cause the sequence to fail.
		/// </summary>
		/// <param name="request">the GraphQL request for this subscription</param>
		/// <param name="webSocketExceptionHandler">an external handler for all <see cref="WebSocketException"/>s occuring within the sequence</param>
		/// <returns>an observable stream for the specified subscription</returns>
		[Obsolete("EXPERIMENTAL")]
		IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request, Action<WebSocketException> webSocketExceptionHandler);

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
		[Obsolete("EXPERIMENTAL")]
		IObservable<GraphQLResponse> CreateSubscriptionStream(GraphQLRequest request, Action<Exception> exceptionHandler);
	}

}
