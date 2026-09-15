using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Proposes one capability-gated differing quotation of an exact heard dialogue event.</summary>
public sealed class CharacterDialogueMisquotationProposal
{
    /// <summary>Creates one immutable misquotation proposal without changing its source event.</summary>
    public CharacterDialogueMisquotationProposal(
        CharacterId actorId,
        DialogueEvent sourceEvent,
        string misquotedText,
        string requiredCapabilityKey)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(sourceEvent);
        ArgumentNullException.ThrowIfNull(misquotedText);

        if (string.IsNullOrWhiteSpace(misquotedText)
            || misquotedText.Length > DialogueUtterance.MaximumTextLength
            || StringComparer.Ordinal.Equals(misquotedText, sourceEvent.Utterance.Text))
        {
            throw new ArgumentException(
                "Misquoted text must be non-empty, bounded, and differ from the source utterance.",
                nameof(misquotedText));
        }

        ActorId = actorId;
        SourceEvent = sourceEvent;
        MisquotedText = misquotedText;
        RequiredCapabilityKey = CanonicalIdentifierText.RequireCanonical(
            requiredCapabilityKey,
            nameof(requiredCapabilityKey));
        ActionKind = CharacterSabotageActionKind.Misquote;
    }

    /// <summary>Gets the exact character proposing the misquotation.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact prior in-world dialogue event being misquoted.</summary>
    public DialogueEvent SourceEvent { get; }

    /// <summary>Gets the exact differing quotation text without creating a new utterance.</summary>
    public string MisquotedText { get; }

    /// <summary>Gets the canonical authored capability required to formulate the misquotation.</summary>
    public string RequiredCapabilityKey { get; }

    /// <summary>Gets the exact closed sabotage kind represented by this proposal.</summary>
    public CharacterSabotageActionKind ActionKind { get; }
}
