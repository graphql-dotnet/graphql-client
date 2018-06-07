namespace GraphQL.Common.Tests.Model.GitHunt {

	public class Entry {

		public Repository Repository { get; set; }

		public User PostedBy { get; set; }

		public float CreatedAt { get; set; }

		public int Score { get; set; }

		public float HotScore { get; set; }

		public Comment[] Comments { get; set; }

		public int CommentCount { get; set; }

		public int Id { get; set; }

		public Vote Vote { get; set; }

	}

}
