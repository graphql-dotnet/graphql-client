using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using GraphQL.Types;

namespace IntegrationTestServer.ChatSchema {
	public class CapitalizedFieldsGraphType: ObjectGraphType {
		public CapitalizedFieldsGraphType() {
			Name = "CapitalizedFields";

			Field<StringGraphType>()
				.Name("StringField")
				.Resolve(context => "hello world");
		}
	}
}
