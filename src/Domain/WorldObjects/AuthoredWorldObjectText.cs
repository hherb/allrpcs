namespace Allrpcs.Domain.WorldObjects;

/// <summary>
/// Validates authored world-object text without normalizing or rewriting it.
/// </summary>
internal static class AuthoredWorldObjectText
{
    internal static string RequireNonBlank(
        string value,
        int maximumLength,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
        {
            throw new ArgumentException(
                $"Text must contain a non-whitespace character and be at most {maximumLength} "
                    + "UTF-16 code units.",
                parameterName);
        }

        return value;
    }

    internal static string RequireBounded(
        string value,
        int maximumLength,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length > maximumLength)
        {
            throw new ArgumentException(
                $"Text must be at most {maximumLength} UTF-16 code units.",
                parameterName);
        }

        return value;
    }
}
