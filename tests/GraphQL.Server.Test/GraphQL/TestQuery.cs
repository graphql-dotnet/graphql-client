using GraphQL.Server.Test.GraphQL.Models;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL {

	public class TestQuery:ObjectGraphType {

		public TestQuery() {
			this.Field<ListGraphType<FilmGraphType>>("films",
				resolve: context => Storage.Films);
			this.Field<ListGraphType<PersonGraphType>>("people",
				resolve: context => Storage.People);
			this.Field<ListGraphType<PlanetGraphType>>("planets",
				resolve: context => Storage.Planets);
			this.Field<ListGraphType<SpecieGraphType>>("species",
				resolve: context => Storage.Species);
			this.Field<ListGraphType<StarshipGraphType>>("starships",
				resolve: context => Storage.Starships);
			this.Field<ListGraphType<VehicleGraphType>>("vehicles",
				resolve: context => Storage.Vehicles);
		}

	}

}
