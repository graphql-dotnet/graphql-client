using System;
using System.Linq;

namespace GraphQL.Client.Abstractions
{
    public static class GraphQLJsonSerializerExtensions
    {
        public static TSerializerInterface EnsureAssigned<TSerializerInterface>(this TSerializerInterface jsonSerializer) where TSerializerInterface : IGraphQLJsonSerializer
        {
            // if no serializer was assigned
            if (jsonSerializer == null)
            {
                // try to find one in the assembly and assign that
                var type = typeof(TSerializerInterface);
                var serializerType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .FirstOrDefault(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
                if (serializerType == null)
                    throw new InvalidOperationException($"no implementation of \"{type}\" found");

                jsonSerializer = (TSerializerInterface)Activator.CreateInstance(serializerType);
            }

            return jsonSerializer;
        }

        public static TOptions New<TOptions>(this Action<TOptions> configure) =>
            configure.AndReturn(Activator.CreateInstance<TOptions>());

        public static TOptions AndReturn<TOptions>(this Action<TOptions> configure, TOptions options)
        {
            configure(options);
            return options;
        }
    }
}
