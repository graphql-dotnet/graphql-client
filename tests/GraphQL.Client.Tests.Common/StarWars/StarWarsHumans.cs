using System.Collections;
using System.Collections.Generic;

namespace GraphQL.Client.Tests.Common.StarWars
{
    public class StarWarsHumans : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { 1, "Luke" };
            yield return new object[] { 2, "Vader" };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
