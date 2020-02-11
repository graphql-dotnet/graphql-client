using System.Collections.Generic;
using FluentAssertions;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GraphQL.Client.Serializer.Tests {
	public class NewtonsoftSerializerTest : BaseSerializerTest {
		public NewtonsoftSerializerTest() : base(new NewtonsoftJsonSerializer()) { }

		protected override void AssertGraphQlErrorDataExtensions(object dataField, IDictionary<string, object> expectedContent) {
			dataField.Should().BeOfType<JObject>().Which.Should().ContainKeys(expectedContent.Keys);
			var data = (JObject) dataField;

			foreach (var item in expectedContent) {
				switch (item.Value) {
					case int i:
						data[item.Key].Value<int>().Should().Be(i);
						break;
					case string s:
						data[item.Key].Value<string>().Should().BeEquivalentTo(s);
						break;
					default:
						Assert.True(false, $"unexpected value type \"{item.Value.GetType()}\" in expected content! Please review this unit test and add the missing type case!");
						break;
				}
			}
		}
	}
}
