namespace GraphQL.Common.Tests.Model
{
	public class Schema
	{
		public Person MainHero { get; set; }

		public Person[] Heroes { get; set; }
	}




	public class SwapiSchema
	{
		public Film Film { get; set; }
		public FilmConnection AllFilms { get; set; }

		public Person Person { get; set; }
		public PeopleConnection PeopleConnection { get; set; }
	}
}
