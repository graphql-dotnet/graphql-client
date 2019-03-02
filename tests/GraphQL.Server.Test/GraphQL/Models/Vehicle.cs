using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Vehicle {

		public string Name { get; set; }

	}

	public class VehicleGraphType : ObjectGraphType<Vehicle> {

		public VehicleGraphType() {
			this.Name = nameof(Vehicle);
			this.Field(expression => expression.Name);
		}

	}

}
