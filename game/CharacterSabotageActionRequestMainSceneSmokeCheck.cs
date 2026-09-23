using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact composed sabotage-action consideration through main-scene dependencies.</summary>
public partial class CharacterSabotageActionRequestMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_ACTION_REQUEST_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionConcealmentRequestController concealmentAuthor = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private CharacterSabotageActionRequestConsiderationFlowController flow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator actionSpecificRequests = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource concealmentModelSource = null!;
    private CharacterObjectSabotageConcealmentTaskController concealmentTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator actionSpecificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator actionSpecificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator actionSpecificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator outcomeEligibility = null!;
    private CharacterSabotageOutcomeRecoveryCoordinator outcomeRecovery = null!;
    private WorldObjectConcealmentSnapshot initialConcealment = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private RecordingModel model = null!;
    private RecordingConcealmentModel concealmentModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        if (!isEnabled)
        {
            SetProcess(false);
            return;
        }

        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        concealmentAuthor = GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController");
        authors = GetAuthors();
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        flow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        actionSpecificRequests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        concealmentModelSource = GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
            "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        concealmentTask = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        actionSpecificStart = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        actionSpecificResult = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        actionSpecificOutcome = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        outcomeEligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        outcomeRecovery = GetNode<CharacterSabotageOutcomeRecoveryCoordinator>(
            "../CharacterSabotageOutcomeRecoveryCoordinator");
        initialConcealment = concealmentState.Concealment!;
        initialEvidence = concealmentState.Evidence!;
        model = new(value => new(value.Actor.Identity.Id, CharacterSabotageActionKind.Conceal));
        concealmentModel = new(
            value => new(
                value.Character.Identity.Id,
                value.Candidates.Single().Evidence.EvidenceId));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene composed sabotage-action request exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartExactRequest();
            return;
        }

        if (stage == 1)
        {
            ObserveAndBindCompletedSelection();
            return;
        }

        if (stage == 2)
        {
            ObserveActionSpecificSelection();
            return;
        }

        Fail("Main-scene composed sabotage-action request entered an unknown stage.");
    }

    private void StartExactRequest()
    {
        bool inertStartup = authors.All(author => author.Request is null)
            && composer.Request is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && selection.ObservedResult is null
            && binding.SelectedAuthorRequest is null
            && binding.SelectedCandidateBinding is null
            && actionSpecificRequests.RequestCount == 0
            && concealmentTask.Status == CharacterObjectSabotageConcealmentTaskStatus.Idle
            && concealmentTask.Result is null
            && actionSpecificStart.StartedAuthorRequest is null
            && actionSpecificResult.ResultCount == 0
            && actionSpecificOutcome.OutcomeCount == 0
            && outcomeEligibility.EligibilityCount == 0
            && outcomeRecovery.RecoveredConcealment is null
            && !actionSpecificRequests.TryCompose(null, null)
            && !actionSpecificStart.TryStart()
            && !actionSpecificResult.TryObserveCompletedResult()
            && !actionSpecificResult.TryAcknowledgeCompletedResult()
            && !actionSpecificOutcome.TryBindAcknowledgedOutcome()
            && !outcomeEligibility.TryPublish(null, null)
            && !outcomeRecovery.TryRecoverConcealment(null, null, null)
            && StateIsUnchanged();
        bool authored = concealmentAuthor.TryCreate(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            initialConcealment,
            initialEvidence,
            source.Candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(source.Character, outputs);
        if (!composed || composer.Request is not { } request)
        {
            Fail("Main-scene exact author output did not compose one action request.");
            return;
        }

        bool started = modelSource.TrySupplyModel(model)
            && flow.TryStart(request)
            && task.Status == CharacterSabotageActionTaskStatus.Running;

        if (!inertStartup
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[0], concealmentAuthor.Request)
            || !started
            || !RequestIsExact(request)
            || !StateIsUnchanged())
        {
            Fail("Main-scene sabotage-action request failed inert exact composition or start.");
            return;
        }

        stage = 1;
    }

    private void ObserveAndBindCompletedSelection()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageActionConsiderationRequest request = composer.Request!;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Conceal)
            || model.CallCount != 1
            || !ReferenceEquals(model.Request, request)
            || selection.ObservedResult is not null
            || !RequestIsExact(request)
            || !StateIsUnchanged())
        {
            Fail("Main-scene composed sabotage-action selection changed authoritative state.");
            return;
        }

        if (!selection.TryObserveCompletedResult()
            || !ReferenceEquals(selection.ObservedResult, result)
            || !binding.TryBindObservedSelection()
            || !ReferenceEquals(binding.SelectedAuthorRequest, concealmentAuthor.Request)
            || !CandidateBindingIsExact()
            || binding.TryBindObservedSelection()
            || actionSpecificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || !binding.TryAcknowledgeBoundResult()
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || task.Result is not null
            || selection.ObservedResult is not null
            || !ReferenceEquals(binding.AcknowledgedResult, result)
            || !ReferenceEquals(binding.SelectedAuthorRequest, concealmentAuthor.Request)
            || !CandidateBindingIsExact()
            || binding.TryAcknowledgeBoundResult()
            || actionSpecificRequests.TryCompose(
                new CharacterSabotageActionConsiderationRequest(
                    source.Character,
                    [CharacterSabotageActionKind.Conceal]),
                binding.SelectedCandidateBinding)
            || !actionSpecificRequests.TryCompose(
                binding.SelectedAuthorRequest!,
                binding.SelectedCandidateBinding!)
            || actionSpecificRequests.ConcealmentRequest is not { } concealmentRequest
            || actionSpecificRequests.RequestCount != 1
            || !ReferenceEquals(concealmentRequest.Character, source.Character)
            || !ReferenceEquals(concealmentRequest.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(concealmentRequest.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(concealmentRequest.Concealment, initialConcealment)
            || !ReferenceEquals(concealmentRequest.Evidence, initialEvidence)
            || concealmentRequest.Candidates.Count != 1
            || !ReferenceEquals(concealmentRequest.Candidates.Single(), source.Candidate)
            || actionSpecificRequests.TryCompose(
                binding.SelectedAuthorRequest!,
                binding.SelectedCandidateBinding!)
            || !concealmentModelSource.TrySupplyModel(concealmentModel)
            || !actionSpecificStart.TryStart()
            || !ReferenceEquals(
                actionSpecificStart.StartedAuthorRequest,
                binding.SelectedAuthorRequest)
            || actionSpecificResult.TryObserveCompletedResult()
            || concealmentTask.Status != CharacterObjectSabotageConcealmentTaskStatus.Running
            || actionSpecificStart.TryStart()
            || !RequestIsExact(request)
            || !StateIsUnchanged())
        {
            Fail("Main-scene exact selection did not compose and start once without mutation.");
            return;
        }

        stage = 2;
    }

    private void ObserveActionSpecificSelection()
    {
        if (concealmentTask.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        CharacterObjectSabotageConcealmentConsiderationRequest request =
            actionSpecificRequests.ConcealmentRequest!;
        CharacterEvidenceViewSnapshot eligibleEvidenceView = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.EvidenceId]);
        CharacterEvidenceViewSnapshot unseenEvidenceView = new(
            source.Character.Identity.Id,
            []);
        CharacterEvidenceViewSnapshot rebuiltEvidenceView = new(
            source.Character.Identity.Id,
            [source.Candidate.Evidence.EvidenceId]);
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            source.Character,
            [CharacterSabotageActionKind.Conceal]);
        if (concealmentTask.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || concealmentTask.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, source.Candidate)
            || concealmentModel.CallCount != 1
            || !ReferenceEquals(concealmentModel.Request, request)
            || !ReferenceEquals(
                actionSpecificStart.StartedAuthorRequest,
                binding.SelectedAuthorRequest)
            || actionSpecificStart.TryStart()
            || !actionSpecificResult.TryObserveCompletedResult()
            || !ReferenceEquals(actionSpecificResult.ConcealmentResult, result)
            || actionSpecificResult.ResultCount != 1
            || actionSpecificResult.TryObserveCompletedResult()
            || actionSpecificOutcome.TryBindAcknowledgedOutcome()
            || outcomeEligibility.TryPublish(
                binding.SelectedAuthorRequest,
                eligibleEvidenceView)
            || !actionSpecificResult.TryAcknowledgeCompletedResult()
            || concealmentTask.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || concealmentTask.Result is not null
            || !ReferenceEquals(
                actionSpecificResult.AcknowledgedAuthorRequest,
                binding.SelectedAuthorRequest)
            || actionSpecificResult.TryAcknowledgeCompletedResult()
            || !actionSpecificOutcome.TryBindAcknowledgedOutcome()
            || !ReferenceEquals(
                actionSpecificOutcome.AuthorRequest,
                binding.SelectedAuthorRequest)
            || !ReferenceEquals(actionSpecificOutcome.Concealment, concealmentState.Concealment)
            || !ReferenceEquals(
                actionSpecificOutcome.ConcealmentEvidence,
                concealmentState.Evidence)
            || actionSpecificOutcome.OutcomeCount != 1
            || actionSpecificOutcome.Placements is not null
            || actionSpecificOutcome.MoveEvidence is not null
            || actionSpecificOutcome.Substitutions is not null
            || actionSpecificOutcome.SubstitutionEvidence is not null
            || actionSpecificOutcome.Contamination is not null
            || actionSpecificOutcome.ContaminationEvidence is not null
            || actionSpecificOutcome.MisquotationExposure is not null
            || actionSpecificOutcome.Delay is not null
            || actionSpecificOutcome.Influence is not null
            || actionSpecificOutcome.TryBindAcknowledgedOutcome()
            || outcomeEligibility.TryPublish(rebuiltAuthorRequest, eligibleEvidenceView)
            || outcomeEligibility.TryPublish(binding.SelectedAuthorRequest, unseenEvidenceView)
            || !outcomeEligibility.TryPublish(
                binding.SelectedAuthorRequest,
                eligibleEvidenceView)
            || !ReferenceEquals(
                outcomeEligibility.AuthorRequest,
                binding.SelectedAuthorRequest)
            || !ReferenceEquals(
                outcomeEligibility.ConcealmentCandidate,
                source.Candidate)
            || outcomeEligibility.EligibilityCount != 1
            || outcomeRecovery.TryRecoverConcealment(
                rebuiltAuthorRequest,
                outcomeEligibility.ConcealmentCandidate,
                outcomeEligibility.EvidenceView)
            || outcomeRecovery.TryRecoverConcealment(
                outcomeEligibility.AuthorRequest,
                outcomeEligibility.ConcealmentCandidate,
                rebuiltEvidenceView)
            || !outcomeRecovery.TryRecoverConcealment(
                outcomeEligibility.AuthorRequest,
                outcomeEligibility.ConcealmentCandidate,
                outcomeEligibility.EvidenceView)
            || !ReferenceEquals(
                outcomeRecovery.AuthorRequest,
                outcomeEligibility.AuthorRequest)
            || !ReferenceEquals(outcomeRecovery.EvidenceView, eligibleEvidenceView)
            || outcomeRecovery.RecoveredConcealment is not { } recoveredConcealment
            || !ReferenceEquals(concealmentState.Concealment, recoveredConcealment)
            || ReferenceEquals(recoveredConcealment, actionSpecificOutcome.Concealment)
            || recoveredConcealment.ConcealedObjectIds.Count != 0
            || !ReferenceEquals(
                concealmentState.Evidence,
                actionSpecificOutcome.ConcealmentEvidence)
            || model.CallCount != 1
            || concealmentModel.CallCount != 1
            || outcomeRecovery.TryRecoverConcealment(
                outcomeEligibility.AuthorRequest,
                outcomeEligibility.ConcealmentCandidate,
                outcomeEligibility.EvidenceView)
            || outcomeEligibility.TryPublish(
                binding.SelectedAuthorRequest,
                eligibleEvidenceView)
            || StateIsUnchanged())
        {
            Fail("Main-scene exact selected action did not retain its current typed outcome.");
            return;
        }

        GD.Print(
            "Main-scene composed sabotage-action request smoke passed: ordinary startup stayed "
            + "inert; one exact live conceal request completed, was observed, bound to its exact "
            + "author output, acknowledged, composed into one exact concealment request, and "
            + "started once through an explicit selecting model, applied, presented, observed, "
            + "acknowledged, bound to its exact current typed authoritative outcome, and passed "
            + "its existing evidence gate into one-shot eligibility before the exact retained "
            + "concealment branch alone routed through its existing recovery transition.");
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

    private bool RequestIsExact(CharacterSabotageActionConsiderationRequest request) =>
        ReferenceEquals(request.Actor, source.Character)
        && request.ActionKinds.Count == 1
        && ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(authors[0].Request, concealmentAuthor.Request)
        && authors.Skip(1).All(author => author.Request is null);

    private bool CandidateBindingIsExact() =>
        binding.SelectedCandidateBinding
            is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageEvidence>
        {
            Candidate: { } candidate,
            Request: { } request,
            ActionKind: { } actionKind,
        }
        && ReferenceEquals(candidate, source.Candidate)
        && ReferenceEquals(request, concealmentAuthor.Request)
        && ReferenceEquals(actionKind, CharacterSabotageActionKind.Conceal);

    private bool StateIsUnchanged() =>
        ReferenceEquals(concealmentState.Concealment, initialConcealment)
        && ReferenceEquals(concealmentState.Evidence, initialEvidence)
        && initialConcealment.ConcealedObjectIds.Count == 0
        && initialEvidence.Entries.Count == 0;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> decide)
        : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }

    private sealed class RecordingConcealmentModel(
        Func<CharacterObjectSabotageConcealmentConsiderationRequest,
            CharacterObjectSabotageConcealmentDecision> decide)
        : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageConcealmentConsiderationRequest? Request
        {
            get;
            private set;
        }

        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}
