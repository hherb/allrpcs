namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains authored background text that may be exposed as public character information.
/// </summary>
public sealed record PublicCharacterBackground
{
    /// <summary>
    /// Gets the maximum supported authored background length.
    /// </summary>
    public const int MaximumLength = 4_000;

    /// <summary>
    /// Creates a public background. Empty text explicitly means no public background is authored.
    /// </summary>
    public PublicCharacterBackground(string text)
    {
        Text = AuthoredCharacterText.RequireBounded(text, MaximumLength, nameof(text));
    }

    /// <summary>
    /// Gets the authored public background exactly as supplied.
    /// </summary>
    public string Text { get; }
}
