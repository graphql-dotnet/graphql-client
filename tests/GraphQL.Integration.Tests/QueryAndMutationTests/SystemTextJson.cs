using GraphQL.Client.Serializer.SystemTextJson;

namespace GraphQL.Integration.Tests.QueryAndMutationTests {
	public class SystemTextJson: Base {
		public SystemTextJson() : base(new SystemTextJsonSerializer())
		{
		}
	}
}
