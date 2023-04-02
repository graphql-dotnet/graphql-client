namespace GraphQL.Client.Abstractions.Utilities;

/// <summary>
/// Copied from https://github.com/jquense/StringUtils
/// </summary>
public static class StringExtensions
{
    public static string StripIndent(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.StripIndent(str);

    public static IEnumerable<string> ToWords(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToWords(str);

    public static string ToUpperFirst(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToUpperFirst(str);

    public static string ToLowerFirst(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToLowerFirst(str);

    public static string Capitalize(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.Capitalize(str);

    public static string ToCamelCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToCamelCase(str);

    public static string ToConstantCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToConstantCase(str);

    public static string ToUpperCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToUpperCase(str);

    public static string ToLowerCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToLowerCase(str);


    public static string ToPascalCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToPascalCase(str);


    public static string ToKebabCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToKebabCase(str);


    public static string ToSnakeCase(this string str) => GraphQL.Client.Abstractions.Utilities.StringUtils.ToSnakeCase(str);
}
