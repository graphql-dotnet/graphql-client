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
var heroAndFriendsRequest = new GraphQLRequest {
    Query =@"
	query HeroNameAndFriends($episode: Episode) {
		hero(episode: $episode) {
			name
			friends {
				name
			}
		}
	}",
	OperationName = "HeroNameAndFriends",
	Variables = new {
		episode = "JEDI"
	}
};
```

### Execute Query/Mutation:
```csharp
var graphQLClient = new GraphQLClient("https://swapi.apis.guru/");

public class HeroAndFriendsResponse {
    public Hero Hero {get; set;}

    public class Hero {
        public string Name {get; set;}

        public List<Hero> Friends {get; set;}
    }
}

var graphQLResponse = await graphQLClient.SendQueryAsync<HeroAndFriendsResponse>(heroAndFriendsRequest);

var heroName = graphQLResponse.Data.Hero.Name;
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
