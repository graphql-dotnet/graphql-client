using System.Collections.Generic;
using System.Linq;
using GraphQL.Server.Test.GraphQL.Models;

namespace GraphQL.Server.Test.GraphQL {

	public static class Storage {

		public static IQueryable<Film> Films { get; set; } = new List<Film>().AsQueryable();

		public static IQueryable<People> Peoples { get; set; } = new List<People>().AsQueryable();

		public static IQueryable<Planet> Planets { get; set; } = new List<Planet>().AsQueryable();

		public static IQueryable<Specie> Species { get; set; } = new List<Specie>().AsQueryable();

		public static IQueryable<Starship> Starships { get; set; } = new List<Starship>().AsQueryable();

		public static IQueryable<Vehicle> Vehicles { get; set; } = new List<Vehicle>().AsQueryable();

	}

}
