using System.Text;

namespace AmSoul.Core;

public static class StringExtensions
{
    public static string ToPascalCase(this string? source)
        => string.IsNullOrWhiteSpace(source)
            ? string.Empty
            : SymbolsPipe(
                source,
                '\0',
                (s, i) => new char[] { char.ToUpperInvariant(s) });
    public static string ToCamelCase(this string? source)
        => string.IsNullOrWhiteSpace(source)
            ? string.Empty
            : SymbolsPipe(
                source,
                '\0',
                (s, disableFrontDelimeter) => disableFrontDelimeter ? (new char[] { char.ToLowerInvariant(s) }) : (new char[] { char.ToUpperInvariant(s) }));
    public static string ToKebabCase(this string? source)
        => string.IsNullOrWhiteSpace(source)
            ? string.Empty
            : SymbolsPipe(
                source,
                '-',
                (s, disableFrontDelimeter) => disableFrontDelimeter ? (new char[] { char.ToLowerInvariant(s) }) : (new char[] { '-', char.ToLowerInvariant(s) }));
    public static string ToSnakeCase(this string? source)
        => string.IsNullOrWhiteSpace(source)
            ? string.Empty
            : SymbolsPipe(
                source,
                '_',
                (s, disableFrontDelimeter) => disableFrontDelimeter ? (new char[] { char.ToLowerInvariant(s) }) : (new char[] { '_', char.ToLowerInvariant(s) }));
    public static string ToTrainCase(this string? source)
        => string.IsNullOrWhiteSpace(source)
            ? string.Empty
            : SymbolsPipe(
                source,
                '-',
                (s, disableFrontDelimeter) => disableFrontDelimeter ? (new char[] { char.ToUpperInvariant(s) }) : (new char[] { '-', char.ToUpperInvariant(s) }));
    public static string Format(this string? source, params object[] args)
        => string.IsNullOrWhiteSpace(source)
        ? string.Empty
        : string.Format(source, args);

    private static readonly char[] Delimeters = { ' ', '-', '_' };

    private static string SymbolsPipe(string source, char mainDelimeter, Func<char, bool, char[]> newWordSymbolHandler)
    {
        var builder = new StringBuilder();

        bool nextSymbolStartsNewWord = true;
        bool disableFrontDelimeter = true;
        for (var i = 0; i < source.Length; i++)
        {
            var symbol = source[i];
            if (Delimeters.Contains(symbol))
            {
                if (symbol == mainDelimeter)
                {
                    builder.Append(symbol);
                    disableFrontDelimeter = true;
                }

                nextSymbolStartsNewWord = true;
            }
            else if (!char.IsLetterOrDigit(symbol))
            {
                builder.Append(symbol);
                disableFrontDelimeter = true;
                nextSymbolStartsNewWord = true;
            }
            else
            {
                if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                {
                    builder.Append(newWordSymbolHandler(symbol, disableFrontDelimeter));
                    disableFrontDelimeter = false;
                    nextSymbolStartsNewWord = false;
                }
                else
                {
                    builder.Append(symbol);
                }
            }
        }

        return builder.ToString();
    }
}
