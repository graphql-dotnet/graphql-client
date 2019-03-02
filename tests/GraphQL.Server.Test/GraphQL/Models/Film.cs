using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models {

	public class Film {

		public string Director { get; set; }

		public int Id { get; set; }

		public string Title { get; set; }

	}

	public class FilmGraphType:ObjectGraphType<Film> {

		public FilmGraphType() {
			this.Name = nameof(Film);
			this.Field(expression => expression.Director);
			this.Field(expression => expression.Id);
			this.Field(expression => expression.Title);
		}

	}

}
