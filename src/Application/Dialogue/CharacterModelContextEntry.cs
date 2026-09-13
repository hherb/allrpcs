using Allrpcs.Domain.Dialogue;

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
        ValidateKindAndText(kind, text);
        if (IsDialogueKind(kind))
        {
            throw new ArgumentException(
                "Dialogue context must retain an explicit delivery mode.",
                nameof(kind));
        }

        Kind = kind;
        Text = text;
    }

    /// <summary>
    /// Creates one dialogue context entry while retaining its exact text and delivery mode.
    /// </summary>
    public CharacterModelContextEntry(
        CharacterModelContextEntryKind kind,
        string text,
        DialogueDeliveryMode deliveryMode)
    {
        ValidateKindAndText(kind, text);
        ArgumentNullException.ThrowIfNull(deliveryMode);
        if (!IsDialogueKind(kind))
        {
            throw new ArgumentException(
                "Only dialogue context can retain a delivery mode.",
                nameof(kind));
        }

        Kind = kind;
        Text = text;
        DeliveryMode = deliveryMode;
    }

    /// <summary>
    /// Gets the trusted provider-neutral origin and role of the entry.
    /// </summary>
    public CharacterModelContextEntryKind Kind { get; }

    /// <summary>
    /// Gets the exact context text as supplied.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the exact delivery mode for recent or current dialogue context, if applicable.
    /// </summary>
    public DialogueDeliveryMode? DeliveryMode { get; }

    private static bool IsDialogueKind(CharacterModelContextEntryKind kind) =>
        kind is CharacterModelContextEntryKind.RecentSpeech
            or CharacterModelContextEntryKind.UntrustedPlayerDialogue;

    private static void ValidateKindAndText(CharacterModelContextEntryKind kind, string text)
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
    }
}
