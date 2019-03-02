using GraphQL.Server.Test.GraphQL.Models;

namespace GraphQL.Server.Test.GraphQL {

	public static class Storage {

		public static Film[] Films { get; } = new[] {
			new Film {
				Director="George Lucas",
				Title="A New Hope"
			}
		};

		public static People[] Peoples { get; } = new[] {
			new People {
				Height=172,
				Mass=77,
				Name="Luke Skywalker"
			}
		};

	}

}
