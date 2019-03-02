using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Film {
		public DateTime Created { get; set; }
		public string Director { get; set; }
		public DateTime Edited { get; set; }
		public int Id { get; set; }
		public string OpeningCrawl { get; set; }
		public string Producer { get; set; }
		public string ReleaseDate { get; set; }
		public string Title { get; set; }
		public List<Specie> Species { get; set; }
		public List<Starship> Starships { get; set; }
		public List<Vehicle> Vehicles { get; set; }
	}

	public class FilmGraphType:ObjectGraphType<Film> {

		public FilmGraphType() {
			this.Name = nameof(Film);
			this.Field(expression => expression.Created);
			this.Field(expression => expression.Director);
			this.Field(expression => expression.Edited);
			this.Field(expression => expression.Id);
			this.Field(expression => expression.OpeningCrawl);
			this.Field(expression => expression.Producer);
			this.Field(expression => expression.ReleaseDate);
			this.Field(expression => expression.Title);
			this.Field<ListGraphType<SpecieGraphType>>("species");
			this.Field<ListGraphType<StarshipGraphType>>("starships");
			this.Field<ListGraphType<VehicleGraphType>>("vehicles");
		}

	}

}
