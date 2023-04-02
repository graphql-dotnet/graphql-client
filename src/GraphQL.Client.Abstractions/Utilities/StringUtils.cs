using System.Text.RegularExpressions;

namespace GraphQL.Client.Abstractions.Utilities;

/// <summary>
/// Copied from https://github.com/jquense/StringUtils
/// </summary>
public static class StringUtils
{
    private static readonly Regex _reWords = new Regex(@"[A-Z\xc0-\xd6\xd8-\xde]?[a-z\xdf-\xf6\xf8-\xff]+(?:['’](?:d|ll|m|re|s|t|ve))?(?=[\xac\xb1\xd7\xf7\x00-\x2f\x3a-\x40\x5b-\x60\x7b-\xbf\u2000-\u206f \t\x0b\f\xa0\ufeff\n\r\u2028\u2029\u1680\u180e\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000]|[A-Z\xc0-\xd6\xd8-\xde]|$)|(?:[A-Z\xc0-\xd6\xd8-\xde]|[^\ud800-\udfff\xac\xb1\xd7\xf7\x00-\x2f\x3a-\x40\x5b-\x60\x7b-\xbf\u2000-\u206f \t\x0b\f\xa0\ufeff\n\r\u2028\u2029\u1680\u180e\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000\d+\u2700-\u27bfa-z\xdf-\xf6\xf8-\xffA-Z\xc0-\xd6\xd8-\xde])+(?:['’](?:D|LL|M|RE|S|T|VE))?(?=[\xac\xb1\xd7\xf7\x00-\x2f\x3a-\x40\x5b-\x60\x7b-\xbf\u2000-\u206f \t\x0b\f\xa0\ufeff\n\r\u2028\u2029\u1680\u180e\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000]|[A-Z\xc0-\xd6\xd8-\xde](?:[a-z\xdf-\xf6\xf8-\xff]|[^\ud800-\udfff\xac\xb1\xd7\xf7\x00-\x2f\x3a-\x40\x5b-\x60\x7b-\xbf\u2000-\u206f \t\x0b\f\xa0\ufeff\n\r\u2028\u2029\u1680\u180e\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000\d+\u2700-\u27bfa-z\xdf-\xf6\xf8-\xffA-Z\xc0-\xd6\xd8-\xde])|$)|[A-Z\xc0-\xd6\xd8-\xde]?(?:[a-z\xdf-\xf6\xf8-\xff]|[^\ud800-\udfff\xac\xb1\xd7\xf7\x00-\x2f\x3a-\x40\x5b-\x60\x7b-\xbf\u2000-\u206f \t\x0b\f\xa0\ufeff\n\r\u2028\u2029\u1680\u180e\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000\d+\u2700-\u27bfa-z\xdf-\xf6\xf8-\xffA-Z\xc0-\xd6\xd8-\xde])+(?:['’](?:d|ll|m|re|s|t|ve))?|[A-Z\xc0-\xd6\xd8-\xde]+(?:['’](?:D|LL|M|RE|S|T|VE))?|\d+|(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff])[\ufe0e\ufe0f]?(?:[\u0300-\u036f\ufe20-\ufe23\u20d0-\u20f0]|\ud83c[\udffb-\udfff])?(?:\u200d(?:[^\ud800-\udfff]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff])[\ufe0e\ufe0f]?(?:[\u0300-\u036f\ufe20-\ufe23\u20d0-\u20f0]|\ud83c[\udffb-\udfff])?)*");
    private static readonly Regex _reIndent = new Regex(@"^[ \t]*(?=\S)", RegexOptions.Multiline);

    /// <summary>
    /// Removes the leading indent from a multi-line string
    /// </summary>
    /// <param name="str">String</param>
    /// <returns></returns>
    public static string StripIndent(string str)
    {
        int indent = _reIndent.Matches(str).Cast<Match>().Select(m => m.Value.Length).Min();
        return new Regex(@"^[ \t]{" + indent + "}", RegexOptions.Multiline).Replace(str, "");
    }

    /// <summary>
    /// Split a cased string into a series of "words" excluding the seperator.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static IEnumerable<string> ToWords(string str)
    {
        foreach (Match match in _reWords.Matches(str))
        {
            yield return match.Value;
        }
    }

    /// <summary>
    /// Uppercase the first character in a string, leaving the rest of the string as is
    /// </summary>
    /// <param name="str"></param>
    /// <returns>a string with the first character uppercased</returns>
    public static string ToUpperFirst(string str) => ChangeCaseFirst(str, c => c.ToUpperInvariant());

    /// <summary>
    /// Lowercase the first character in a string, leaving the rest of the string as is
    /// </summary>
    /// <param name="str"></param>
    /// <returns>a string with the first character lowercased</returns>
    public static string ToLowerFirst(string str) => ChangeCaseFirst(str, c => c.ToLowerInvariant());

