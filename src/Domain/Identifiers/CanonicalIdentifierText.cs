namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Defines the canonical text contract shared by every stable domain identifier.
/// </summary>
public static class CanonicalIdentifierText
{
    /// <summary>
    /// Gets the maximum supported canonical identifier length.
    /// </summary>
    public const int MaximumLength = 64;

    /// <summary>
    /// Reports whether text is a canonical identifier: one to 64 lowercase ASCII characters,
    /// beginning with a letter and containing only letters, digits, and single internal hyphens.
    /// </summary>
    public static bool IsCanonical(string? value)
    {
        if (string.IsNullOrEmpty(value)
            || value.Length > MaximumLength
            || !IsLowerAsciiLetter(value[0]))
        {
            return false;
        }

        bool previousWasHyphen = false;

        for (int index = 1; index < value.Length; index++)
        {
            char character = value[index];

            if (IsLowerAsciiLetter(character) || IsAsciiDigit(character))
            {
                previousWasHyphen = false;
                continue;
            }

            if (character != '-' || previousWasHyphen || index == value.Length - 1)
            {
                return false;
            }

            previousWasHyphen = true;
        }

        return true;
    }

    internal static string RequireCanonical(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (!IsCanonical(value))
        {
            throw new ArgumentException(
                $"Identifier text must be 1 to {MaximumLength} characters, begin with a lowercase "
                    + "ASCII letter, and contain only lowercase ASCII letters, digits, and single "
                    + "internal hyphens.",
                parameterName);
        }

        return value;
    }

    private static bool IsLowerAsciiLetter(char character) => character is >= 'a' and <= 'z';

    private static bool IsAsciiDigit(char character) => character is >= '0' and <= '9';
}
