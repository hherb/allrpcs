using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact main-scene sabotage-opportunity ownership without starting a turn.</summary>
public partial class CharacterSabotageTurnOpportunityControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_TURN_OPPORTUNITY_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int EarlyProcessPriority = -100;
    private const int MaximumProcessFrames = 30;

    private int processFrames;
    private bool decisionStarted;
    private bool typedDecisionStarted;
    private bool recoveryStarted;
    private CharacterSabotageTurnOpportunityRecordingModel? recordingModel;
    private CharacterSabotageTurnOpportunityRecordingDelayModel? delayModel;
    private CharacterSabotageTurnOpportunityRecordingDelayRecoveryModel? recoveryModel;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
            return;
        }

        ProcessPriority = EarlyProcessPriority;
        GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator").SetProcess(false);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;

        if (recoveryStarted)
        {
            VerifyCompletedRecovery();
            return;
        }

        if (typedDecisionStarted)
        {
            VerifyCompletedTypedDecision();
            return;
        }

        if (decisionStarted)
        {
            VerifyCompletedDecisionObservation();
            return;
        }

        CharacterSabotageTurnOpportunityController controller =
            GetNode<CharacterSabotageTurnOpportunityController>(
                "../CharacterSabotageTurnOpportunityController");
        if (controller.Actor is null || controller.Cadence is null)
        {
            if (processFrames >= MaximumProcessFrames)
            {
                Fail("The sabotage-opportunity controller did not initialize.");
            }

            return;
        }

        TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source =
            GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
                "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        TemporaryWorldEventSource eventSource =
            GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        WorldEventController world = GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnActivityController turns =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring =
            GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
                "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        CharacterSabotageTurnOpportunitySchedulingCoordinator scheduling =
            GetNode<CharacterSabotageTurnOpportunitySchedulingCoordinator>(
                "../CharacterSabotageTurnOpportunitySchedulingCoordinator");
        CharacterSabotageTurnReservationCoordinator reservation =
            GetNode<CharacterSabotageTurnReservationCoordinator>(
                "../CharacterSabotageTurnReservationCoordinator");
        Character actor = source.Actor;
        CharacterSabotageTurnOpportunityCadence cadence = controller.Cadence;
        CharacterSabotageActionTaskController task =
            GetNode<CharacterSabotageActionTaskController>(
                "../CharacterSabotageActionTaskController");
        CharacterSabotageOrdinaryTurnConsiderationStartCoordinator start =
            GetNode<CharacterSabotageOrdinaryTurnConsiderationStartCoordinator>(
                "../CharacterSabotageOrdinaryTurnConsiderationStartCoordinator");
        TemporaryCharacterSabotageActionModelSource modelSource =
            GetNode<TemporaryCharacterSabotageActionModelSource>(
                "../TemporaryCharacterSabotageActionModelSource");

        if (!ReferenceEquals(controller.Actor, actor)
            || controller.Opportunity is not null
            || reservation.Reservation is not null
            || turns.Activity is not null
            || authoring.Request is not null
            || controller.TryCreate(actor, cadence, null)
            || controller.TryCreate(
                RepresentativeEnemyAgentCharacterSource.Create(),
                cadence,
                eventSource.HerbPatchIdentifiedWorldEvent)
            || controller.TryCreate(
                actor,
                new CharacterSabotageTurnOpportunityCadence(cadence.MeaningfulEventInterval),
                eventSource.HerbPatchIdentifiedWorldEvent))
        {
            Fail("Ordinary startup or rejected opportunity input changed scene state.");
            return;
        }

        if (!world.TryOccur(eventSource.HerbPatchIdentifiedWorldEvent.EventId))
        {
            Fail("The first meaningful event could not occur for the opportunity check.");
            return;
        }

        if (scheduling.TryObserveCurrentEvent()
            || !ReferenceEquals(scheduling.ObservedWorldEvent, eventSource.HerbPatchIdentifiedWorldEvent)
            || scheduling.TryObserveCurrentEvent())
        {
            Fail("A pre-cadence meaningful event produced a sabotage opportunity.");
            return;
        }

        if (!world.TryOccur(eventSource.HerbPatchExaminedWorldEvent.EventId))
        {
            Fail("The due meaningful event could not occur for the opportunity check.");
            return;
        }

        if (!scheduling.TryObserveCurrentEvent()
            || controller.Opportunity is not { } opportunity
            || !ReferenceEquals(opportunity.Actor, actor)
            || !ReferenceEquals(
                opportunity.Signal.WorldEvent,
                eventSource.HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(opportunity.Signal.Cadence, cadence)
            || opportunity.Signal.OccurredMeaningfulEventCount != 2
            || scheduling.TryObserveCurrentEvent()
            || controller.TryCreate(
                actor,
                cadence,
                eventSource.HerbPatchExaminedWorldEvent)
            || reservation.TryReserve(new CharacterSabotageTurnOpportunity(actor, opportunity.Signal))
            || !reservation.TryReserve(opportunity)
            || reservation.Reservation is not { } retainedReservation
            || !ReferenceEquals(retainedReservation.Opportunity, opportunity)
            || reservation.TryReserve(opportunity)
            || turns.TryStart(CreateDialogueTurnRequest())
            || turns.Activity is not null
            || !authoring.TryAuthor(retainedReservation)
            || !ReferenceEquals(authoring.Reservation, retainedReservation)
            || authoring.Turn is not null
            || authoring.Request is not { } request
            || !ReferenceEquals(request.Actor, actor)
            || request.ActionKinds.Count != 3
            || authoring.AuthorOutputs is not { Count: 7 } outputs
            || outputs.Count(output => output is not null) != 3
            || authoring.TryAuthor(retainedReservation))
        {
            Fail("The due exact signal did not produce only one inert opportunity.");
            return;
        }

        recordingModel = new CharacterSabotageTurnOpportunityRecordingModel();
        CharacterSabotageTurnReservation rebuiltReservation = new(opportunity);
        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            actor,
            request.ActionKinds);
        if (!modelSource.TrySupplyModel(recordingModel)
            || start.TryStart(rebuiltReservation, request)
            || start.TryStart(retainedReservation, rebuiltRequest)
            || recordingModel.CallCount != 0
            || !start.TryStart(retainedReservation, request)
            || !ReferenceEquals(start.Reservation, retainedReservation)
            || !ReferenceEquals(start.Request, request)
            || task.Status != CharacterSabotageActionTaskStatus.Running
            || task.Result is not null
            || recordingModel.CallCount != 1
            || start.TryStart(retainedReservation, request))
        {
            Fail("The exact reserved request did not start one generic decision once.");
            return;
        }

        decisionStarted = true;
    }

    private void VerifyCompletedDecisionObservation()
    {
        CharacterSabotageActionTaskController task =
            GetNode<CharacterSabotageActionTaskController>(
                "../CharacterSabotageActionTaskController");
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageOrdinaryTurnResultObservationCoordinator observation =
            GetNode<CharacterSabotageOrdinaryTurnResultObservationCoordinator>(
                "../CharacterSabotageOrdinaryTurnResultObservationCoordinator");
        CharacterSabotageOrdinaryTurnResultBindingCoordinator binding =
            GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
                "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        CharacterSabotageOrdinaryTurnSpecificRequestCoordinator specificRequest =
            GetNode<CharacterSabotageOrdinaryTurnSpecificRequestCoordinator>(
                "../CharacterSabotageOrdinaryTurnSpecificRequestCoordinator");
        CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator specificStart =
            GetNode<CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator>(
                "../CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator");
        TemporaryCharacterTargetSabotageDelayModelSource delayModelSource =
            GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
                "../TemporaryCharacterTargetSabotageDelayModelSource");
        CharacterTargetSabotageDelayTaskController delayTask =
            GetNode<CharacterTargetSabotageDelayTaskController>(
                "../CharacterTargetSabotageDelayTaskController");
        CharacterTurnActivityController turns =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterSabotageTurnReservation? reservation = turns.SabotageReservation;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || reservation is null
            || recordingModel?.CallCount != 1
            || !observation.TryObserveCompletedResult(result)
            || !ReferenceEquals(observation.ObservedResult, result)
            || observation.TryObserveCompletedResult(result)
            || binding.TryBindObservedResult(null, result)
            || binding.TryBindObservedResult(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                result)
            || binding.TryBindObservedResult(
                reservation,
                new CharacterSabotageActionConsiderationResult(
                    result.Request,
                    new CharacterSabotageActionDecision(
                        result.Decision.ActorId,
                        result.SelectedActionKind)))
            || !binding.TryBindObservedResult(reservation, result)
            || !ReferenceEquals(binding.Reservation, reservation)
            || !ReferenceEquals(binding.BoundResult, result)
            || binding.SelectedAuthorRequest is null
            || binding.SelectedCandidateBinding is null
            || binding.TryBindObservedResult(reservation, result)
            || binding.TryAcknowledgeBoundResult(null, result)
            || binding.TryAcknowledgeBoundResult(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                result)
            || !binding.TryAcknowledgeBoundResult(reservation, result)
            || !ReferenceEquals(binding.AcknowledgedResult, result)
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || task.Result is not null
            || binding.TryAcknowledgeBoundResult(reservation, result)
            || recordingModel.CallCount != 1
            || binding.SelectedAuthorRequest is not { } selectedRequest
            || binding.SelectedCandidateBinding is not
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
                    selectedBinding
            || specificRequest.TryCompose(
                null,
                result,
                selectedRequest,
                selectedBinding)
            || specificRequest.TryCompose(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                result,
                selectedRequest,
                selectedBinding)
            || !specificRequest.TryCompose(
                reservation,
                result,
                selectedRequest,
                selectedBinding)
            || !ReferenceEquals(specificRequest.Reservation, reservation)
            || specificRequest.Request is not { } typedRequest
            || specificRequest.TryCompose(
                reservation,
                result,
                selectedRequest,
                selectedBinding)
            || !delayModelSource.TrySupplyModel(
                delayModel = new CharacterSabotageTurnOpportunityRecordingDelayModel())
            || specificStart.TryStart(
                reservation,
                new CharacterTargetSabotageDelayConsiderationRequest(
                    typedRequest.Actor,
                    typedRequest.InteractionAccess,
                    typedRequest.WorldEvents,
                    typedRequest.EventLog,
                    typedRequest.WorldTime,
                    typedRequest.Candidates))
            || !specificStart.TryStart(reservation, typedRequest)
            || !ReferenceEquals(specificStart.Reservation, reservation)
            || !ReferenceEquals(specificStart.Request, typedRequest)
            || delayTask.Status != CharacterTargetSabotageDelayTaskStatus.Running
            || delayTask.Result is not null
            || specificStart.TryStart(reservation, typedRequest)
            || recordingModel.CallCount != 1)
        {
            Fail("The reserved generic decision did not start one exact typed Delay request.");
            return;
        }

        typedDecisionStarted = true;
    }

    private void VerifyCompletedTypedDecision()
    {
        CharacterTurnActivityController turns =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterSabotageTurnReservation? reservation = turns.SabotageReservation;
        CharacterTargetSabotageDelayTaskController task =
            GetNode<CharacterTargetSabotageDelayTaskController>(
                "../CharacterTargetSabotageDelayTaskController");
        if (task.Status == CharacterTargetSabotageDelayTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageOrdinaryTurnSpecificResultCoordinator resultCoordinator =
            GetNode<CharacterSabotageOrdinaryTurnSpecificResultCoordinator>(
                "../CharacterSabotageOrdinaryTurnSpecificResultCoordinator");
        CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator outcomeCoordinator =
            GetNode<CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator>(
                "../CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator");
        CharacterSabotageOrdinaryTurnResultBindingCoordinator binding =
            GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
                "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator recoveryCandidate =
            GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator>(
                "../CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator");
        CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator recoveryRequest =
            GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator>(
                "../CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator");
        CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator recoveryStart =
            GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator>(
                "../CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator");
        TemporaryCharacterTargetSabotageDelayRecoveryModelSource recoveryModelSource =
            GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
                "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        CharacterTargetSabotageDelayRecoveryTaskController recoveryTask =
            GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
                "../CharacterTargetSabotageDelayRecoveryTaskController");
        TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source =
            GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
                "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        recoveryModel = new CharacterSabotageTurnOpportunityRecordingDelayRecoveryModel();
        if (reservation is null
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || task.Result is not { } result
            || delayModel?.CallCount != 1
            || !ReferenceEquals(delayModel.Request, result.Request))
        {
            Fail("The exact reserved typed Delay decision did not complete once.");
            return;
        }

        CharacterTargetSabotageDelayConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterTargetSabotageDelayDecision(
                result.Decision.ActorId,
                result.SelectedCandidate));
        bool nullRejected = !resultCoordinator.TryObserveAndAcknowledge(null, result);
        bool rebuiltReservationRejected = !resultCoordinator.TryObserveAndAcknowledge(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                result);
        bool rebuiltResultRejected =
            !resultCoordinator.TryObserveAndAcknowledge(reservation, rebuiltResult);
        bool acknowledged = resultCoordinator.TryObserveAndAcknowledge(reservation, result);
        bool repeatedRejected = !resultCoordinator.TryObserveAndAcknowledge(reservation, result);
        if (!nullRejected
            || !rebuiltReservationRejected
            || !rebuiltResultRejected
            || !acknowledged
            || !ReferenceEquals(resultCoordinator.Reservation, reservation)
            || !ReferenceEquals(resultCoordinator.Result, result)
            || resultCoordinator.EventLog is null
            || resultCoordinator.WorldTime is null
            || resultCoordinator.AppliedResult is not { } appliedResult
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || task.Result is not null
            || !repeatedRejected
            || recordingModel?.CallCount != 1
            || delayModel.CallCount != 1
            || outcomeCoordinator.TryBind(null, result, appliedResult)
            || outcomeCoordinator.TryBind(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                result,
                appliedResult)
            || !outcomeCoordinator.TryBind(reservation, result, appliedResult)
            || !ReferenceEquals(outcomeCoordinator.Reservation, reservation)
            || !ReferenceEquals(outcomeCoordinator.Result, result)
            || !ReferenceEquals(outcomeCoordinator.Delay, appliedResult)
            || outcomeCoordinator.TryBind(reservation, result, appliedResult)
            || recoveryCandidate.TryPublish(null, appliedResult, source.RecoveryEvent)
            || recoveryCandidate.TryPublish(reservation, null, source.RecoveryEvent)
            || !recoveryCandidate.TryPublish(
                reservation,
                appliedResult,
                source.RecoveryEvent)
            || !ReferenceEquals(recoveryCandidate.Reservation, reservation)
            || !ReferenceEquals(recoveryCandidate.Delay, appliedResult)
            || !ReferenceEquals(recoveryCandidate.RecoveryEvent, source.RecoveryEvent)
            || recoveryCandidate.Candidate is null
            || recoveryCandidate.TryPublish(
                reservation,
                appliedResult,
                source.RecoveryEvent)
            || binding.SelectedAuthorRequest is not { } authorRequest
            || recoveryCandidate.Candidate is not { } candidate
            || recoveryRequest.TryCompose(null, authorRequest, candidate)
            || recoveryRequest.TryCompose(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                authorRequest,
                candidate)
            || !recoveryRequest.TryCompose(reservation, authorRequest, candidate)
            || !ReferenceEquals(recoveryRequest.Reservation, reservation)
            || !ReferenceEquals(recoveryRequest.AuthorRequest, authorRequest)
            || !ReferenceEquals(recoveryRequest.Candidate, candidate)
            || recoveryRequest.Request is not { } composedRecoveryRequest
            || recoveryRequest.TryCompose(reservation, authorRequest, candidate)
            || !recoveryModelSource.TrySupplyModel(recoveryModel)
            || recoveryStart.TryStart(null, authorRequest, composedRecoveryRequest)
            || recoveryStart.TryStart(
                new CharacterSabotageTurnReservation(reservation.Opportunity),
                authorRequest,
                composedRecoveryRequest)
            || !recoveryStart.TryStart(
                reservation,
                authorRequest,
                composedRecoveryRequest)
            || !ReferenceEquals(recoveryStart.Reservation, reservation)
            || !ReferenceEquals(recoveryStart.AuthorRequest, authorRequest)
            || !ReferenceEquals(recoveryStart.Request, composedRecoveryRequest)
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Running
            || recoveryTask.Result is not null
            || recoveryModel.CallCount != 1
            || !ReferenceEquals(recoveryModel.Request, composedRecoveryRequest)
            || recoveryStart.TryStart(
                reservation,
                authorRequest,
                composedRecoveryRequest))
        {
            Fail(
                "The reserved Delay recovery request did not start exactly once. "
                + $"null={nullRejected}; rebuiltReservation={rebuiltReservationRejected}; "
                + $"rebuiltResult={rebuiltResultRejected}; acknowledged={acknowledged}; "
                + $"repeated={repeatedRejected}; status={task.Status}.");
            return;
        }

        recoveryStarted = true;
    }

    private static void Fail(string message)
    {
        GD.PushError(message);
        ((SceneTree)Engine.GetMainLoop()).Quit(1);
    }

    private static CharacterTurnRequest CreateDialogueTurnRequest()
    {
        CharacterId speakerId = new("sabotage-opportunity-smoke-speaker");
        CharacterId listenerId = new("sabotage-opportunity-smoke-listener");
        DialogueUtterance utterance = new(speakerId, "The road remains open.");
        return new CharacterTurnRequest(
            listenerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(speakerId, [listenerId])),
            new DialogueAct(utterance, DialogueActType.ReportedClaim));
    }

}
