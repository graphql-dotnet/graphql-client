using System.Collections.Generic;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;

namespace GraphQL.Client.Serializer.Tests
{
    public class NewtonsoftSerializerTest : BaseSerializerTest
    {
        public NewtonsoftSerializerTest() : base(new NewtonsoftJsonSerializer()) { }
    }

    public class NewtonsoftSerializeNoCamelCaseTest : BaseSerializeNoCamelCaseTest
    {
        public NewtonsoftSerializeNoCamelCaseTest()
            : base(new NewtonsoftJsonSerializer(new JsonSerializerSettings())) { }
    }
}
