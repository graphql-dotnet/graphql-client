namespace GraphQL.Common.Tests.Model.GitHunt {

	public class Repository {

		public string Name { get; set; }

		public string FullName { get; set; }

		public string Description { get; set; }

		public string HtmlUrl { get; set; }

		public string StargazersCount { get; set; }

		public string OpenIssuesCount { get; set; }

		public User Owner { get; set; }

	}

}
