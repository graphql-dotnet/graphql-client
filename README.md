# GraphQL.Client:
[![CircleCI](https://circleci.com/gh/deinok/GraphQL.Client.svg?style=svg)](https://circleci.com/gh/deinok/GraphQL.Client)

A GraphQL Client for .NET Standard.

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
  Query = @"
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

## Useful Links:
[GraphQL Specification](http://facebook.github.io/graphql/October2016/)

[StarWars Example Server (GitHub)](https://github.com/graphql/swapi-graphql)
[StarWars Example Server (EndPoint)](https://swapi.apis.guru/)

[GitHub GraphQL API Docs](https://developer.github.com/v4/guides/forming-calls/)
[GitHub GraphQL Explorer](https://developer.github.com/v4/explorer/)
[GitHub GraphQL Endpoint](https://api.github.com/graphql)
