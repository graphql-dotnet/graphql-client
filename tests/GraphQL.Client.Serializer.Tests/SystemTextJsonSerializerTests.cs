using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.SystemTextJson;
using Xunit;

namespace GraphQL.Client.Serializer.Tests {
	public class SystemTextJsonSerializerTests: BaseSerializerTest {
		public SystemTextJsonSerializerTests() : base(new SystemTextJsonSerializer())
		{
		}

		protected override void AssertGraphQlErrorDataExtensions(object dataField, IDictionary<string, object> expectedContent) {
			dataField.Should().BeEquivalentTo(expectedContent);
		}
	}
}
