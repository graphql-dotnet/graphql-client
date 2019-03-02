using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Starship {

		public string Name { get; set; }

	}

	public class StarshipGraphType : ObjectGraphType<Starship> {

		public StarshipGraphType() {
			this.Name = nameof(Starship);
			this.Field(expression => expression.Name);
		}

	}

}
