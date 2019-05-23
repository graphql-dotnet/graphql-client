using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Planet {
		public string Climate { get; set; }
		public DateTime Created { get; set; }
		public string Diameter { get; set; }
		public DateTime Edited { get; set; }
		public IQueryable<Film> Films { get; set; }
		public string Gravity { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string OrbitalPeriod { get; set; }
		public string Population { get; set; }
		public IQueryable<People> Residents { get; set; }
		public string RotationPeriod { get; set; }
		public string SurfaceWater { get; set; }
		public string Terrain { get; set; }
	}

	public class PlanetGraphType : ObjectGraphType<Planet> {

		public PlanetGraphType() {
			this.Name = nameof(Planet);
			this.Field(expression => expression.Climate);
			this.Field(expression => expression.Created);
			this.Field(expression => expression.Diameter);
			this.Field(expression => expression.Edited);
			this.Field<ListGraphType<FilmGraphType>>("films");
			this.Field(expression => expression.Gravity);
			this.Field(expression => expression.Id);
			this.Field(expression => expression.Name);
			this.Field(expression => expression.OrbitalPeriod);
			this.Field(expression => expression.Population);
			this.Field<ListGraphType<PeopleGraphType>>("residents");
			this.Field(expression => expression.RotationPeriod);
			this.Field(expression => expression.SurfaceWater);
			this.Field(expression => expression.Terrain);
		}

	}

}
