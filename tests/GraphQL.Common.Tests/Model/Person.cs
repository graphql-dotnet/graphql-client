namespace GraphQL.Common.Tests.Model {

	public class Person {

		public string[] AppearsIn { get; set; }

		public Person[] Friends { get; set; }

		public double Height { get; set; }

		public string Name { get; set; }

		public string PrimaryFunction { get; set; }


		public FilmConnection FilmConnection { get; set; }

	}

	public class Film
	{
		public string Producer { get; set; }
		public int EpisodeId { get; set; }
		public string OpeningCrawl { get; set; }
		public string Title { get; set; }
		public string Director { get; set; }
		public CharacterConnection CharacterConnection { get; set; }
	}

	public class CharacterConnection
	{
		public int TotalCount { get; set; }
		public Person[] Characters { get; set; }
	}
	public class PeopleConnection
	{
		public int TotalCount { get; set; }
		public Person[] People { get; set; }
	}
	public class FilmConnection
	{
		public int TotalCount { get; set; }
		public Film[] Films { get; set; }
	}


	public class SwapiSchema
	{
		public Film Film { get; set; }
		public FilmConnection AllFilms { get; set; }

		public Person Person { get; set; }
		public PeopleConnection PeopleConnection { get; set; }
	}

}
