# GraphQL.Client

A GraphQL Client for .NET Standard over HTTP.

Provides the following packages:

| Package | Downloads | Nuget Latest | 
|---------|-----------|--------------|
| GraphQL.Client | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Client)](https://www.nuget.org/packages/GraphQL.Client/) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Client)](https://www.nuget.org/packages/GraphQL.Client) | 
| GraphQL.Client.Abstractions | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Client.Abstractions)](https://www.nuget.org/packages/GraphQL.Client.Abstractions) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Client.Abstractions)](https://www.nuget.org/packages/GraphQL.Client.Abstractions) |
| GraphQL.Client.Abstractions.Websocket | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Client.Abstractions.Websocket)](https://www.nuget.org/packages/GraphQL.Client.Abstractions.Websocket) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Client.Abstractions.Websocket)](https://www.nuget.org/packages/GraphQL.Client.Abstractions.Websocket) |
| GraphQL.Client.LocalExecution | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Client.LocalExecution)](https://www.nuget.org/packages/GraphQL.Client.LocalExecution) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Client.LocalExecution)](https://www.nuget.org/packages/GraphQL.Client.LocalExecution) | 
| GraphQL.Client.Serializer.Newtonsoft | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Client.Serializer.Newtonsoft)](https://www.nuget.org/packages/GraphQL.Client.Serializer.Newtonsoft) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Client.Serializer.Newtonsoft)](https://www.nuget.org/packages/GraphQL.Client.Serializer.Newtonsoft) | 
| GraphQL.Client.Serializer.SystemTextJson | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Client.Serializer.SystemTextJson)](https://www.nuget.org/packages/GraphQL.Client.Serializer.SystemTextJson) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Client.Serializer.SystemTextJson)](https://www.nuget.org/packages/GraphQL.Client.Serializer.SystemTextJson) | 
| GraphQL.Primitives | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Primitives)](https://www.nuget.org/packages/GraphQL.Primitives/) | [![Nuget](https://img.shields.io/nuget/vpre/GraphQL.Primitives)](https://www.nuget.org/packages/GraphQL.Primitives) | 

## Specification
The Library will try to follow the following standards and documents:

* [GraphQL Specification](https://spec.graphql.org/June2018/)
* [GraphQL HomePage](https://graphql.org/learn)

## Usage

The intended use of `GraphQLHttpClient` is to keep one instance alive per endpoint (obvious in case you're
operating full websocket, but also true for regular requests) and is built with thread-safety in mind.

### Create a GraphQLHttpClient

```csharp
// To use NewtonsoftJsonSerializer, add a reference to 
// NuGet package GraphQL.Client.Serializer.Newtonsoft
var graphQLClient = new GraphQLHttpClient(
    "https://api.example.com/graphql", 
    new NewtonsoftJsonSerializer());
```

> [!NOTE]
> *GraphQLHttpClient* is meant to be used as a single long-lived instance per endpoint (i.e. register as singleton in a DI system), which should be reused for multiple requests.

### Create a GraphQLRequest:
#### Simple Request:
```csharp
var heroRequest = new GraphQLRequest {
    Query = """
    {
        hero {
            name
        }
    }
    """
};
```

#### OperationName and Variables Request:

```csharp
var personAndFilmsRequest = new GraphQLRequest {
    Query ="""
    query PersonAndFilms($id: ID) {
        person(id: $id) {
            name
            filmConnection {
                films {
                    title
                }
            }
        }
    }
    """,
    OperationName = "PersonAndFilms",
    Variables = new {
        id = "cGVvcGxlOjE="
    }
};
```

> [!WARNING]
> Be careful when using `byte[]` in your variables object, as most JSON serializers will treat that as binary data.
> 
> If you really need to send a *list of bytes* with a `byte[]` as a  source, then convert it to a `List<byte>` first, which will tell the serializer to output a list of numbers instead of a base64-encoded string.

### Execute Query/Mutation

```csharp
public class ResponseType 
{
    public PersonType Person { get; set; }
}

public class PersonType 
{
    public string Name { get; set; }
    public FilmConnectionType FilmConnection { get; set; }    
}

public class FilmConnectionType {
    public List<FilmContentType> Films { get; set; }    
}

public class FilmContentType {
    public string Title { get; set; }
}

var graphQLResponse = await graphQLClient.SendQueryAsync<ResponseType>(personAndFilmsRequest);

var personName = graphQLResponse.Data.Person.Name;
```

Using the extension method for anonymously typed responses (namespace `GraphQL.Client.Abstractions`) you could achieve the same result with the following code:

```csharp
var graphQLResponse = await graphQLClient.SendQueryAsync(
    personAndFilmsRequest, 
    () => new { person = new PersonType()});
var personName = graphQLResponse.Data.person.Name;
```

> [!IMPORTANT]
> Note that the field in the GraphQL response which gets deserialized into the response object is the `data` field.
>
> A common mistake is to try to directly use the `PersonType` class as response type (because thats the *thing* you actually want to query), but the returned response object contains a property `person` containing a `PersonType` object (like the `ResponseType` modelled above).

### Use Subscriptions

```csharp
public class UserJoinedSubscriptionResult {
    public ChatUser UserJoined { get; set; }

    public class ChatUser {
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }
}
```

#### Create subscription

```csharp
var userJoinedRequest = new GraphQLRequest {
    Query = @"
    subscription {
        userJoined{
            displayName
            id
        }
    }"
};

IObservable<GraphQLResponse<UserJoinedSubscriptionResult>> subscriptionStream 
    = client.CreateSubscriptionStream<UserJoinedSubscriptionResult>(userJoinedRequest);

var subscription = subscriptionStream.Subscribe(response => 
    {
        Console.WriteLine($"user '{response.Data.UserJoined.DisplayName}' joined")
    });
```

#### End Subscription

```csharp
subscription.Dispose();
```

### Automatic persisted queries (APQ)

[Automatic persisted queries (APQ)](https://www.apollographql.com/docs/apollo-server/performance/apq/) are supported since client version 6.1.0.

APQ can be enabled by configuring `GraphQLHttpClientOptions.EnableAutomaticPersistedQueries` to resolve to `true`.

By default, the client will automatically disable APQ for the current session if the server responds with a `PersistedQueryNotSupported` error or a 400 or 600 HTTP status code.
This can be customized by configuring `GraphQLHttpClientOptions.DisableAPQ`.

To re-enable APQ after it has been automatically disabled, `GraphQLHttpClient` needs to be disposed an recreated.

APQ works by first sending a hash of the query string to the server, and only sending the full query string if the server has not yet cached a query with a matching hash.
With queries supplied as a string parameter to `GraphQLRequest`, the hash gets computed each time the request is sent.

When you want to reuse a query string (propably to leverage APQ :wink:), declare the query using the `GraphQLQuery` class. This way, the hash gets computed once on construction
of the `GraphQLQuery` object and handed down to each `GraphQLRequest` using the query.

```csharp
GraphQLQuery query = new("""
                         query PersonAndFilms($id: ID) {
                             person(id: $id) {
                                 name
                                 filmConnection {
                                     films {
                                         title
                                     }
                                 }
                             }
                         }
                         """);
                         
var graphQLResponse = await graphQLClient.SendQueryAsync<ResponseType>(
    query, 
    "PersonAndFilms",
    new { id = "cGVvcGxlOjE=" });
```

### Syntax Highlighting for GraphQL strings in IDEs

.NET 7.0 introduced the [StringSyntaxAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.stringsyntaxattribute?view=net-8.0) to have a unified way of telling what data is expected in a given `string` or `ReadOnlySpan<char>`. IDEs like Visual Studio and Rider can then use this to provide syntax highlighting and checking.

From v6.0.4 on all GraphQL string parameters in this library are decorated with the `[StringSyntax("GraphQL")]` attribute.

Currently, there is no native support for GraphQL formatting and syntax highlighting in Visual Studio, but the [GraphQLTools Extension](https://marketplace.visualstudio.com/items?itemName=codearchitects-research.GraphQLTools) provides that for you.

For Rider, JetBrains provides a [Plugin](https://plugins.jetbrains.com/plugin/8097-graphql), too.

To leverage syntax highlighting in variable declarations, use the `GraphQLQuery` class.


## Useful Links

* [StarWars Example Server (GitHub)](https://github.com/graphql/swapi-graphql)
* [StarWars Example Server (EndPoint)](https://swapi.apis.guru/)

* [GitHub GraphQL API Docs](https://developer.github.com/v4/guides/forming-calls/)
* [GitHub GraphQL Explorer](https://developer.github.com/v4/explorer/)
* [GitHub GraphQL Endpoint](https://api.github.com/graphql)

## Blazor WebAssembly Limitations

Blazor WebAssembly differs from other platforms as it does not support all features of other .NET runtime implementations. For instance, the following WebSocket options properties are not supported and will not be set:
* [ClientCertificates](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocketoptions.clientcertificates?view=netcore-3.1#System_Net_WebSockets_ClientWebSocketOptions_ClientCertificates)
* [UseDefaultCredentials](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocketoptions.usedefaultcredentials?view=netcore-3.1)
