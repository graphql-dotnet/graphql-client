using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.Common
{
	public static class StringExtensions
	{

		/// <summary>
		/// Returns a camel case version of the string.
		/// </summary>
		/// <param name="s">The source string.</param>
		/// <returns>System.String.</returns>
		public static string ToCamelCase(this string s)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return string.Empty;
			}

			var newFirstLetter = char.ToLowerInvariant(s[0]);
			if (newFirstLetter == s[0])
				return s;

			return newFirstLetter + s.Substring(1);
		}
	}
}
