using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact eligible contamination outcome through main-scene recovery dependencies.</summary>
public partial class CharacterSabotageOutcomeContaminationRecoveryMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_OUTCOME_CONTAMINATION_RECOVERY_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageContaminationSource source = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private CharacterSabotageActionContaminationRequestController contaminationAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource actionModels = null!;
    private CharacterSabotageActionTaskController actionTask = null!;
    private CharacterSabotageActionRequestConsiderationFlowController actionFlow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private TemporaryCharacterObjectSabotageContaminationModelSource contaminationModels = null!;
    private CharacterObjectSabotageContaminationTaskController contaminationTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterSabotageOutcomeRecoveryCoordinator recovery = null!;
    private RecordingActionModel actionModel = null!;
    private RecordingContaminationModel contaminationModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
            return;
        }

        source = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        contaminationAuthor = GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController");
        authors = GetAuthors();
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        actionModels = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        actionTask = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        actionFlow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        specificRequests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        contaminationModels = GetNode<TemporaryCharacterObjectSabotageContaminationModelSource>(
            "../TemporaryCharacterObjectSabotageContaminationModelSource");
        contaminationTask = GetNode<CharacterObjectSabotageContaminationTaskController>(
            "../CharacterObjectSabotageContaminationTaskController");
        specificStart = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        specificResult = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        specificOutcome = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeRecoveryCoordinator>(
            "../CharacterSabotageOutcomeRecoveryCoordinator");
        actionModel = new();
        contaminationModel = new();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene contamination recovery exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartActionSelection();
        }
        else if (stage == 1)
        {
            StartContaminationSelection();
        }
        else if (stage == 2)
        {
            RecoverContamination();
        }
        else
        {
            Fail("Main-scene contamination recovery entered an unknown stage.");
        }
    }

    private void StartActionSelection()
    {
        bool inert = authors.All(author => author.Request is null)
            && recovery.AuthorRequest is null
            && recovery.EvidenceView is null
            && recovery.RecoveredConcealment is null
            && recovery.RecoveredPlacements is null
            && recovery.RecoveredSubstitutions is null
            && recovery.RecoveredContamination is null
            && !recovery.TryRecoverContamination(null, null, null)
            && StateIsInitial();
        bool authored = contaminationAuthor.TryCreate(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            source.Contamination,
            source.Evidence,
            source.Candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(source.Character, outputs);
        bool started = composer.Request is { } request
            && actionModels.TrySupplyModel(actionModel)
            && actionFlow.TryStart(request);

        if (!inert
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[3], contaminationAuthor.Request)
            || !composed
            || !started
            || actionTask.Status != CharacterSabotageActionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene contamination author did not start one exact inert generic selection.");
            return;
        }

        stage = 1;
    }

    private void StartContaminationSelection()
    {
        if (actionTask.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (actionTask.Status != CharacterSabotageActionTaskStatus.Completed
            || actionTask.Result is not { } result
            || actionModel.CallCount != 1
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Contaminate)
            || !selection.TryObserveCompletedResult()
            || !binding.TryBindObservedSelection()
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
                    selected
            || !ReferenceEquals(selected.Request, contaminationAuthor.Request)
            || !ReferenceEquals(selected.Candidate, source.Candidate)
            || !binding.TryAcknowledgeBoundResult()
            || !specificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || specificRequests.ContaminationRequest is not { } contaminationRequest
            || !ReferenceEquals(contaminationRequest.Contamination, source.Contamination)
            || !ReferenceEquals(contaminationRequest.Evidence, source.Evidence)
            || !ReferenceEquals(contaminationRequest.Candidates.Single(), source.Candidate)
            || !contaminationModels.TrySupplyModel(contaminationModel)
            || !specificStart.TryStart()
            || contaminationTask.Status != CharacterObjectSabotageContaminationTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene generic contamination selection did not start its exact typed request.");
            return;
        }

        stage = 2;
    }

    private void RecoverContamination()
    {
        if (contaminationTask.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        CharacterEvidenceViewSnapshot view = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.EvidenceId]);
        CharacterEvidenceViewSnapshot rebuiltView = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.EvidenceId]);
        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            source.Character,
            [CharacterSabotageActionKind.Contaminate]);
        CharacterObjectSabotageEvidence rebuiltCandidate = new(
            source.Candidate.Occurrence,
            source.Candidate.Evidence);

        if (contaminationTask.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || contaminationTask.Result is not { SelectedCandidate: { } selected }
            || !ReferenceEquals(selected, source.Candidate)
            || contaminationModel.CallCount != 1
            || !specificResult.TryObserveCompletedResult()
            || !specificResult.TryAcknowledgeCompletedResult()
            || !specificOutcome.TryBindAcknowledgedOutcome()
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, view)
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.ContaminationCandidate, source.Candidate)
            || recovery.TryRecoverContamination(
                rebuiltAuthor,
                eligibility.ContaminationCandidate,
                eligibility.EvidenceView)
            || recovery.TryRecoverContamination(
                eligibility.AuthorRequest,
                rebuiltCandidate,
                eligibility.EvidenceView)
            || recovery.TryRecoverContamination(
                eligibility.AuthorRequest,
                eligibility.ContaminationCandidate,
                rebuiltView)
            || !recovery.TryRecoverContamination(
                eligibility.AuthorRequest,
                eligibility.ContaminationCandidate,
                eligibility.EvidenceView)
            || recovery.RecoveredContamination is not { } recovered
            || !ReferenceEquals(contaminationState.Contamination, recovered)
            || recovered.ContaminatedObjectIds.Count != 0
            || !ReferenceEquals(contaminationState.Evidence, specificOutcome.ContaminationEvidence)
            || actionModel.CallCount != 1
            || contaminationModel.CallCount != 1
            || recovery.TryRecoverContamination(
                eligibility.AuthorRequest,
                eligibility.ContaminationCandidate,
                eligibility.EvidenceView))
        {
            Fail("Main-scene exact eligible contamination did not recover once without another model call.");
            return;
        }

        GD.Print(
            "Main-scene sabotage outcome contamination recovery smoke passed: ordinary startup "
            + "stayed inert; one exact live contamination selection retained its author, typed "
            + "candidate, acknowledgement, outcome, and evidence-view tokens; and the existing "
            + "recovery transition removed that exact contamination once while preserving "
            + "authoritative evidence and both model call counts.");
        GetTree().Quit();
    }

    private ICharacterSabotageActionRequestAuthor[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController"),
        GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController"),
        GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        contaminationAuthor,
        GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController"),
        GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool StateIsInitial() =>
        ReferenceEquals(contaminationState.Contamination, source.Contamination)
        && ReferenceEquals(contaminationState.Evidence, source.Evidence)
        && source.Contamination.ContaminatedObjectIds.Count == 0
        && source.Evidence.Entries.Count == 0;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingActionModel : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    request.Actor.Identity.Id,
                    CharacterSabotageActionKind.Contaminate));
        }
    }

    private sealed class RecordingContaminationModel
        : ICharacterObjectSabotageContaminationDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageContaminationDecision(
                    request.Character.Identity.Id,
                    request.Candidates.Single().Evidence.EvidenceId));
        }
    }
}
