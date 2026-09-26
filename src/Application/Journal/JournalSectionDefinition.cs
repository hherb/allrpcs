namespace Allrpcs.Application.Journal;

/// <summary>Defines one content-free journal section through bounded authored text.</summary>
public sealed class JournalSectionDefinition
{
    /// <summary>Gets the maximum accepted canonical-key length in UTF-16 code units.</summary>
    public const int MaximumKeyLength = 64;

    /// <summary>Gets the maximum accepted display-label length in UTF-16 code units.</summary>
    public const int MaximumDisplayLabelLength = 80;

    /// <summary>Creates one immutable journal-section definition.</summary>
    public JournalSectionDefinition(string key, string displayLabel)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(displayLabel);

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A journal-section key must contain non-whitespace text.", nameof(key));
        }

        if (key.Length > MaximumKeyLength)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        if (string.IsNullOrWhiteSpace(displayLabel))
        {
            throw new ArgumentException(
                "A journal-section display label must contain non-whitespace text.",
                nameof(displayLabel));
        }

        if (displayLabel.Length > MaximumDisplayLabelLength)
        {
            throw new ArgumentOutOfRangeException(nameof(displayLabel));
        }

        Key = key;
        DisplayLabel = displayLabel;
    }

    /// <summary>Gets the exact authored canonical key.</summary>
    public string Key { get; }

    /// <summary>Gets the exact authored display label.</summary>
    public string DisplayLabel { get; }
}
