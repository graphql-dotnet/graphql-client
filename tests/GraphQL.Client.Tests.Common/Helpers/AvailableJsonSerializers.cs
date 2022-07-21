using System.Collections;
using GraphQL.Client.Abstractions;

namespace GraphQL.Client.Tests.Common.Helpers;

public class AvailableJsonSerializers<TSerializerInterface> : IEnumerable<object[]> where TSerializerInterface : IGraphQLJsonSerializer
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // try to find one in the assembly and assign that
        var type = typeof(TSerializerInterface);
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .Select(serializerType => new object[] { Activator.CreateInstance(serializerType) })
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
