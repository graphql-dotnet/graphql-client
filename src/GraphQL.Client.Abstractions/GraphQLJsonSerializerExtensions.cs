using System;
using System.Linq;

namespace GraphQL.Client.Abstractions {
	public static class GraphQLJsonSerializerExtensions {
		public static void EnsureAssigned<TSerializerInterface>(this TSerializerInterface jsonSerializer) where TSerializerInterface: IGraphQLJsonSerializer {
			// return if a serializer was assigned
			if (jsonSerializer != null) return;

			// else try to find one in the assembly and assign that
			var type = typeof(TSerializerInterface);
			var serializerType = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.FirstOrDefault(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
			if (serializerType == null)
				throw new InvalidOperationException($"no implementation of \"{type}\" found");

			jsonSerializer = (TSerializerInterface)Activator.CreateInstance(serializerType);
		}
	}
}
