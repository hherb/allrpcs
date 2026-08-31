namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains authored background text that must remain private character information.
/// </summary>
public sealed record PrivateCharacterBackground
{
    /// <summary>
    /// Gets the maximum supported authored background length.
    /// </summary>
    public const int MaximumLength = PublicCharacterBackground.MaximumLength;

    /// <summary>
    /// Creates a private background. Empty text explicitly means no private background is authored.
    /// </summary>
    public PrivateCharacterBackground(string text)
    {
        Text = AuthoredCharacterText.RequireBounded(text, MaximumLength, nameof(text));
    }

    /// <summary>
    /// Gets the authored private background exactly as supplied.
    /// </summary>
    public string Text { get; }
}
