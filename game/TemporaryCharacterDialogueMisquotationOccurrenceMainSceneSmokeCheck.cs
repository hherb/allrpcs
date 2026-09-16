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
        CharacterDialogueMisquotationEvidenceCandidateController candidateController =
            GetNode<CharacterDialogueMisquotationEvidenceCandidateController>(
                "../CharacterDialogueMisquotationEvidenceCandidateController");
        CharacterDialogueMisquotationEvidenceCandidateRequestController requestController =
            GetNode<CharacterDialogueMisquotationEvidenceCandidateRequestController>(
                "../CharacterDialogueMisquotationEvidenceCandidateRequestController");
        CharacterTurnTranscriptController transcript = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        CharacterDialogueMisquotationEvidenceTaskController evidenceTask =
            GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
                "../CharacterDialogueMisquotationEvidenceTaskController");
        CharacterDialogueMisquotationEvidenceExposureController exposure =
            GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
                "../CharacterDialogueMisquotationEvidenceExposureController");
        CharacterDialogueMisquotationEvidenceViewController view =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidencePresentationSelectionController selection =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        CharacterDialogueMisquotationEvidencePresentationController presentation =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        TemporaryWorldObjectSource worldSource = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource");
        FocusedSearchActivityController perception =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");

        CharacterDialogueMisquotationOccurrence authoredOccurrence = source.Occurrence;
        DialogueEvent authoredDeliveredEvent = source.DeliveredEvent;
        CharacterDialogueMisquotationEvidence authoredCandidate = source.Candidate;
        CharacterTurnTranscriptSnapshot? retainedTranscript = transcript.Transcript;
        CharacterDialogueMisquotationEvidenceExposureSnapshot? retainedExposure =
            exposure.Exposure;
        CharacterDialogueMisquotationEvidenceExposureSnapshot? retainedViewExposure =
            view.SourceExposure;
        CharacterDialogueMisquotationEvidenceViewSnapshot? retainedView = view.View;
        CharacterDialogueMisquotationEvidencePresentationSelection? retainedSelection =
            selection.Selection;
        CharacterDialogueMisquotationEvidencePresentationSnapshot? retainedPresentation =
            presentation.Presentation;
        object retainedWorldObjects = worldSource.WorldObjects;
        object? retainedPerception = perception.Perception;
        CharacterDialogueMisquotationProposal proposal = authoredOccurrence.Proposal;
        DialogueDelivery delivery = new(
            new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
            proposal.SourceEvent.Delivery.Mode);
        DialogueHearingCandidate[] candidates =
        [
            new(source.WitnessCharacterId, new NumericsVector2(32.0f, 0.0f)),
        ];

        bool inertStartup = coordinator.LastOccurrence is null
            && candidateController.Candidate is null
            && requestController.Request is null
            && dialogue.LastOccurredEvent is null
            && evidenceTask.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && evidenceTask.Result is null
            && exposure.Exposure is { Entries.Count: 0 }
            && ReferenceEquals(source.Occurrence, authoredOccurrence)
            && ReferenceEquals(source.DeliveredEvent, authoredDeliveredEvent)
            && ReferenceEquals(source.Candidate, authoredCandidate);
        bool unavailableRejected = !candidateController.TryCreate(
                source.Actor,
                authoredOccurrence,
                source.WitnessCharacterId)
            && candidateController.Candidate is null;
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
        CharacterDialogueMisquotationOccurrence currentOccurrence = coordinator.LastOccurrence!;
        bool staleAndUnheardRejected = !candidateController.TryCreate(
                source.Actor,
                authoredOccurrence,
                source.WitnessCharacterId)
            && !candidateController.TryCreate(
                source.Actor,
                currentOccurrence,
                source.Actor.Identity.Id)
            && candidateController.Candidate is null;
        bool created = candidateController.TryCreate(
                source.Actor,
                currentOccurrence,
                source.WitnessCharacterId)
            && candidateController.Candidate is { } candidate
            && ReferenceEquals(candidate.Occurrence, currentOccurrence)
            && ReferenceEquals(candidate.WitnessCharacterId, source.WitnessCharacterId);
        CharacterDialogueMisquotationEvidence retainedCandidate = candidateController.Candidate!;
        bool requestCreated = !requestController.TryCreate(source.Actor, authoredCandidate)
            && requestController.TryCreate(source.Actor, retainedCandidate)
            && requestController.Request is { } request
            && ReferenceEquals(request.Actor, source.Actor)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates.Single(), retainedCandidate);
        CharacterDialogueMisquotationEvidenceConsiderationRequest retainedRequest =
            requestController.Request!;
        bool oneShotAndUnrelatedStatePreserved = !requestController.TryCreate(
                source.Actor,
                retainedCandidate)
            && !candidateController.TryCreate(
                source.Actor,
                currentOccurrence,
                source.WitnessCharacterId)
            && !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                192.0f,
                48.0f,
                candidates)
            && ReferenceEquals(requestController.Request, retainedRequest)
            && ReferenceEquals(candidateController.Candidate, retainedCandidate)
            && ReferenceEquals(transcript.Transcript, retainedTranscript)
            && evidenceTask.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && evidenceTask.Result is null
            && ReferenceEquals(exposure.Exposure, retainedExposure)
            && ReferenceEquals(view.SourceExposure, retainedViewExposure)
            && ReferenceEquals(view.View, retainedView)
            && ReferenceEquals(selection.Selection, retainedSelection)
            && ReferenceEquals(presentation.Presentation, retainedPresentation)
            && ReferenceEquals(worldSource.WorldObjects, retainedWorldObjects)
            && ReferenceEquals(perception.Perception, retainedPerception)
            && ReferenceEquals(source.Occurrence, authoredOccurrence)
            && ReferenceEquals(source.DeliveredEvent, authoredDeliveredEvent)
            && ReferenceEquals(source.Candidate, authoredCandidate);

        if (!inertStartup
            || !unavailableRejected
            || !occurred
            || !staleAndUnheardRejected
            || !created
            || !requestCreated
            || !oneShotAndUnrelatedStatePreserved)
        {
            Fail("Main-scene misquotation candidate was not inert, exact, and one-shot.");
            return;
        }

        GD.Print(
            "Misquotation candidate main-scene smoke passed: ordinary startup, unavailable, "
            + "stale, unheard, and repeated paths stayed inert; one switch-gated exact delivery "
            + "produced one exact witness candidate and request without changing existing scene "
            + "state or starting consideration.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
