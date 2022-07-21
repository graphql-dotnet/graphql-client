using System.Collections;

namespace GraphQL.Client.Tests.Common.StarWars.TestData;

/// <summary>
/// Test data object
/// </summary>
public class StarWarsHumans : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, "Luke" };
        yield return new object[] { 2, "Vader" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
