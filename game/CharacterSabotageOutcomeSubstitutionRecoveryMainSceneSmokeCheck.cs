using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact eligible substitution outcome through main-scene recovery dependencies.</summary>
public partial class CharacterSabotageOutcomeSubstitutionRecoveryMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_OUTCOME_SUBSTITUTION_RECOVERY_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSubstitutionSource source = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private CharacterSabotageActionSubstitutionRequestController substitutionAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource actionModels = null!;
    private CharacterSabotageActionTaskController actionTask = null!;
    private CharacterSabotageActionRequestConsiderationFlowController actionFlow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private TemporaryCharacterObjectSabotageSubstitutionModelSource substitutionModels = null!;
    private CharacterObjectSabotageSubstitutionTaskController substitutionTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterSabotageOutcomeRecoveryCoordinator recovery = null!;
    private RecordingActionModel actionModel = null!;
    private RecordingSubstitutionModel substitutionModel = null!;
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

        source = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        substitutionAuthor = GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController");
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
        substitutionModels = GetNode<TemporaryCharacterObjectSabotageSubstitutionModelSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionModelSource");
        substitutionTask = GetNode<CharacterObjectSabotageSubstitutionTaskController>(
            "../CharacterObjectSabotageSubstitutionTaskController");
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
        substitutionModel = new();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene substitution recovery exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartActionSelection();
        }
        else if (stage == 1)
        {
            StartSubstitutionSelection();
        }
        else if (stage == 2)
        {
            RecoverSubstitution();
        }
        else
        {
            Fail("Main-scene substitution recovery entered an unknown stage.");
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
            && !recovery.TryRecoverSubstitution(null, null, null)
            && StateIsInitial();
        bool authored = substitutionAuthor.TryCreate(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            source.Substitutions,
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
            || !ReferenceEquals(outputs[2], substitutionAuthor.Request)
            || !composed
            || !started
            || actionTask.Status != CharacterSabotageActionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene substitution author did not start one exact inert generic selection.");
            return;
        }

        stage = 1;
    }

    private void StartSubstitutionSelection()
    {
        if (actionTask.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (actionTask.Status != CharacterSabotageActionTaskStatus.Completed
            || actionTask.Result is not { } result
            || actionModel.CallCount != 1
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Substitute)
            || !selection.TryObserveCompletedResult()
            || !binding.TryBindObservedSelection()
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterObjectSabotageSubstitutionCandidate> selected
            || !ReferenceEquals(selected.Request, substitutionAuthor.Request)
            || !ReferenceEquals(selected.Candidate, source.Candidate)
            || !binding.TryAcknowledgeBoundResult()
            || !specificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || specificRequests.SubstitutionRequest is not { } substitutionRequest
            || !ReferenceEquals(substitutionRequest.Substitutions, source.Substitutions)
            || !ReferenceEquals(substitutionRequest.Evidence, source.Evidence)
            || !ReferenceEquals(substitutionRequest.Candidates.Single(), source.Candidate)
            || !substitutionModels.TrySupplyModel(substitutionModel)
            || !specificStart.TryStart()
            || substitutionTask.Status != CharacterObjectSabotageSubstitutionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene generic substitution selection did not start its exact typed request.");
            return;
        }

        stage = 2;
    }

    private void RecoverSubstitution()
    {
        if (substitutionTask.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        CharacterEvidenceViewSnapshot view = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.Evidence.EvidenceId]);
        CharacterEvidenceViewSnapshot rebuiltView = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.Evidence.EvidenceId]);
        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            source.Character,
            [CharacterSabotageActionKind.Substitute]);
        CharacterObjectSabotageSubstitutionCandidate rebuiltCandidate = new(
            source.Candidate.Substitution,
            source.Candidate.Evidence);

        if (substitutionTask.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || substitutionTask.Result is not { SelectedCandidate: { } selected }
            || !ReferenceEquals(selected, source.Candidate)
            || substitutionModel.CallCount != 1
            || !specificResult.TryObserveCompletedResult()
            || !specificResult.TryAcknowledgeCompletedResult()
            || !specificOutcome.TryBindAcknowledgedOutcome()
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, view)
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.SubstitutionCandidate, source.Candidate)
            || recovery.TryRecoverSubstitution(
                rebuiltAuthor,
                eligibility.SubstitutionCandidate,
                eligibility.EvidenceView)
            || recovery.TryRecoverSubstitution(
                eligibility.AuthorRequest,
                rebuiltCandidate,
                eligibility.EvidenceView)
            || recovery.TryRecoverSubstitution(
                eligibility.AuthorRequest,
                eligibility.SubstitutionCandidate,
                rebuiltView)
            || !recovery.TryRecoverSubstitution(
                eligibility.AuthorRequest,
                eligibility.SubstitutionCandidate,
                eligibility.EvidenceView)
            || recovery.RecoveredSubstitutions is not { } recovered
            || !ReferenceEquals(substitutionState.Substitutions, recovered)
            || recovered.Entries.Count != 0
            || !ReferenceEquals(substitutionState.Evidence, specificOutcome.SubstitutionEvidence)
            || actionModel.CallCount != 1
            || substitutionModel.CallCount != 1
            || recovery.TryRecoverSubstitution(
                eligibility.AuthorRequest,
                eligibility.SubstitutionCandidate,
                eligibility.EvidenceView))
        {
            Fail("Main-scene exact eligible substitution did not recover once without another model call.");
            return;
        }

        GD.Print(
            "Main-scene sabotage outcome substitution recovery smoke passed: ordinary startup stayed "
            + "inert; one exact live substitution selection retained its author, typed candidate, "
            + "acknowledgement, outcome, and evidence-view tokens; and the existing recovery "
            + "transition removed that exact active substitution once while preserving "
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
        GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController"),
        GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController"),
        GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool StateIsInitial() =>
        ReferenceEquals(substitutionState.Substitutions, source.Substitutions)
        && ReferenceEquals(substitutionState.Evidence, source.Evidence)
        && source.Substitutions.Entries.Count == 0
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
                    CharacterSabotageActionKind.Substitute));
        }
    }

    private sealed class RecordingSubstitutionModel
        : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterObjectSabotageSubstitutionDecision(
                    request.Character.Identity.Id,
                    request.Candidates.Single().Evidence.Evidence.EvidenceId));
        }
    }
}
