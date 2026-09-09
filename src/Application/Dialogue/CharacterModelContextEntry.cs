namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one typed, bounded piece of provider-neutral character-model context.
/// </summary>
public sealed record CharacterModelContextEntry
{
    /// <summary>
    /// Gets the maximum supported entry length in UTF-16 code units.
    /// </summary>
    public const int MaximumTextLength = 4_000;

    /// <summary>
    /// Creates one context entry while retaining its exact text.
    /// </summary>
    public CharacterModelContextEntry(CharacterModelContextEntryKind kind, string text)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaximumTextLength)
        {
            throw new ArgumentException(
                $"Model context must contain a non-whitespace character and be at most "
                    + $"{MaximumTextLength} UTF-16 code units.",
                nameof(text));
        }

        Kind = kind;
        Text = text;
    }

    /// <summary>
    /// Gets the trusted provider-neutral origin and role of the entry.
    /// </summary>
    public CharacterModelContextEntryKind Kind { get; }

    /// <summary>
    /// Gets the exact context text as supplied.
    /// </summary>
    public string Text { get; }
}
