using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.Client.Tests.Common.Chat {
	public class AddMessageVariables {

		public AddMessageInput Input { get; set; }
		public class AddMessageInput {
			public string FromId { get; set; }
			public string Content { get; set; }
			public DateTime SentAt { get; set; }
		}
	}
}
