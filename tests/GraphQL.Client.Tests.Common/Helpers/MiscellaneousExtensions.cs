using System.Linq;

namespace GraphQL.Client.Tests.Common.Helpers {
	public static class MiscellaneousExtensions {
		public static string RemoveWhitespace(this string input) {
			return new string(input.ToCharArray()
				.Where(c => !char.IsWhiteSpace(c))
				.ToArray());
		}
	}
}
