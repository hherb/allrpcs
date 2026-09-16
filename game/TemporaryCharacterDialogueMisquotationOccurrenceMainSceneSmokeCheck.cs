using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks one explicit misquotation occurrence through the inert main-scene owner.</summary>
public partial class TemporaryCharacterDialogueMisquotationOccurrenceMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_DIALOGUE_MISQUOTATION_OCCURRENCE_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        DialogueEventController dialogue = GetNode<DialogueEventController>(
            "../DialogueEventController");
        CharacterDialogueMisquotationOccurrenceCoordinator coordinator =
            GetNode<CharacterDialogueMisquotationOccurrenceCoordinator>(
                "../CharacterDialogueMisquotationOccurrenceCoordinator");
        CharacterTurnTranscriptController transcript = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        CharacterDialogueMisquotationEvidenceTaskController evidenceTask =
            GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
                "../CharacterDialogueMisquotationEvidenceTaskController");
        CharacterDialogueMisquotationEvidenceExposureController exposure =
            GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
                "../CharacterDialogueMisquotationEvidenceExposureController");

        CharacterDialogueMisquotationOccurrence authoredOccurrence = source.Occurrence;
        DialogueEvent authoredDeliveredEvent = source.DeliveredEvent;
        CharacterDialogueMisquotationEvidence authoredCandidate = source.Candidate;
        CharacterTurnTranscriptSnapshot? retainedTranscript = transcript.Transcript;
        CharacterDialogueMisquotationProposal proposal = authoredOccurrence.Proposal;
        DialogueDelivery delivery = new(
            new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
            proposal.SourceEvent.Delivery.Mode);
        DialogueHearingCandidate[] candidates =
        [
            new(source.WitnessCharacterId, new NumericsVector2(32.0f, 0.0f)),
        ];

        bool inertStartup = coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null
            && evidenceTask.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && evidenceTask.Result is null
            && exposure.Exposure is { Entries.Count: 0 }
            && ReferenceEquals(source.Occurrence, authoredOccurrence)
            && ReferenceEquals(source.DeliveredEvent, authoredDeliveredEvent)
            && ReferenceEquals(source.Candidate, authoredCandidate);
        bool occurred = coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                192.0f,
                48.0f,
                candidates)
            && coordinator.LastOccurrence is { } occurrence
            && ReferenceEquals(occurrence.Proposal, proposal)
            && ReferenceEquals(occurrence.DeliveredEvent, dialogue.LastOccurredEvent)
            && ReferenceEquals(occurrence.DeliveredEvent.Delivery, delivery)
            && ReferenceEquals(
                occurrence.DeliveredEvent.Audience.ListenerCharacterIds.Single(),
                source.WitnessCharacterId);
        bool oneShotAndUnrelatedStatePreserved = !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                192.0f,
                48.0f,
                candidates)
            && ReferenceEquals(transcript.Transcript, retainedTranscript)
            && evidenceTask.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && evidenceTask.Result is null
            && exposure.Exposure is { Entries.Count: 0 }
            && ReferenceEquals(source.Occurrence, authoredOccurrence)
            && ReferenceEquals(source.DeliveredEvent, authoredDeliveredEvent)
            && ReferenceEquals(source.Candidate, authoredCandidate);

        if (!inertStartup || !occurred || !oneShotAndUnrelatedStatePreserved)
        {
            Fail("Main-scene misquotation occurrence was not inert, exact, and one-shot.");
            return;
        }

        GD.Print(
            "Misquotation occurrence main-scene smoke passed: ordinary startup was inert; one "
            + "switch-gated exact delivery occurred once without changing transcripts, evidence "
            + "tasks, exposure, or the existing authored evidence source.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
