using System;

namespace GraphQL.Client.Http
{
    public static class UriExtensions
    {
        /// <summary>
        /// Returns true if <see cref="Uri.Scheme"/> equals "wss" or "ws"
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool HasWebSocketScheme(this Uri uri) => uri.Scheme.Equals("wss") || uri.Scheme.Equals("ws");

        /// <summary>
        /// Infers the websocket uri from <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri GetWebSocketUri(this Uri uri)
        {
            if (uri.HasWebSocketScheme())
                return uri;

            string webSocketScheme = uri.Scheme == "https" ? "wss" : "ws";
            return new Uri($"{webSocketScheme}://{uri.Host}:{uri.Port}{uri.PathAndQuery}");
        }
    }
}
