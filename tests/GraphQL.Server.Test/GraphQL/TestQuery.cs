using GraphQL.Server.Test.GraphQL.Models;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL {

	public class TestQuery:ObjectGraphType {

		public TestQuery() {
			this.Field<ListGraphType<FilmGraphType>>("films",
				resolve: context => Storage.Films);
			this.Field<ListGraphType<PeopleGraphType>>("peoples",
				resolve: context => Storage.Peoples);
		}

	}

}
