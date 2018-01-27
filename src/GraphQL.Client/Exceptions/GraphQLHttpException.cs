using System;
using System.Net.Http;

namespace GraphQL.Client.Exceptions {

	/// <summary>
	/// An exception thrown on unexpected <see cref="System.Net.Http.HttpResponseMessage"/>
	/// </summary>
	public class GraphQLHttpException : Exception {

		/// <summary>
		/// The <see cref="System.Net.Http.HttpResponseMessage"/>
		/// </summary>
		public HttpResponseMessage HttpResponseMessage { get; }

		/// <summary>
		/// Creates a new instance of <see cref="GraphQLHttpException"/>
		/// </summary>
		/// <param name="httpResponseMessage">The unexpected <see cref="System.Net.Http.HttpResponseMessage"/></param>
		public GraphQLHttpException(HttpResponseMessage httpResponseMessage):base($"Unexpected {nameof(System.Net.Http.HttpResponseMessage)} with code: {httpResponseMessage.StatusCode}") {
			this.HttpResponseMessage = httpResponseMessage ?? throw new ArgumentNullException(nameof(httpResponseMessage));
		}

	}

}
