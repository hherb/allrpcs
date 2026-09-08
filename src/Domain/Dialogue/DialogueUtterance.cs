using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one character's exact in-world dialogue text without interpreting or normalizing it.
/// </summary>
public sealed record DialogueUtterance
{
    /// <summary>
    /// Gets the maximum supported utterance length in UTF-16 code units.
    /// </summary>
    public const int MaximumTextLength = 2_000;

    /// <summary>
    /// Creates an immutable utterance from an existing speaker identifier and exact player text.
    /// </summary>
    public DialogueUtterance(CharacterId speakerCharacterId, string text)
    {
        ArgumentNullException.ThrowIfNull(speakerCharacterId);
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text) || text.Length > MaximumTextLength)
        {
            throw new ArgumentException(
                $"Dialogue text must contain a non-whitespace character and be at most "
                    + $"{MaximumTextLength} UTF-16 code units.",
                nameof(text));
        }

        SpeakerCharacterId = speakerCharacterId;
        Text = text;
    }

    /// <summary>
    /// Gets the stable identifier of the character who spoke the utterance.
    /// </summary>
    public CharacterId SpeakerCharacterId { get; }

    /// <summary>
    /// Gets the exact dialogue text as supplied.
    /// </summary>
    public string Text { get; }
}
