using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL {

	public class TestMutation : ObjectGraphType {

		public TestMutation() {
			this.Field<StringGraphType>("hero", resolve: context => "");
		}

	}

}
