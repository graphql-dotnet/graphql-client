using System;

namespace IntegrationTestServer.ChatSchema {
	public class ReceivedMessage {
		public string FromId { get; set; }

		public string Content { get; set; }

		public DateTime SentAt { get; set; }
	}
}
