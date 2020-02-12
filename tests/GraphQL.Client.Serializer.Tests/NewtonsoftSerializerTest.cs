using GraphQL.Client.Serializer.Newtonsoft;

namespace GraphQL.Client.Serializer.Tests {
	public class NewtonsoftSerializerTest : BaseSerializerTest {
		public NewtonsoftSerializerTest() : base(new NewtonsoftJsonSerializer()) { }
	}
}
