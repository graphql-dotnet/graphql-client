using GraphQL.Client.Serializer.Newtonsoft;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.WebsocketTests {
	public class Newtonsoft: Base {
		public Newtonsoft(ITestOutputHelper output) : base(output, new NewtonsoftJsonSerializer())
		{
		}
	}
}