    /// <summary>
    /// Capitalizes a string, lowercasing the entire string and uppercasing the first character
    /// </summary>
    /// <param name="str"></param>
    /// <returns>a capitalized string</returns>
    public static string Capitalize(string str) => ToUpperFirst(str.ToLowerInvariant());

    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToCamelCase("FOOBAR")  // "foobar"</code>
    ///     <code>StringUtils.ToCamelCase("FOO_BAR") // "fooBar"</code>
    ///     <code>StringUtils.ToCamelCase("FooBar")  // "fooBar"</code>
    ///     <code>StringUtils.ToCamelCase("foo bar") // "fooBar"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToCamelCase(string str) =>
        ChangeCase(str, (word, index) =>
            (index == 0 ? word.ToLowerInvariant() : Capitalize(word)));

    /// <summary>
    /// Convert a string to CONSTANT_CASE
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToConstantCase("fOo BaR") // "FOO_BAR"</code>
    ///     <code>StringUtils.ToConstantCase("FooBar")  // "FOO_BAR"</code>
    ///     <code>StringUtils.ToConstantCase("Foo Bar") // "FOO_BAR"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToConstantCase(string str) => ChangeCase(str, "_", w => w.ToUpperInvariant());

    /// <summary>
    /// Convert a string to UPPERCASE
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToUpperCase("foobar")  // "FOOBAR"</code>
    ///     <code>StringUtils.ToUpperCase("FOO_BAR") // "FOO BAR"</code>
    ///     <code>StringUtils.ToUpperCase("FooBar")  // "FOO BAR"</code>
    ///     <code>StringUtils.ToUpperCase("Foo Bar") // "FOO BAR"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToUpperCase(string str) => ChangeCase(str, " ", (word) => word.ToUpperInvariant());

    /// <summary>
    /// Convert a string to lowercase
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToLowerCase("FOOBAR")  // "foobar"</code>
    ///     <code>StringUtils.ToLowerCase("FOO_BAR") // "foo bar"</code>
    ///     <code>StringUtils.ToLowerCase("FooBar")  // "foo bar"</code>
    ///     <code>StringUtils.ToLowerCase("Foo Bar") // "foo bar"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToLowerCase(string str) => ChangeCase(str, " ", word => word.ToLowerInvariant());

    /// <summary>
    /// convert a string to PascalCase
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToPascalCase("FOOBAR")  // "FooBar"</code>
    ///     <code>StringUtils.ToPascalCase("FOO_BAR") // "FooBar"</code>
    ///     <code>StringUtils.ToPascalCase("fooBar")  // "FooBar"</code>
    ///     <code>StringUtils.ToPascalCase("Foo Bar") // "FooBar"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToPascalCase(string str) => ChangeCase(str, Capitalize);

    /// <summary>
    /// convert a string to kebab-case
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToKebabCase("FOOBAR")  // "foo-bar"</code>
    ///     <code>StringUtils.ToKebabCase("FOO_BAR") // "foo-bar"</code>
    ///     <code>StringUtils.ToKebabCase("fooBar")  // "foo-bar"</code>
    ///     <code>StringUtils.ToKebabCase("Foo Bar") // "foo-bar"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToKebabCase(string str) => ChangeCase(str, "-", word => word.ToLowerInvariant());

    /// <summary>
    /// convert a string to snake_case
    /// </summary>
    /// <example>
    ///     <code>StringUtils.ToSnakeCase("FOOBAR")  // "foo_bar"</code>
    ///     <code>StringUtils.ToSnakeCase("FOO_BAR") // "foo_bar"</code>
    ///     <code>StringUtils.ToSnakeCase("fooBar")  // "foo_bar"</code>
    ///     <code>StringUtils.ToSnakeCase("Foo Bar") // "foo_bar"</code>
    /// </example>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSnakeCase(string str) => ChangeCase(str, "_", word => word.ToLowerInvariant());

    public static string ChangeCase(string str, Func<string, string> composer) => ChangeCase(str, "", composer);

    public static string ChangeCase(string str, string sep, Func<string, string> composer) => ChangeCase(str, sep, (w, i) => composer(w));

    public static string ChangeCase(string str, Func<string, int, string> composer) => ChangeCase(str, "", composer);

    /// <summary>
    /// Convert a string to a new case
    /// </summary>
    /// <example>
    /// Convert a string to inverse camelCase: CAMELcASE
    ///     <code>
    ///         StringUtils.ChangeCase("my string", "", (word, index) => {
    ///             word = word.ToUpperInvariant();
    ///             if (index > 0)
    ///                 word = StringUtils.toLowerFirst(word);
    ///             return word
    ///         });
    ///         // "MYsTRING"
    ///     </code>
    /// </example>
    /// <param name="str">an input string </param>
    /// <param name="sep">a seperator string used between "words" in the string</param>
    /// <param name="composer">a function that converts individual words to a new case</param>
    /// <returns></returns>
    public static string ChangeCase(string str, string sep, Func<string, int, string> composer)
    {
        string result = "";
        int index = 0;

        foreach (string word in ToWords(str))
        {
            result += ((index == 0 ? "" : sep) + composer(word, index++));
        }

        return result;
    }

    private static string ChangeCaseFirst(string str, Func<string, string> change) => change(str.Substring(0, 1)) + str.Substring(1);
}
