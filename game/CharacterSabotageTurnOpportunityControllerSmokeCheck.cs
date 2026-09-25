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
    private const int MaximumProcessFrames = 30;

    private int processFrames;
    private bool decisionStarted;
    private RecordingModel? recordingModel;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;

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

        recordingModel = new RecordingModel();
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
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || recordingModel?.CallCount != 1
            || !observation.TryObserveCompletedResult(result)
            || !ReferenceEquals(observation.ObservedResult, result)
            || observation.TryObserveCompletedResult(result)
            || binding.BoundResult is not null
            || binding.AcknowledgedResult is not null)
        {
            Fail("The reserved generic decision was not observed exactly once without binding.");
            return;
        }

        GD.Print(
            "Sabotage-turn opportunity controller smoke passed: ordinary startup stayed inert; "
            + "one exact due authoritative meaningful-event signal produced one opportunity "
            + "and reserved the shared turn slot before invoking all exact current authors once "
            + "and starting and observing only their exact completed generic decision once without "
            + "binding or acknowledging it.");
        GetTree().Quit();
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

    private sealed class RecordingModel : ICharacterSabotageActionDecisionModel
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
                    CharacterSabotageActionKind.Delay));
        }
    }
}
