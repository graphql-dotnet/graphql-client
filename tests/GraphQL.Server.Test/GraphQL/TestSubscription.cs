using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL {

	public class TestSubscription : ObjectGraphType {

		public TestSubscription() {
			this.Field<StringGraphType>("hero", resolve: context => "");
		}

	}

}
