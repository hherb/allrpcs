namespace Allrpcs.Domain.WorldObjects;

/// <summary>
/// Contains authoritative authored world-object text without implying character perception.
/// </summary>
public sealed record WorldObjectDescription
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumLength = 4_000;

    /// <summary>
    /// Creates a description. Empty text explicitly means no world-object description is authored.
    /// </summary>
    public WorldObjectDescription(string text)
    {
        Text = AuthoredWorldObjectText.RequireBounded(text, MaximumLength, nameof(text));
    }

    /// <summary>
    /// Gets the authored world-object description exactly as supplied.
    /// </summary>
    public string Text { get; }
}
