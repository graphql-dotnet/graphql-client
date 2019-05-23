using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Vehicle {
		public string CargoCapacity { get; set; }
		public string Consumables { get; set; }
		public string CostInCredits { get; set; }
		public DateTime Created { get; set; }
		public string Crew { get; set; }
		public DateTime Edited { get; set; }
		public IQueryable<Film> Films { get; set; }
		public int Id { get; set; }
		public string Length { get; set; }
		public string Manufacturer { get; set; }
		public string MaxAtmospheringSpeed { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public string Passengers { get; set; }
		public IQueryable<Person> Pilots { get; set; }
		public string VehicleClass { get; set; }
	}

	public class VehicleGraphType : ObjectGraphType<Vehicle> {

		public VehicleGraphType() {
			this.Name = nameof(Vehicle);
			this.Field(expression => expression.CargoCapacity);
			this.Field(expression => expression.Consumables);
			this.Field(expression => expression.CostInCredits);
			this.Field(expression => expression.Created);
			this.Field(expression => expression.Crew);
			this.Field(expression => expression.Edited);
			this.Field<ListGraphType<FilmGraphType>>("films");
			this.Field(expression => expression.Id);
			this.Field(expression => expression.Length);
			this.Field(expression => expression.Manufacturer);
			this.Field(expression => expression.MaxAtmospheringSpeed);
			this.Field(expression => expression.Model);
			this.Field(expression => expression.Name);
			this.Field(expression => expression.Passengers);
			this.Field<ListGraphType<PersonGraphType>>("pilots");
			this.Field(expression => expression.VehicleClass);
		}

	}

}
