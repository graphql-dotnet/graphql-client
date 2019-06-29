using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Server.Test.GraphQL.Models;

namespace GraphQL.Server.Test.GraphQL {

	public static class Storage {

		public static IQueryable<Repository> Repositories { get; } = new List<Repository>()
			.Append(new Repository {
				DatabaseId = 113196300,
				Id = "MDEwOlJlcG9zaXRvcnkxMTMxOTYzMDA=",
				Name = "graphql-client",
				Owner = null,
				Url = new Uri("https://github.com/graphql-dotnet/graphql-client")
			})
			.AsQueryable();

		public static IQueryable<Film> Films { get; set; } = new List<Film>().AsQueryable();

		public static IQueryable<Person> People { get; set; } = new List<Person>().AsQueryable();

		public static IQueryable<Planet> Planets { get; set; } = new List<Planet>().AsQueryable();

		public static IQueryable<Specie> Species { get; set; } = new List<Specie>().AsQueryable();

		public static IQueryable<Starship> Starships { get; set; } = new List<Starship>().AsQueryable();

		public static IQueryable<Vehicle> Vehicles { get; set; } = new List<Vehicle>().AsQueryable();

	}

}
