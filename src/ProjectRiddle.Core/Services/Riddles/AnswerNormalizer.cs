using System.Globalization;
using System.Text.RegularExpressions;

namespace ProjectRiddle.Core.Services.Riddles;

/// <summary>
/// Normalizes authored answers for pattern validation and later answer checking.
/// </summary>
public static partial class AnswerNormalizer
{
    /// <summary>
    /// Trims, collapses whitespace, and uppercases an answer using Bulgarian culture.
    /// </summary>
    /// <param name="answer">The answer text. Cannot be <see langword="null" />.</param>
    /// <returns>The normalized answer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="answer" /> is <see langword="null" />.</exception>
    public static string Normalize(string answer)
    {
        ArgumentNullException.ThrowIfNull(answer);

        var collapsed = WhitespacePattern().Replace(answer.Trim(), " ");
        return collapsed.ToUpper(CultureInfo.GetCultureInfo("bg-BG"));
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespacePattern();
}
