using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one misquote action request from exact eligible source state.</summary>
public partial class CharacterSabotageActionMisquotationRequestController
    : Node, ICharacterSabotageActionRequestAuthor
{
    private TemporaryCharacterDialogueMisquotationEvidenceSource source = null!;

    /// <summary>Gets the exact one-shot sabotage-action request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
    }

    /// <summary>Creates one request only for the exact eligible misquotation source graph.</summary>
    public bool TryCreate(
        Character actor,
        DialogueEvent sourceEvent,
        DialogueEvent deliveredEvent,
        CharacterDialogueMisquotationOccurrence occurrence,
        CharacterId witnessCharacterId,
        CharacterDialogueMisquotationEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(sourceEvent);
        ArgumentNullException.ThrowIfNull(deliveredEvent);
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(witnessCharacterId);
        ArgumentNullException.ThrowIfNull(candidate);

        if (Request is not null
            || !ReferenceEquals(source.Actor, actor)
            || !ReferenceEquals(source.SourceEvent, sourceEvent)
            || !ReferenceEquals(source.DeliveredEvent, deliveredEvent)
            || !ReferenceEquals(source.Occurrence, occurrence)
            || !ReferenceEquals(source.WitnessCharacterId, witnessCharacterId)
            || !ReferenceEquals(source.Candidate, candidate)
            || !CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, candidate))
        {
            return false;
        }

        Request = new CharacterSabotageActionConsiderationRequest(
            actor,
            [CharacterSabotageActionKind.Misquote]);
        return true;
    }
}
