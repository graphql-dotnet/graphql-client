using System.Collections;
using System.Collections.Generic;

namespace GraphQL.Client.Serializer.Tests.TestData
{
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
                            Extensions = new GraphQLExtensionsType {
                                {"code", "THROTTLED" },
                                {"documentation", "https://help.shopify.com/api/graphql-admin-api/graphql-admin-api-rate-limits" }
                            }
                        }
                    },
                    Extensions = new GraphQLExtensionsType {
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
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
