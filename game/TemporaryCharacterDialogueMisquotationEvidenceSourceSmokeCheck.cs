using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact inert temporary misquotation-evidence source.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceSourceSmokeCheck : Node
{
    private const string ActorId = "enemy-agent";
    private const string SourceSpeakerId = "sage";
    private const string WitnessId = "seeker";
    private const string SourceText = "Use the clean herb now.";
    private const string MisquotedText =
        "The sage said the remedy should wait for royal approval.";

    private static readonly string[] ExpectedPropertyNames =
    [
        "Actor",
        "Candidate",
        "DeliveredEvent",
        "Occurrence",
        "SourceEvent",
        "WitnessCharacterId",
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        Character actor = source.Actor;
        DialogueEvent sourceEvent = source.SourceEvent;
        DialogueEvent deliveredEvent = source.DeliveredEvent;
        CharacterDialogueMisquotationOccurrence occurrence = source.Occurrence;
        CharacterDialogueMisquotationProposal proposal = occurrence.Proposal;
        CharacterId witnessCharacterId = source.WitnessCharacterId;
        CharacterDialogueMisquotationEvidence candidate = source.Candidate;
        CharacterDialogueMisquotationEvidence[] candidates = [candidate];

        bool canPropose = CharacterDialogueMisquotationProposalPolicy.CanPropose(actor, proposal);
        bool canOccur = CharacterDialogueMisquotationOccurrencePolicy.CanOccur(actor, occurrence);
        bool canObserve = CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, candidate);
        CharacterDialogueMisquotationEvidence? selected =
            CharacterDialogueMisquotationEvidenceSelection.SelectFirstObservable(actor, candidates);

        if (!HasStableExactOutput(source)
            || actor.Identity.Id.Value != ActorId
            || sourceEvent.Utterance.SpeakerCharacterId.Value != SourceSpeakerId
            || sourceEvent.Utterance.Text != SourceText
            || !ReferenceEquals(sourceEvent.Delivery.Mode, DialogueDeliveryMode.OrdinarySpeech)
            || sourceEvent.Audience.ListenerCharacterIds.Count != 2
            || !ReferenceEquals(sourceEvent.Audience.ListenerCharacterIds[0], actor.Identity.Id)
            || !ReferenceEquals(sourceEvent.Audience.ListenerCharacterIds[1], witnessCharacterId)
            || proposal.MisquotedText != MisquotedText
            || !ReferenceEquals(proposal.ActorId, actor.Identity.Id)
            || !ReferenceEquals(proposal.SourceEvent, sourceEvent)
            || !ReferenceEquals(occurrence.Proposal, proposal)
            || !ReferenceEquals(occurrence.DeliveredEvent, deliveredEvent)
            || !ReferenceEquals(deliveredEvent.Utterance.SpeakerCharacterId, actor.Identity.Id)
            || deliveredEvent.Utterance.Text != proposal.MisquotedText
            || !ReferenceEquals(deliveredEvent.Delivery.Mode, DialogueDeliveryMode.OrdinarySpeech)
            || deliveredEvent.Audience.ListenerCharacterIds.Count != 1
            || !ReferenceEquals(
                deliveredEvent.Audience.ListenerCharacterIds[0],
                witnessCharacterId)
            || witnessCharacterId.Value != WitnessId
            || ReferenceEquals(witnessCharacterId, actor.Identity.Id)
            || !ReferenceEquals(candidate.Occurrence, occurrence)
            || !ReferenceEquals(candidate.WitnessCharacterId, witnessCharacterId)
            || !canPropose
            || !canOccur
            || !canObserve
            || !ReferenceEquals(selected, candidate)
            || candidates.Length != 1
            || !ReferenceEquals(candidates[0], candidate)
            || !HasClosedSourceSurface())
        {
            Fail("The temporary misquotation-evidence source was not exact, eligible, and inert.");
            return;
        }

        GD.Print(
            "Temporary misquotation-evidence source smoke passed: exact enemy, heard source, "
            + "delivered occurrence, non-actor witness candidate, unchanged policy inputs, "
            + "stable references, inert startup, and closed output.");
        GetTree().Quit();
    }

    private static bool HasStableExactOutput(
        TemporaryCharacterDialogueMisquotationEvidenceSource source) =>
        ReferenceEquals(source.Actor, source.Actor)
        && ReferenceEquals(source.SourceEvent, source.SourceEvent)
        && ReferenceEquals(source.DeliveredEvent, source.DeliveredEvent)
        && ReferenceEquals(source.Occurrence, source.Occurrence)
        && ReferenceEquals(source.WitnessCharacterId, source.WitnessCharacterId)
        && ReferenceEquals(source.Candidate, source.Candidate);

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterDialogueMisquotationEvidenceSource);
        string[] propertyNames = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        return propertyNames.SequenceEqual(ExpectedPropertyNames)
            && methods.Length == 1
            && methods[0].Name == nameof(_Ready);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
