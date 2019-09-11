# GraphQL.Client
[![NuGet](https://img.shields.io/nuget/v/GraphQL.Client.svg)](https://www.nuget.org/packages/GraphQL.Client)
[![MyGet](https://img.shields.io/myget/graphql-dotnet/v/GraphQL.Client.svg)](https://www.myget.org/feed/graphql-dotnet/package/nuget/GraphQL.Client)

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

### Send Request:
```csharp
var graphQLClient = new GraphQLClient("https://swapi.apis.guru/");
var graphQLResponse = await graphQLClient.PostAsync(heroRequest);
```

### Read GraphQLResponse:

#### Dynamic:
```csharp
var graphQLResponse = await graphQLClient.PostAsync(heroRequest);
var dynamicHeroName = graphQLResponse.Data.hero.name.Value; //Value of data->hero->name
```

#### Typed:
```csharp
var graphQLResponse = await graphQLClient.PostAsync(heroRequest);
var personType = graphQLResponse.GetDataFieldAs<Person>("hero"); //data->hero is casted as Person
var name = personType.Name;
```

## Useful Links:
[StarWars Example Server (GitHub)](https://github.com/graphql/swapi-graphql)
[StarWars Example Server (EndPoint)](https://swapi.apis.guru/)

[GitHub GraphQL API Docs](https://developer.github.com/v4/guides/forming-calls/)
[GitHub GraphQL Explorer](https://developer.github.com/v4/explorer/)
[GitHub GraphQL Endpoint](https://api.github.com/graphql)
