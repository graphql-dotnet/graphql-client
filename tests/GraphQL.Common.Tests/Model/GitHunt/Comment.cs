namespace GraphQL.Common.Tests.Model.GitHunt {

	public class Comment {

		public int Id { get; set; }

		public User PostedBy { get; set; }

		public float CreatedAt { get; set; }

		public string Content { get; set; }

		public string RepoName { get; set; }

	}

}
