using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Specie {

		public string Name { get; set; }

	}

	public class SpecieGraphType : ObjectGraphType<Specie> {

		public SpecieGraphType() {
			this.Name = nameof(Specie);
			this.Field(expression => expression.Name);
		}

	}

}
