using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact ordinary-turn sabotage authoring and generic consideration start.</summary>
public partial class CharacterSabotageOrdinaryTurnAuthoringMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_ORDINARY_TURN_AUTHORING_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int EarlyProcessPriority = -100;
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source = null!;
    private CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring = null!;
    private CharacterSabotageOrdinaryTurnConsiderationStartCoordinator start = null!;
    private CharacterSabotageOrdinaryTurnResultObservationCoordinator observation = null!;
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator resultBinding = null!;
    private CharacterSabotageOrdinaryTurnSpecificRequestCoordinator specificRequest = null!;
    private CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageOrdinaryTurnSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator delayRecovery = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator recoveryRequest = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator recoveryStart = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult = null!;
    private CharacterSabotageOutcomeDelayRecoveryCoordinator recovery = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController recoveryRequests = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController recoveryTask = null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource recoveryModelSource = null!;
    private CharacterSabotageTerminalRouteCoordinator terminalRoute = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResults = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcomes = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator delayResolution = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private TemporaryCharacterTargetSabotageDelayModelSource delayModelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageDelayTaskController delayTask = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterTurnActivity turn = null!;
    private RecordingModel model = null!;
    private RecordingDelayModel delayModel = null!;
    private RecordingDelayRecoveryModel recoveryModel = null!;
    private int processFrames;
    private bool recoveryStarted;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
            return;
        }

        ProcessPriority = EarlyProcessPriority;
        GetParent().SetProcess(false);
        source = GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
            "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        authoring = GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
            "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        start = GetNode<CharacterSabotageOrdinaryTurnConsiderationStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnConsiderationStartCoordinator");
        observation = GetNode<CharacterSabotageOrdinaryTurnResultObservationCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultObservationCoordinator");
        resultBinding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        specificRequest = GetNode<CharacterSabotageOrdinaryTurnSpecificRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificRequestCoordinator");
        specificStart = GetNode<CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator");
        specificResult = GetNode<CharacterSabotageOrdinaryTurnSpecificResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificResultCoordinator");
        specificOutcome = GetNode<CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator");
        delayRecovery = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator");
        recoveryRequest = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator");
        recoveryStart = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator");
        recoveryResult = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeDelayRecoveryCoordinator>(
            "../CharacterSabotageOutcomeDelayRecoveryCoordinator");
        recoveryRequests = GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        recoveryTask = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        recoveryModelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        terminalRoute = GetNode<CharacterSabotageTerminalRouteCoordinator>(
            "../CharacterSabotageTerminalRouteCoordinator");
        specificResults = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        specificOutcomes = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        specificRequests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        authors = GetAuthors();
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        delayModelSource = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        delayTask = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
        delayResolution = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        turn = CreateTurn(source.Actor);
        model = new RecordingModel();
        delayModel = new RecordingDelayModel();
        recoveryModel = new RecordingDelayRecoveryModel();

        RunAuthoringChecks();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        GetParent().SetProcess(false);
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Ordinary-turn sabotage consideration exceeded its bounded duration.");
            return;
        }

        if (recoveryStarted)
        {
            CompleteExactDelayRecovery();
            return;
        }

        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, composer.Request)
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Delay)
            || model.CallCount != 1
            || !ReferenceEquals(model.Request, composer.Request)
            || start.TryStart(turn, composer.Request)
            || !ReferenceEquals(turns.Activity, turn)
            || !ObserveExactResult(result)
            || !BindAndAcknowledgeExactResult(result)
            || !ComposeExactDelayRequest(result)
            || !StartExactDelayRequest(specificRequest.Request)
            || !ObserveAndAcknowledgeExactDelayResult(specificRequest.Request)
            || !BindExactDelayOutcome()
            || !PublishExactDelayRecoveryCandidate()
            || !ComposeExactDelayRecoveryRequest()
            || !StartExactDelayRecoveryRequest())
        {
            Fail("Ordinary-turn sabotage generic consideration did not remain exact and one-shot.");
            return;
        }

        recoveryStarted = true;
    }

    private void CompleteExactDelayRecovery()
    {
        if (recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running)
        {
            return;
        }

        if (!RetainExactCompletedDelayRecovery())
        {
            Fail("Ordinary-turn sabotage Delay recovery did not remain exact and one-shot.");
            return;
        }

        GD.Print(
            "Ordinary-turn sabotage authoring smoke passed: rejected turn and actor tokens stayed "
            + "inert; one exact current enemy turn invoked all seven authors in canonical order, "
            + "retained eligible Conceal, Delay, and Influence outputs while leaving four kinds "
            + "absent, composed once, and explicitly started one provider-neutral generic decision "
            + "before observing, binding, and acknowledging its exact selected Delay result once "
            + "and composing and explicitly starting one exact typed Delay request, then observing, "
            + "resolving, and acknowledging only its exact completed result once before binding its "
            + "exact applied outcome, publishing its exact recovery candidate, and composing its "
            + "exact recovery request before explicitly starting, completing, and acknowledging its "
            + "exact applied recovery once without terminal routing.");
        GetTree().Quit();
    }

    private void RunAuthoringChecks()
    {
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterTurnActivity rebuiltTurn = new(
            turn.Request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
        bool inertStartup = authoring.Turn is null
            && authoring.Actor is null
            && authoring.Request is null
            && start.Turn is null
            && start.Request is null
            && observation.ObservedResult is null
            && resultBinding.BoundResult is null
            && resultBinding.AcknowledgedResult is null
            && specificRequest.Request is null
            && specificStart.Request is null
            && specificResult.Result is null
            && specificResult.EventLog is null
            && specificResult.WorldTime is null
            && specificResult.AppliedResult is null
            && specificOutcome.AuthorRequest is null
            && specificOutcome.Result is null
            && specificOutcome.EventLog is null
            && specificOutcome.WorldTime is null
            && specificOutcome.Delay is null
            && delayRecovery.AuthorRequest is null
            && delayRecovery.Delay is null
            && delayRecovery.RecoveryEvent is null
            && delayRecovery.Candidate is null
            && recoveryRequest.AuthorRequest is null
            && recoveryRequest.Candidate is null
            && recoveryRequest.Request is null
            && recoveryStart.AuthorRequest is null
            && recoveryStart.Request is null
            && recoveryResult.AuthorRequest is null
            && recoveryResult.Request is null
            && recoveryResult.Result is null
            && recoveryResult.AppliedResult is null
            && recoveryResult.EventLog is null
            && recoveryResult.WorldTime is null
            && recovery.AuthorRequest is null
            && recovery.Request is null
            && recovery.StartedRequest is null
            && recoveryRequests.Request is null
            && recoveryState.EventLog is null
            && recoveryState.WorldTime is null
            && recoveryState.LatestAppliedResult is null
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && recoveryTask.Result is null
            && terminalRoute.TerminalOutputCount == 0
            && specificResults.ResultCount == 0
            && specificResults.AcknowledgedAuthorRequest is null
            && specificOutcomes.AuthorRequest is null
            && specificOutcomes.OutcomeCount == 0
            && selection.ObservedResult is null
            && specificRequests.RequestCount == 0
            && composer.Request is null
            && authors.All(author => author.Request is null)
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && delayTask.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && delayTask.Result is null
            && delayResolution.AcknowledgedResult is null
            && delayResolution.AcknowledgedAppliedResult is null
            && turns.Activity is null
            && model.CallCount == 0
            && delayModel.CallCount == 0
            && recoveryModel.CallCount == 0;
        bool initialized = delayState.TryInitialize(
                source.DelayRequest.EventLog,
                source.DelayRequest.WorldTime)
            && influenceState.TryInitialize(source.InfluenceRequest.Relationship)
            && turns.TryInitialize(turn);
        bool invalidRejected = !authoring.TryAuthor(null, source.Actor)
            && !authoring.TryAuthor(turn, null)
            && !authoring.TryAuthor(rebuiltTurn, source.Actor)
            && !authoring.TryAuthor(turn, replacementActor)
            && AuthorsAreEmpty();
        bool authored = authoring.TryAuthor(turn, source.Actor);
        CharacterSabotageActionConsiderationRequest? request = composer.Request;
        bool outputIsExact = authored
            && request is not null
            && ReferenceEquals(authoring.Turn, turn)
            && ReferenceEquals(authoring.Actor, source.Actor)
            && ReferenceEquals(authoring.Request, request)
            && request.ActionKinds.SequenceEqual(
                [
                    CharacterSabotageActionKind.Conceal,
                    CharacterSabotageActionKind.Delay,
                    CharacterSabotageActionKind.Influence,
                ],
                ReferenceEqualityComparer.Instance)
            && authoring.AuthorOutputs is { Count: 7 } outputs
            && outputs.Select((output, index) => ReferenceEquals(output, authors[index].Request))
                .All(value => value)
            && outputs[0] is not null
            && outputs.Skip(1).Take(4).All(output => output is null)
            && outputs[5] is not null
            && outputs[6] is not null
            && !authoring.TryAuthor(turn, source.Actor);
        bool invalidStartRejected = request is not null
            && modelSource.TrySupplyModel(model)
            && !start.TryStart(rebuiltTurn, request)
            && !start.TryStart(turn, new CharacterSabotageActionConsiderationRequest(
                source.Actor,
                request.ActionKinds))
            && model.CallCount == 0
            && task.Status == CharacterSabotageActionTaskStatus.Idle;
        bool started = request is not null
            && start.TryStart(turn, request)
            && ReferenceEquals(start.Turn, turn)
            && ReferenceEquals(start.Request, request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && !observation.TryObserveCompletedResult(null)
            && !start.TryStart(turn, request);

        if (!inertStartup
            || !initialized
            || !invalidRejected
            || !outputIsExact
            || !invalidStartRejected
            || !started)
        {
            Fail("Ordinary-turn sabotage authoring or consideration start violated its boundary.");
            return;
        }

        SetProcess(true);
    }

    private bool ObserveExactResult(CharacterSabotageActionConsiderationResult result)
    {
        CharacterSabotageActionConsiderationRequest request = composer.Request!;
        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            request,
            new CharacterSabotageActionDecision(
                result.Decision.ActorId,
                result.SelectedActionKind));
        CharacterSabotageActionConsiderationRequest foreignRequest = new(
            source.Actor,
            request.ActionKinds);
        CharacterSabotageActionConsiderationResult foreignResult = new(
            foreignRequest,
            new CharacterSabotageActionDecision(
                source.Actor.Identity.Id,
                CharacterSabotageActionKind.Delay));
        ICharacterSabotageActionRequestAuthor selectedAuthor = authors[5];
        CharacterSabotageActionConsiderationRequest? selectedRequest = selectedAuthor.Request;
        CharacterSabotageActionCandidateBinding? selectedBinding = selectedAuthor.CandidateBinding;

        return observation.ObservedResult is null
            && selection.ObservedResult is null
            && selectedRequest is not null
            && selectedBinding is not null
            && !observation.TryObserveCompletedResult(null)
            && !observation.TryObserveCompletedResult(rebuiltResult)
            && !observation.TryObserveCompletedResult(foreignResult)
            && observation.TryObserveCompletedResult(result)
            && ReferenceEquals(observation.ObservedResult, result)
            && ReferenceEquals(selection.ObservedResult, result)
            && ReferenceEquals(selectedAuthor.Request, selectedRequest)
            && ReferenceEquals(selectedAuthor.CandidateBinding, selectedBinding)
            && ReferenceEquals(selectedBinding.Request, selectedRequest)
            && ReferenceEquals(selectedBinding.ActionKind, result.SelectedActionKind)
            && !observation.TryObserveCompletedResult(result)
            && task.Status == CharacterSabotageActionTaskStatus.Completed
            && ReferenceEquals(task.Result, result);
    }

    private bool BindAndAcknowledgeExactResult(
        CharacterSabotageActionConsiderationResult result)
    {
        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterSabotageActionDecision(
                result.Decision.ActorId,
                result.SelectedActionKind));
        ICharacterSabotageActionRequestAuthor selectedAuthor = authors[5];
        CharacterSabotageActionConsiderationRequest selectedRequest = selectedAuthor.Request!;
        CharacterSabotageActionCandidateBinding selectedBinding = selectedAuthor.CandidateBinding!;

        return resultBinding.BoundResult is null
            && resultBinding.AcknowledgedResult is null
            && !resultBinding.TryBindObservedResult(null)
            && !resultBinding.TryBindObservedResult(rebuiltResult)
            && resultBinding.TryBindObservedResult(result)
            && ReferenceEquals(resultBinding.BoundResult, result)
            && ReferenceEquals(resultBinding.SelectedAuthorRequest, selectedRequest)
            && ReferenceEquals(resultBinding.SelectedCandidateBinding, selectedBinding)
            && !resultBinding.TryBindObservedResult(result)
            && !resultBinding.TryAcknowledgeBoundResult(null)
            && !resultBinding.TryAcknowledgeBoundResult(rebuiltResult)
            && resultBinding.TryAcknowledgeBoundResult(result)
            && ReferenceEquals(resultBinding.AcknowledgedResult, result)
            && ReferenceEquals(resultBinding.SelectedAuthorRequest, selectedRequest)
            && ReferenceEquals(resultBinding.SelectedCandidateBinding, selectedBinding)
            && !resultBinding.TryAcknowledgeBoundResult(result)
            && ReferenceEquals(observation.ObservedResult, result)
            && selection.ObservedResult is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null;
    }

    private bool ComposeExactDelayRequest(
        CharacterSabotageActionConsiderationResult result)
    {
        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterSabotageActionDecision(
                result.Decision.ActorId,
                result.SelectedActionKind));
        CharacterSabotageActionConsiderationRequest selectedRequest =
            resultBinding.SelectedAuthorRequest!;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
            selectedBinding = (CharacterSabotageActionCandidateBinding<
                CharacterTargetSabotageDelayCandidate>)resultBinding.SelectedCandidateBinding!;
        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            source.Actor,
            [CharacterSabotageActionKind.Delay]);
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
            rebuiltBinding = new(
                rebuiltRequest,
                CharacterSabotageActionKind.Delay,
                selectedBinding.Candidate);

        return specificRequest.Request is null
            && specificRequests.RequestCount == 0
            && !specificRequest.TryCompose(null, selectedRequest, selectedBinding)
            && !specificRequest.TryCompose(rebuiltResult, selectedRequest, selectedBinding)
            && !specificRequest.TryCompose(result, rebuiltRequest, rebuiltBinding)
            && specificRequest.TryCompose(result, selectedRequest, selectedBinding)
            && specificRequest.Request is { } request
            && ReferenceEquals(specificRequests.DelayRequest, request)
            && ReferenceEquals(request.Actor, source.DelayRequest.Actor)
            && ReferenceEquals(request.InteractionAccess, source.DelayRequest.InteractionAccess)
            && ReferenceEquals(request.WorldEvents, source.DelayRequest.WorldEvents)
            && ReferenceEquals(request.EventLog, source.DelayRequest.EventLog)
            && ReferenceEquals(request.WorldTime, source.DelayRequest.WorldTime)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates[0], selectedBinding.Candidate)
            && specificRequests.RequestCount == 1
            && !specificRequest.TryCompose(result, selectedRequest, selectedBinding)
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && delayTask.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && delayTask.Result is null;
    }

    private bool StartExactDelayRequest(
        CharacterTargetSabotageDelayConsiderationRequest? request)
    {
        if (request is null)
        {
            return false;
        }

        CharacterTargetSabotageDelayConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.InteractionAccess,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            request.Candidates);
        return delayModelSource.TrySupplyModel(delayModel)
            && !specificStart.TryStart(null)
            && !specificStart.TryStart(rebuiltRequest)
            && delayModel.CallCount == 0
            && delayTask.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && delayTask.Result is null
            && specificStart.TryStart(request)
            && ReferenceEquals(specificStart.Request, request)
            && delayModel.CallCount == 1
            && ReferenceEquals(delayModel.Request, request)
            && delayTask.Status == CharacterTargetSabotageDelayTaskStatus.Running
            && delayTask.Result is null
            && !specificStart.TryStart(request);
    }

    private bool ObserveAndAcknowledgeExactDelayResult(
        CharacterTargetSabotageDelayConsiderationRequest? request)
    {
        if (request is null)
        {
            return false;
        }

        CharacterTargetSabotageDelayConsiderationResult runningResult = new(
            request,
            new CharacterTargetSabotageDelayDecision(
                request.Actor.Identity.Id,
                request.Candidates.Single()));
        if (specificResult.TryObserveAndAcknowledge(runningResult)
            || delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Running
            || delayTask.Result is not null)
        {
            return false;
        }

        delayTask._Process(0.0);
        if (delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || delayTask.Result is not { } result)
        {
            return false;
        }

        CharacterTargetSabotageDelayConsiderationResult rebuiltResult = new(
            request,
            new CharacterTargetSabotageDelayDecision(
                result.Decision.ActorId,
                result.SelectedCandidate));
        CharacterTargetSabotageDelayConsiderationRequest foreignRequest = new(
            request.Actor,
            request.InteractionAccess,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            request.Candidates);
        CharacterTargetSabotageDelayConsiderationResult foreignResult = new(
            foreignRequest,
            new CharacterTargetSabotageDelayDecision(
                result.Decision.ActorId,
                result.SelectedCandidate));
        return specificResult.Result is null
            && specificResult.EventLog is null
            && specificResult.WorldTime is null
            && specificResult.AppliedResult is null
            && specificResults.ResultCount == 0
            && specificResults.AcknowledgedAuthorRequest is null
            && !specificResult.TryObserveAndAcknowledge(null)
            && !specificResult.TryObserveAndAcknowledge(rebuiltResult)
            && !specificResult.TryObserveAndAcknowledge(foreignResult)
            && ReferenceEquals(delayState.EventLog, request.EventLog)
            && ReferenceEquals(delayState.WorldTime, request.WorldTime)
            && delayState.LatestAppliedResult is null
            && specificResult.TryObserveAndAcknowledge(result)
            && ReferenceEquals(specificResult.Result, result)
            && specificResult.EventLog is { } eventLog
            && specificResult.WorldTime is { } worldTime
            && specificResult.AppliedResult is { } appliedResult
            && ReferenceEquals(delayState.EventLog, eventLog)
            && ReferenceEquals(delayState.WorldTime, worldTime)
            && ReferenceEquals(delayState.LatestAppliedResult, appliedResult)
            && ReferenceEquals(appliedResult.Candidate, result.SelectedCandidate)
            && ReferenceEquals(appliedResult.EventLog, eventLog)
            && ReferenceEquals(appliedResult.WorldTime, worldTime)
            && !ReferenceEquals(eventLog, request.EventLog)
            && !ReferenceEquals(worldTime, request.WorldTime)
            && ReferenceEquals(specificResults.DelayResult, result)
            && ReferenceEquals(
                specificResults.AcknowledgedAuthorRequest,
                resultBinding.SelectedAuthorRequest)
            && ReferenceEquals(delayResolution.AcknowledgedResult, result)
            && ReferenceEquals(delayResolution.AcknowledgedAppliedResult, appliedResult)
            && delayTask.Status == CharacterTargetSabotageDelayTaskStatus.Idle
            && delayTask.Result is null
            && delayModel.CallCount == 1
            && ReferenceEquals(turns.Activity, turn)
            && !specificResult.TryObserveAndAcknowledge(result);
    }

    private bool BindExactDelayOutcome()
    {
        if (specificResult.Result is not { } result
            || specificResult.AppliedResult is not { } appliedResult)
        {
            return false;
        }

        CharacterTargetSabotageDelayConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterTargetSabotageDelayDecision(
                result.Decision.ActorId,
                result.SelectedCandidate));
        CharacterTargetSabotageDelayResult rebuiltAppliedResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        bool initialState = specificOutcome.AuthorRequest is null
            && specificOutcome.Result is null
            && specificOutcome.EventLog is null
            && specificOutcome.WorldTime is null
            && specificOutcome.Delay is null
            && specificOutcomes.AuthorRequest is null
            && specificOutcomes.OutcomeCount == 0;
        bool invalidRejected = !specificOutcome.TryBind(null, appliedResult)
            && !specificOutcome.TryBind(result, null)
            && !specificOutcome.TryBind(rebuiltResult, appliedResult)
            && !specificOutcome.TryBind(result, rebuiltAppliedResult);
        bool bound = specificOutcome.TryBind(result, appliedResult);
        bool retained = ReferenceEquals(
                specificOutcome.AuthorRequest,
                resultBinding.SelectedAuthorRequest)
            && ReferenceEquals(
                specificOutcome.Result,
                result)
            && ReferenceEquals(specificOutcome.EventLog, appliedResult.EventLog)
            && ReferenceEquals(specificOutcome.WorldTime, appliedResult.WorldTime)
            && ReferenceEquals(specificOutcome.Delay, appliedResult)
            && ReferenceEquals(
                specificOutcomes.AuthorRequest,
                resultBinding.SelectedAuthorRequest)
            && ReferenceEquals(specificOutcomes.Delay, appliedResult)
            && specificOutcomes.OutcomeCount == 1
            && delayModel.CallCount == 1
            && ReferenceEquals(turns.Activity, turn)
            && !specificOutcome.TryBind(result, appliedResult);
        return initialState && invalidRejected && bound && retained;
    }

    private bool PublishExactDelayRecoveryCandidate()
    {
        if (specificOutcome.Delay is not { } appliedResult)
        {
            return false;
        }

        CharacterTargetSabotageDelayResult rebuiltAppliedResult = new(
            appliedResult.Candidate,
            appliedResult.EventLog,
            appliedResult.WorldTime);
        WorldEvent rebuiltRecoveryEvent = new(
            source.RecoveryEvent.EventId,
            source.RecoveryEvent.TimeEffect);
        bool initialState = delayRecovery.AuthorRequest is null
            && delayRecovery.Delay is null
            && delayRecovery.RecoveryEvent is null
            && delayRecovery.Candidate is null;
        bool invalidRejected = !delayRecovery.TryPublish(null, source.RecoveryEvent)
            && !delayRecovery.TryPublish(appliedResult, null)
            && !delayRecovery.TryPublish(rebuiltAppliedResult, source.RecoveryEvent)
            && !delayRecovery.TryPublish(appliedResult, rebuiltRecoveryEvent);
        bool published = delayRecovery.TryPublish(appliedResult, source.RecoveryEvent);
        bool retained = ReferenceEquals(
                delayRecovery.AuthorRequest,
                resultBinding.SelectedAuthorRequest)
            && ReferenceEquals(delayRecovery.Delay, appliedResult)
            && ReferenceEquals(delayRecovery.RecoveryEvent, source.RecoveryEvent)
            && delayRecovery.Candidate is { } candidate
            && ReferenceEquals(candidate.DelayResult, appliedResult)
            && ReferenceEquals(candidate.RecoveryEvent, source.RecoveryEvent)
            && delayModel.CallCount == 1
            && ReferenceEquals(turns.Activity, turn)
            && !delayRecovery.TryPublish(appliedResult, source.RecoveryEvent);
        return initialState && invalidRejected && published && retained;
    }

    private bool ComposeExactDelayRecoveryRequest()
    {
        if (delayRecovery.AuthorRequest is not { } authorRequest
            || delayRecovery.Candidate is not { } candidate)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterTargetSabotageDelayRecoveryCandidate rebuiltCandidate = new(
            candidate.DelayResult,
            candidate.RecoveryEvent);
        bool initialState = recoveryRequest.AuthorRequest is null
            && recoveryRequest.Candidate is null
            && recoveryRequest.Request is null
            && recovery.Request is null
            && recoveryRequests.Request is null;
        bool invalidRejected = !recoveryRequest.TryCompose(null, candidate)
            && !recoveryRequest.TryCompose(authorRequest, null)
            && !recoveryRequest.TryCompose(rebuiltAuthorRequest, candidate)
            && !recoveryRequest.TryCompose(authorRequest, rebuiltCandidate);
        bool composed = recoveryRequest.TryCompose(authorRequest, candidate);
        bool retained = ReferenceEquals(recoveryRequest.AuthorRequest, authorRequest)
            && ReferenceEquals(recoveryRequest.Candidate, candidate)
            && recoveryRequest.Request is { } request
            && ReferenceEquals(recovery.AuthorRequest, authorRequest)
            && ReferenceEquals(recovery.Request, request)
            && ReferenceEquals(recoveryRequests.Request, request)
            && ReferenceEquals(request.Actor, candidate.DelayResult.Candidate.Occurrence.Target)
            && ReferenceEquals(request.WorldEvents, source.DelayRequest.WorldEvents)
            && ReferenceEquals(request.EventLog, candidate.DelayResult.EventLog)
            && ReferenceEquals(request.WorldTime, candidate.DelayResult.WorldTime)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates[0], candidate)
            && recovery.StartedRequest is null
            && recoveryState.EventLog is null
            && recoveryState.WorldTime is null
            && recoveryState.LatestAppliedResult is null
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && recoveryTask.Result is null
            && delayModel.CallCount == 1
            && !recoveryRequest.TryCompose(authorRequest, candidate);
        return initialState && invalidRejected && composed && retained;
    }

    private bool StartExactDelayRecoveryRequest()
    {
        if (recoveryRequest.AuthorRequest is not { } authorRequest
            || recoveryRequest.Request is not { } request)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            request.Candidates);
        bool initialState = recoveryStart.AuthorRequest is null
            && recoveryStart.Request is null
            && recovery.StartedRequest is null
            && recoveryState.EventLog is null
            && recoveryState.WorldTime is null
            && recoveryState.LatestAppliedResult is null
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && recoveryTask.Result is null;
        bool invalidRejected = recoveryModelSource.TrySupplyModel(recoveryModel)
            && !recoveryStart.TryStart(null, request)
            && !recoveryStart.TryStart(authorRequest, null)
            && !recoveryStart.TryStart(rebuiltAuthorRequest, request)
            && !recoveryStart.TryStart(authorRequest, rebuiltRequest)
            && recoveryModel.CallCount == 0;
        bool started = recoveryStart.TryStart(authorRequest, request);
        bool retained = ReferenceEquals(recoveryStart.AuthorRequest, authorRequest)
            && ReferenceEquals(recoveryStart.Request, request)
            && ReferenceEquals(recovery.StartedRequest, request)
            && ReferenceEquals(recoveryState.EventLog, request.EventLog)
            && ReferenceEquals(recoveryState.WorldTime, request.WorldTime)
            && recoveryState.LatestAppliedResult is null
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            && recoveryTask.Result is null
            && recoveryModel.CallCount == 1
            && ReferenceEquals(recoveryModel.Request, request)
            && delayModel.CallCount == 1
            && !recoveryStart.TryStart(authorRequest, request)
            && recoveryModel.CallCount == 1;
        return initialState && invalidRejected && started && retained;
    }

    private bool RetainExactCompletedDelayRecovery()
    {
        if (recoveryStart.AuthorRequest is not { } authorRequest
            || recoveryStart.Request is not { } request
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || recoveryTask.Result is not { } completedResult)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            request.Candidates);
        bool initialState = recoveryResult.AuthorRequest is null
            && recoveryResult.Request is null
            && recoveryResult.Result is null
            && recoveryResult.AppliedResult is null
            && recoveryResult.EventLog is null
            && recoveryResult.WorldTime is null
            && recovery.CompletedResult is null
            && recovery.RecoveredResult is null;
        bool invalidRejected = !recoveryResult.TryRetain(null, request)
            && !recoveryResult.TryRetain(authorRequest, null)
            && !recoveryResult.TryRetain(rebuiltAuthorRequest, request)
            && !recoveryResult.TryRetain(authorRequest, rebuiltRequest);
        bool retained = recoveryResult.TryRetain(authorRequest, request);
        bool exact = ReferenceEquals(recoveryResult.AuthorRequest, authorRequest)
            && ReferenceEquals(recoveryResult.Request, request)
            && ReferenceEquals(recoveryResult.Result, completedResult)
            && ReferenceEquals(recovery.CompletedResult, completedResult)
            && recoveryResult.AppliedResult is { } appliedResult
            && ReferenceEquals(recovery.RecoveredResult, appliedResult)
            && ReferenceEquals(recoveryResult.EventLog, appliedResult.EventLog)
            && ReferenceEquals(recoveryResult.WorldTime, appliedResult.WorldTime)
            && ReferenceEquals(recovery.RecoveredEventLog, appliedResult.EventLog)
            && ReferenceEquals(recovery.RecoveredWorldTime, appliedResult.WorldTime)
            && ReferenceEquals(completedResult.Request, request)
            && ReferenceEquals(completedResult.SelectedCandidate, request.Candidates.Single())
            && ReferenceEquals(appliedResult.Candidate, completedResult.SelectedCandidate)
            && appliedResult.EventLog.Entries.Count == request.EventLog.Entries.Count + 1
            && ReferenceEquals(
                appliedResult.EventLog.Entries[^1],
                source.RecoveryEvent.EventId)
            && ReferenceEquals(appliedResult.WorldTime.CurrentEventId, source.RecoveryEvent.EventId)
            && recoveryTask.Status == CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            && recoveryTask.Result is null
            && recoveryModel.CallCount == 1
            && delayModel.CallCount == 1
            && terminalRoute.TerminalOutputCount == 0
            && !recoveryResult.TryRetain(authorRequest, request);
        return initialState && invalidRejected && retained && exact;
    }

    private bool AuthorsAreEmpty() =>
        authors.All(author => author.Request is null && author.CandidateBinding is null)
        && composer.Request is null;

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

    private static CharacterTurnActivity CreateTurn(Character actor)
    {
        CharacterId speakerId = new("ordinary-turn-sabotage-smoke-speaker");
        DialogueUtterance utterance = new(speakerId, "The road ahead remains uncertain.");
        CharacterTurnRequest request = new(
            actor.Identity.Id,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(speakerId, [actor.Identity.Id])),
            new DialogueAct(utterance, DialogueActType.ReportedClaim));
        return new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel : ICharacterSabotageActionDecisionModel
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
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    value.Actor.Identity.Id,
                    CharacterSabotageActionKind.Delay));
        }
    }

    private sealed class RecordingDelayModel : ICharacterTargetSabotageDelayDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(
                new CharacterTargetSabotageDelayDecision(
                    value.Actor.Identity.Id,
                    value.Candidates.Single()));
        }
    }

    private sealed class RecordingDelayRecoveryModel
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request
        {
            get;
            private set;
        }

        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(
                new CharacterTargetSabotageDelayRecoveryDecision(
                    value.Actor.Identity.Id,
                    value.Candidates.Single()));
        }
    }
}
