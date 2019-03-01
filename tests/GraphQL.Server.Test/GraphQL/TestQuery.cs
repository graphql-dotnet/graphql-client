using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL {

	public class TestQuery:ObjectGraphType {

		public TestQuery() {
			this.Field<StringGraphType>("hero", resolve: context => "");
		}

	}

}
