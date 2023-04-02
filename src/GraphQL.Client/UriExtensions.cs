namespace GraphQL.Client.Http;

public static class UriExtensions
{
    /// <summary>
    /// Returns true if <see cref="Uri.Scheme"/> equals "wss" or "ws"
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static bool HasWebSocketScheme(this Uri? uri) =>
        uri is not null &&
        (uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("ws", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Infers the websocket uri from <paramref name="uri"/>.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static Uri GetWebSocketUri(this Uri uri)
    {
        if (uri is null)
            throw new ArgumentNullException(nameof(uri));

        if (uri.HasWebSocketScheme())
            return uri;

        string webSocketScheme;

        if (uri.Scheme == Uri.UriSchemeHttps)
            webSocketScheme = "wss";
        else if (uri.Scheme == Uri.UriSchemeHttp)
            webSocketScheme = "ws";
        else
            throw new NotSupportedException($"cannot infer websocket uri from uri scheme {uri.Scheme}");

        return new UriBuilder(uri) { Scheme = webSocketScheme }.Uri;
    }
}
