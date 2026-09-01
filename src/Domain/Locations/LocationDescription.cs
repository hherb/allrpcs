namespace Allrpcs.Domain.Locations;

/// <summary>
/// Contains authoritative authored location text without implying character perception.
/// </summary>
public sealed record LocationDescription
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumLength = 4_000;

    /// <summary>
    /// Creates a description. Empty text explicitly means no location description is authored.
    /// </summary>
    public LocationDescription(string text)
    {
        Text = AuthoredLocationText.RequireBounded(text, MaximumLength, nameof(text));
    }

    /// <summary>
    /// Gets the authored location description exactly as supplied.
    /// </summary>
    public string Text { get; }
}
