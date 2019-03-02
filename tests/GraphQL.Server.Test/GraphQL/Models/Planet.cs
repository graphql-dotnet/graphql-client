using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Planet {

		public string Name { get; set; }

	}

	public class PlanetGraphType : ObjectGraphType<Planet> {

		public PlanetGraphType() {
			this.Name = nameof(Planet);
			this.Field(expression => expression.Name);
		}

	}

}
