using System.Collections;

namespace GraphQL.Client.Serializer.Tests.TestData;

public class DeserializeResponseTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // object array structure:
        // [0]: input json
        // [1]: expected deserialized response

        yield return new object[] {
            "{\"errors\":[{\"message\":\"Throttled\",\"extensions\":{\"code\":\"THROTTLED\",\"documentation\":\"https://help.shopify.com/api/graphql-admin-api/graphql-admin-api-rate-limits\"}}],\"extensions\":{\"cost\":{\"requestedQueryCost\":992,\"actualQueryCost\":null,\"throttleStatus\":{\"maximumAvailable\":1000,\"currentlyAvailable\":632,\"restoreRate\":50}}}}",
            new GraphQLResponse<object> {
                Data = null,
                Errors = new[] {
                    new GraphQLError {
                        Message = "Throttled",
                        Extensions = new Map {
                            {"code", "THROTTLED" },
                            {"documentation", "https://help.shopify.com/api/graphql-admin-api/graphql-admin-api-rate-limits" }
                        }
                    }
                },
                Extensions = new Map {
                    {"cost", new Dictionary<string, object> {
                        {"requestedQueryCost", 992},
                        {"actualQueryCost", null},
                        {"throttleStatus", new Dictionary<string, object> {
                            {"maximumAvailable", 1000},
                            {"currentlyAvailable", 632},
                            {"restoreRate", 50}
                        }}
                    }}
                }
            }
        };

        yield return new object[]
        {
            @"{
                    ""errors"": [
                        {
                            ""message"": ""Name for character with ID 1002 could not be fetched."",
                            ""locations"": [
                                {
                                    ""line"": 6,
                                    ""column"": 7
                                }
                            ],
                            ""path"": [
                                ""hero"",
                                ""heroFriends"",
                                1,
                                ""name""
                            ]
                        }
                    ],
                    ""data"": {
                        ""hero"": {
                            ""name"": ""R2-D2"",
                            ""heroFriends"": [
                                {
                                    ""id"": ""1000"",
                                    ""name"": ""Luke Skywalker""
                                },
                                {
                                    ""id"": ""1002"",
                                    ""name"": null
                                },
                                {
                                    ""id"": ""1003"",
                                    ""name"": ""Leia Organa""
                                }
                            ]
                        }
                    }
                }",
            NewAnonymouslyTypedGraphQLResponse(new
                {
                    hero = new
                    {
                        name = "R2-D2",
                        heroFriends = new List<Friend>
                        {
                            new Friend {Id = "1000", Name = "Luke Skywalker"},
                            new Friend {Id = "1002", Name = null},
                            new Friend {Id = "1003", Name = "Leia Organa"}
                        }
                    }
                },
                new[] {
                    new GraphQLError {
                        Message = "Name for character with ID 1002 could not be fetched.",
                        Locations = new [] { new GraphQLLocation{Line = 6, Column = 7 }},
                        Path = new ErrorPath{"hero", "heroFriends", 1, "name"}
                    }
                })
        };

        // add test for github issue #230 : https://github.com/graphql-dotnet/graphql-client/issues/230
        yield return new object[] {
            "{\"data\":{\"getMyModelType\":{\"id\":\"foo\",\"title\":\"The best Foo movie!\"}}}",
            new GraphQLResponse<GetMyModelTypeResponse> {
                Data = new GetMyModelTypeResponse
                {
                    getMyModelType = new Movie
                    {
                        id = "foo",
                        title = "The best Foo movie!"
                    }
                },
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private GraphQLResponse<T> NewAnonymouslyTypedGraphQLResponse<T>(T data, GraphQLError[]? errors = null, Map? extensions = null)
        => new GraphQLResponse<T> { Data = data, Errors = errors, Extensions = extensions };
}

public class Friend
{
    public string Id { get; set; }
    public string? Name { get; set; }
}

public class GetMyModelTypeResponse
{
    //--- Properties ---
    public Movie getMyModelType { get; set; }
}
public class Movie
{
    //--- Properties ---
    public string id { get; set; }
    public string title { get; set; }
}
