using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class People {

		public int Height { get; set; }

		public int Mass { get; set; }

		public string Name { get; set; }

	}

	public class PeopleGraphType : ObjectGraphType<People> {

		public PeopleGraphType() {
			this.Name = nameof(People);
			this.Field(expression => expression.Height);
			this.Field(expression => expression.Mass);
			this.Field(expression => expression.Name);
		}

	}

}
