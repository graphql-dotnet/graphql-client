using System.Collections;

namespace GraphQL.Client.Serializer.Tests.TestData;

public class SerializeToStringTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] {
            "{\"query\":\"simplequerystring\",\"variables\":null,\"operationName\":null,\"extensions\":null}",
            new GraphQLRequest("simple query string")
        };
        yield return new object[] {
            "{\"query\":\"simplequerystring\",\"variables\":{\"camelCaseProperty\":\"camelCase\",\"PascalCaseProperty\":\"PascalCase\"},\"operationName\":null,\"extensions\":null}",
            new GraphQLRequest("simple query string", new { camelCaseProperty = "camelCase", PascalCaseProperty = "PascalCase"})
        };
        yield return new object[] {
            "{\"query\":\"simplequerystring\",\"variables\":null,\"operationName\":null,\"extensions\":null,\"authentication\":\"an-authentication-token\"}",
            new GraphQLRequest("simple query string"){{"authentication", "an-authentication-token"}}
        };
        yield return new object[] {
            "{\"query\":\"enumtest\",\"variables\":{\"enums\":[\"REGULAR\",\"PASCAL_CASE\",\"CAMEL_CASE\",\"LOWER\",\"UPPER\",\"CONSTANT_CASE\"]},\"operationName\":null,\"extensions\":null}",
            new GraphQLRequest("enumtest", new { enums = Enum.GetValues(typeof(TestEnum)).Cast<TestEnum>()})
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public enum TestEnum
    {
        Regular,
        PascalCase,
        camelCase,
        lower,
        UPPER,
        CONSTANT_CASE
    }
}
