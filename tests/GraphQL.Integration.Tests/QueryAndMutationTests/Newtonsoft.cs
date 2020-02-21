using GraphQL.Client.Serializer.Newtonsoft;

namespace GraphQL.Integration.Tests.QueryAndMutationTests {
	public class Newtonsoft: Base {
		public Newtonsoft() : base(new NewtonsoftJsonSerializer())
		{
		}
	}
}
