# GraphQL.Client
[![NuGet](https://img.shields.io/nuget/v/GraphQL.Client.svg)](https://www.nuget.org/packages/GraphQL.Client)
[![NuGet](https://img.shields.io/nuget/vpre/GraphQL.Client.svg)](https://www.nuget.org/packages/GraphQL.Client)

A GraphQL Client for .NET Standard over HTTP.

## Specification:
The Library will try to follow the following standards and documents:
[GraphQL Specification](https://facebook.github.io/graphql/June2018)
[GraphQL HomePage](http://graphql.org/learn)

## Usage:

### Create a GraphQLRequest:
#### Simple Request:
```csharp
var heroRequest = new GraphQLRequest {
    Query = @"
    {
        hero {
            name
        }
    }"
};
```

#### OperationName and Variables Request:

```csharp
var personAndFilmsRequest = new GraphQLRequest {
    Query =@"
    query PersonAndFilms($id: ID) {
        person(id: $id) {
            name
            filmConnection {
                films {
                    title
                }
            }
        }
    }",
    OperationName = "PersonAndFilms",
    Variables = new {
        id = "cGVvcGxlOjE="
    }
};
```

Be careful when using `byte[]` in your variables object, as most JSON serializers will treat that as binary data! If you really need to send a *list of bytes* with a `byte[]` as a source, then convert it to a `List<byte>` first, which will tell the serializer to output a list of numbers instead of a base64-encoded string.

### Execute Query/Mutation:

```csharp
var graphQLClient = new GraphQLHttpClient("https://swapi.apis.guru/");

public class PersonAndFilmsResponse {
    public PersonContent Person { get; set; }

    public class PersonContent {
        public string Name { get; set; }
        public FilmConnectionContent FilmConnection { get; set; }

        public class FilmConnectionContent {
            public List<FilmContent> Films { get; set; }

            public class FilmContent {
                public string Title { get; set; }
            }
        }
    }
}

var graphQLResponse = await graphQLClient.SendQueryAsync<PersonAndFilmsResponse>(personAndFilmsRequest);

var personName = graphQLResponse.Data.Person.Name;
```


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

## Useful Links:
[StarWars Example Server (GitHub)](https://github.com/graphql/swapi-graphql)
[StarWars Example Server (EndPoint)](https://swapi.apis.guru/)

[GitHub GraphQL API Docs](https://developer.github.com/v4/guides/forming-calls/)
[GitHub GraphQL Explorer](https://developer.github.com/v4/explorer/)
[GitHub GraphQL Endpoint](https://api.github.com/graphql)
