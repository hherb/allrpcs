using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary observable enemy misquotation-evidence candidate.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceSource : Node
{
    private const string SourceSpeakerIdText = "sage";
    private const string WitnessCharacterIdText = "seeker";
    private const string SourceText = "Use the clean herb now.";
    private const string MisquotedText =
        "The sage said the remedy should wait for royal approval.";
    private const string RequiredCapabilityKey = "assess-local-influence";

    /// <summary>Gets the exact authored enemy actor.</summary>
    public Character Actor { get; private set; } = null!;

    /// <summary>Gets the exact authored source event heard by the actor and witness.</summary>
    public DialogueEvent SourceEvent { get; private set; } = null!;

    /// <summary>Gets the exact authored delivered misquotation event.</summary>
    public DialogueEvent DeliveredEvent { get; private set; } = null!;

    /// <summary>Gets the exact authored misquotation occurrence.</summary>
    public CharacterDialogueMisquotationOccurrence Occurrence { get; private set; } = null!;

    /// <summary>Gets the exact authored non-actor witness identifier.</summary>
    public CharacterId WitnessCharacterId { get; private set; } = null!;

    /// <summary>Gets the exact authored observable witness-evidence candidate.</summary>
    public CharacterDialogueMisquotationEvidence Candidate { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId sourceSpeakerId = new(SourceSpeakerIdText);
        WitnessCharacterId = new CharacterId(WitnessCharacterIdText);
        SourceEvent = new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, SourceText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                sourceSpeakerId,
                [Actor.Identity.Id, WitnessCharacterId]));
        CharacterDialogueMisquotationProposal proposal = new(
            Actor.Identity.Id,
            SourceEvent,
            MisquotedText,
            RequiredCapabilityKey);
        DeliveredEvent = new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(Actor.Identity.Id, proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(Actor.Identity.Id, [WitnessCharacterId]));
        Occurrence = new CharacterDialogueMisquotationOccurrence(proposal, DeliveredEvent);
        Candidate = new CharacterDialogueMisquotationEvidence(Occurrence, WitnessCharacterId);
    }
}
