using System.Collections.Generic;
using System.Linq;
using GraphQL.Server.Test.GraphQL.Models;

namespace GraphQL.Server.Test.GraphQL {

	public static class Storage {

		public static Film[] Films { get; } = new[] {
			new Film {
				Director="George Lucas",
				Title="A New Hope"
			}
		};

		public static IQueryable<People> Peoples { get; set; } = new List<People>().AsQueryable();

		public static Planet[] Planets { get; } = new[] {
			new Planet {
				Name="Tatooine"
			}
		};

		public static Specie[] Species { get; } = new[] {
			new Specie {
				Name="Wookie"
			}
		};

		public static Starship[] Starships { get; } = new[] {
			new Starship {
				Name="Death Star"
			}
		};

		public static Vehicle[] Vehicles { get; } = new[] {
			new Vehicle {
				Name="Sand Crawler"
			}
		};

	}

}
