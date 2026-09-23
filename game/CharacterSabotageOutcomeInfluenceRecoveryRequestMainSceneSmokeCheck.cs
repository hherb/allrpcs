using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact generic Influence outcome through recovery-request composition.</summary>
public partial class CharacterSabotageOutcomeInfluenceRecoveryRequestMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_OUTCOME_INFLUENCE_RECOVERY_REQUEST_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private CharacterSabotageActionInfluenceRequestController influenceAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource actionModels = null!;
    private CharacterSabotageActionTaskController actionTask = null!;
    private CharacterSabotageActionRequestConsiderationFlowController actionFlow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private TemporaryCharacterTargetSabotageInfluenceModelSource influenceModels = null!;
    private CharacterTargetSabotageInfluenceTaskController influenceTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterSabotageOutcomeInfluenceRecoveryCoordinator recovery = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateController recoveryCandidates = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateRequestController recoveryRequests =
        null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController recoveryTask = null!;
    private CharacterTargetSabotageInfluenceRecoveryController recoveryState = null!;
    private CharacterTargetSabotageInfluenceConsiderationRequest request = null!;
    private CharacterTargetSabotageInfluenceCandidate candidate = null!;
    private RecordingActionModel actionModel = null!;
    private RecordingInfluenceModel influenceModel = null!;
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

        influenceAuthor = GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController");
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
        influenceModels = GetNode<TemporaryCharacterTargetSabotageInfluenceModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceModelSource");
        influenceTask = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
        specificStart = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        specificResult = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        specificOutcome = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeInfluenceRecoveryCoordinator>(
            "../CharacterSabotageOutcomeInfluenceRecoveryCoordinator");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        recoveryCandidates = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
        recoveryRequests =
            GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        recoveryTask = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        recoveryState = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        request = CreateRequest(out candidate);
        actionModel = new();
        influenceModel = new();
        if (!influenceState.TryInitialize(request.Relationship))
        {
            Fail("Main-scene Influence recovery request rejected authority initialization.");
            return;
        }

        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (++processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene Influence recovery request exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartActionSelection();
        }
        else if (stage == 1)
        {
            StartInfluenceSelection();
        }
        else if (stage == 2)
        {
            ComposeRecoveryRequest();
        }
        else
        {
            Fail("Main-scene Influence recovery request entered an unknown stage.");
        }
    }

    private void StartActionSelection()
    {
        bool inert = authors.All(author => author.Request is null)
            && recovery.AuthorRequest is null
            && recovery.Request is null
            && recoveryCandidates.Candidate is null
            && recoveryRequests.Request is null
            && recoveryTask.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            && recoveryState.Relationship is null
            && recoveryState.LatestAppliedResult is null
            && !recovery.TryCompose(null, null);
        bool authored = influenceAuthor.TryCreate(
            request,
            request.Actor,
            request.InteractionAccess,
            request.Relationship,
            candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool started = composer.TryCompose(request.Actor, outputs)
            && actionModels.TrySupplyModel(actionModel)
            && actionFlow.TryStart(composer.Request!);

        if (!inert
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[6], influenceAuthor.Request)
            || !started
            || actionTask.Status != CharacterSabotageActionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene Influence author did not start one exact generic selection.");
            return;
        }

        stage = 1;
    }

    private void StartInfluenceSelection()
    {
        if (actionTask.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (actionTask.Status != CharacterSabotageActionTaskStatus.Completed
            || actionTask.Result is not { } actionResult
            || !ReferenceEquals(
                actionResult.SelectedActionKind,
                CharacterSabotageActionKind.Influence)
            || !selection.TryObserveCompletedResult()
            || !binding.TryBindObservedSelection()
            || !binding.TryAcknowledgeBoundResult()
            || !specificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || specificRequests.InfluenceRequest is not { } influenceRequest
            || !ReferenceEquals(influenceRequest.Relationship, request.Relationship)
            || !ReferenceEquals(influenceRequest.Candidates.Single(), candidate)
            || !influenceModels.TrySupplyModel(influenceModel)
            || !specificStart.TryStart()
            || influenceTask.Status != CharacterTargetSabotageInfluenceTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene generic Influence selection did not start its exact typed request.");
            return;
        }

        stage = 2;
    }

    private void ComposeRecoveryRequest()
    {
        if (influenceTask.Status == CharacterTargetSabotageInfluenceTaskStatus.Running)
        {
            return;
        }

        CharacterRelationshipChange recoveryChange = CreateRecoveryChange();
        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            request.Actor,
            [CharacterSabotageActionKind.Influence]);
        if (influenceTask.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || !specificResult.TryObserveCompletedResult()
            || !specificResult.TryAcknowledgeCompletedResult()
            || !specificOutcome.TryBindAcknowledgedOutcome()
            || specificOutcome.Influence is not { } outcome
            || !recoveryCandidates.TryCreate(outcome, recoveryChange)
            || recoveryCandidates.Candidate is not { } recoveryCandidate
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, null)
            || recovery.TryCompose(rebuiltAuthor, recoveryCandidate)
            || recovery.TryCompose(
                eligibility.AuthorRequest,
                new CharacterTargetSabotageInfluenceRecoveryCandidate(
                    outcome,
                    recoveryChange))
            || !recovery.TryCompose(eligibility.AuthorRequest, recoveryCandidate)
            || recovery.Request is not { } recoveryRequest
            || !ReferenceEquals(recoveryRequests.Request, recoveryRequest)
            || !ReferenceEquals(recoveryRequest.Actor, candidate.Occurrence.Target)
            || !ReferenceEquals(recoveryRequest.Relationship, outcome.Relationship)
            || !ReferenceEquals(recoveryRequest.Candidates.Single(), recoveryCandidate)
            || recoveryTask.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            || recoveryTask.Result is not null
            || recoveryState.Relationship is not null
            || recoveryState.LatestAppliedResult is not null
            || actionModel.CallCount != 1
            || influenceModel.CallCount != 1
            || recovery.TryCompose(eligibility.AuthorRequest, recoveryCandidate))
        {
            Fail("Main-scene exact eligible Influence did not compose one inert recovery request.");
            return;
        }

        GD.Print(
            "Main-scene sabotage outcome Influence recovery-request smoke passed: ordinary startup "
            + "stayed inert; one exact generic Influence selection retained its typed outcome and "
            + "current relationship authority; and its existing owner composed one exact recovery "
            + "request without initializing recovery authority or consuming another model.");
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
        influenceAuthor,
    ];

    private bool StateIsInitial() =>
        ReferenceEquals(influenceState.Relationship, request.Relationship)
        && influenceState.LatestAppliedResult is null
        && request.Relationship.Dimensions.Single().Value == 10
        && request.Relationship.Reasons.Count == 0;

    private CharacterRelationshipChange CreateRecoveryChange() =>
        new(
            candidate.RelationshipChange.CharacterId,
            candidate.RelationshipChange.RelatedCharacterId,
            -candidate.RelationshipChange.Delta,
            new CharacterRelationshipReason(
                new RelationshipReasonId("generic-influence-recovery"),
                candidate.RelationshipChange.Reason.Dimension,
                new CharacterTurnId("generic-influence-recovery"),
                "The target reconsidered the enemy agent's influence."));

    private static CharacterTargetSabotageInfluenceConsiderationRequest CreateRequest(
        out CharacterTargetSabotageInfluenceCandidate candidate)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        candidate = new CharacterTargetSabotageInfluenceCandidate(
            new CharacterTargetSabotageOccurrence(
                new CharacterTargetSabotageProposal(
                    actor.Identity.Id,
                    new CharacterTargetAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        actor.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Influence),
                target),
            new CharacterRelationshipChange(
                target.Identity.Id,
                actor.Identity.Id,
                1,
                new CharacterRelationshipReason(
                    new RelationshipReasonId("generic-influence-attempt"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("generic-influence-attempt"),
                    "The enemy agent attempted to gain influence.")));
        return new CharacterTargetSabotageInfluenceConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            [candidate]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingActionModel : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    value.Actor.Identity.Id,
                    CharacterSabotageActionKind.Influence));
        }
    }

    private sealed class RecordingInfluenceModel : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterTargetSabotageInfluenceDecision(
                    value.Actor.Identity.Id,
                    value.Candidates.Single()));
        }
    }
}
