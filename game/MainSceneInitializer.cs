using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Initializes the Main scene's authored state once, in a fixed order, failing fast when any
/// owner rejects it. Main calls the two halves around inserting its examination controller.
/// </summary>
internal static class MainSceneInitializer
{
    private const int SabotageTurnMeaningfulEventInterval = 2;
    private static readonly TimeSpan IndependentTaskStepInterval = TimeSpan.FromSeconds(3);

    /// <summary>Initializes model, dialogue, transcript, first-patient and world-event state.</summary>
    public static void InitializeBeforeExamination(Node main)
    {
        CharacterModelStreamController modelStreamController =
            main.GetNode<CharacterModelStreamController>("CharacterModelStreamController");
        if (!modelStreamController.TryInitialize(new CharacterModelStreamSnapshot([])))
        {
            throw new InvalidOperationException(
                "The character-model stream controller could not initialize empty.");
        }

        if (!main.GetNode<CharacterModelGenerationFlowController>(
                "CharacterModelGenerationFlowController").TryInitialize(
                modelStreamController,
                main.GetNode<TemporaryCharacterModelGenerationStatusLabel>(
                    "DialogueOverlay/Panel/ModelStatus"),
                main.GetNode<TemporaryCharacterModelStreamTextLabel>(
                    "DialogueOverlay/Panel/ModelStreamText")))
        {
            throw new InvalidOperationException(
                "The character-model generation flow could not initialize.");
        }

        TemporarySeekerCharacterSource seeker =
            main.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource");
        if (!main.GetNode<DialogueInputController>("DialogueInputController").TryInitialize(
                seeker.Character.Identity.Id))
        {
            throw new InvalidOperationException(
                "The dialogue-input controller could not initialize the scene Seeker.");
        }

        if (!main.GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController")
                .TryInitialize(new CharacterTurnTranscriptSnapshot([])))
        {
            throw new InvalidOperationException(
                "The character-turn transcript controller could not initialize empty.");
        }

        TemporaryFirstPatientTreatmentContextSource firstPatient =
            main.GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "TemporaryFirstPatientTreatmentContextSource");
        if (!main.GetNode<FirstPatientStateController>("FirstPatientStateController").TryInitialize(
                firstPatient.Context.Scenario.DiseaseTruth,
                new PreparedIngredientBatchSnapshot([]),
                new IngredientExperimentJournalSnapshot([]),
                new ExperimentJournalSnapshot([])))
        {
            throw new InvalidOperationException(
                "The first-patient controller could not initialize the authored scene state.");
        }

        if (!main.GetNode<FirstPatientPreparationController>("FirstPatientPreparationController")
                .TryInitialize(
                    firstPatient.Context.Scenario.Remedy.Recipe,
                    firstPatient.Context.Scenario.Ingredients))
        {
            throw new InvalidOperationException(
                "The first-patient preparation controller could not initialize scene ingredients.");
        }

        if (!main.GetNode<WorldEventController>("WorldEventController").TryInitialize(
                main.GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource").WorldEvents,
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)))
        {
            throw new InvalidOperationException(
                "The world-event controller could not initialize the scene state.");
        }
    }

    /// <summary>
    /// Occurs the initial world event, then initializes sabotage cadence, world objects, the
    /// healer's independent task and the Seeker's initial perception.
    /// </summary>
    public static void InitializeAfterExamination(Node main, Healer healer)
    {
        WorldEventController worldEvents = main.GetNode<WorldEventController>("WorldEventController");
        RecordInitialWorldEvent(
            worldEvents,
            main.GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource"));
        InitializeSabotageTurnOpportunity(main);
        WorldObjectController worldObjects =
            main.GetNode<WorldObjectController>("WorldObjectController");
        if (!worldObjects.TryInitializeWorldObjects(
                main.GetNode<TemporaryWorldObjectSource>("TemporaryWorldObjectSource").WorldObjects))
        {
            throw new InvalidOperationException(
                "The world-object controller could not initialize the scene snapshot.");
        }

        InitializeIndependentTask(main, healer);
        InitializeSeekerPerception(main, worldObjects);
    }

    private static void RecordInitialWorldEvent(
        WorldEventController worldEvents,
        TemporaryWorldEventSource source)
    {
        WorldEvent worldEvent = worldEvents.FindWorldEvent(source.InitialWorldEvent.EventId)
            ?? throw new InvalidOperationException(
                "The initial greybox world event is absent from authoritative world events.");

        if (!worldEvents.TryOccur(worldEvent.EventId))
        {
            throw new InvalidOperationException(
                "The initial greybox world event could not occur.");
        }
    }

    private static void InitializeSabotageTurnOpportunity(Node main)
    {
        TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source =
            main.GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
                "TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        if (!main.GetNode<CharacterSabotageTurnOpportunityController>(
                "CharacterSabotageTurnOpportunityController").TryInitialize(
                source.Actor,
                new CharacterSabotageTurnOpportunityCadence(SabotageTurnMeaningfulEventInterval)))
        {
            throw new InvalidOperationException(
                "The sabotage-turn opportunity controller could not initialize inertly.");
        }
    }

    private static void InitializeIndependentTask(Node main, Healer healer)
    {
        TemporaryCharacterIndependentTaskSource source =
            main.GetNode<TemporaryCharacterIndependentTaskSource>(
                "TemporaryCharacterIndependentTaskSource");
        CharacterIndependentTaskDepartureController departure =
            main.GetNode<CharacterIndependentTaskDepartureController>(
                "CharacterIndependentTaskDepartureController");
        CharacterIndependentTaskActivitySnapshot activities = source.Activities;
        CharacterPartySnapshot initialParty = source.Party;
        if (!departure.TryInitialize(activities, initialParty)
            || !departure.TryStartAndSeparate(source.DepartingCharacterId, activities, initialParty))
        {
            throw new InvalidOperationException(
                "The authored independent-task departure could not start atomically.");
        }

        CharacterIndependentTaskActivity activeActivity = departure.ActiveActivity
            ?? throw new InvalidOperationException(
                "The authored independent task did not retain its active departure state.");
        NumericsVector2 origin = new(healer.GlobalPosition.X, healer.GlobalPosition.Y);
        CharacterIndependentTaskTravelPlan plan = main
            .GetNode<TemporaryCharacterIndependentTaskTravelSource>(
                "TemporaryCharacterIndependentTaskTravelSource")
            .CreatePlan(activeActivity, origin);
        if (!main.GetNode<CharacterIndependentTaskTravelController>(
                "CharacterIndependentTaskTravelController").TryInitialize(plan))
        {
            throw new InvalidOperationException(
                "The authored independent-task travel plan could not initialize.");
        }

        if (!main.GetNode<CharacterIndependentTaskStepClockController>(
                "CharacterIndependentTaskStepClockController").TryInitialize(
                new CharacterIndependentTaskStepCadence(IndependentTaskStepInterval)))
        {
            throw new InvalidOperationException(
                "The authored independent-task step clock could not initialize.");
        }

        CharacterPartySnapshot party = departure.Party
            ?? throw new InvalidOperationException(
                "The independent-task party state was not initialized for body presentation.");
        if (!main.GetNode<CharacterIndependentTaskBodyPresentationController>(
                "CharacterIndependentTaskBodyPresentationController").TryInitialize(
                source.DepartingCharacterId,
                healer,
                party))
        {
            throw new InvalidOperationException(
                "The authored independent-task body presentation could not initialize.");
        }
    }

    private static void InitializeSeekerPerception(Node main, WorldObjectController worldObjects)
    {
        FocusedSearchActivityController searches =
            main.GetNode<FocusedSearchActivityController>("Seeker/FocusedSearchActivityController");
        if (!searches.TryInitializePerception(
                main.GetNode<TemporarySeekerPerceptionSource>("TemporarySeekerPerceptionSource")
                    .Perception))
        {
            throw new InvalidOperationException(
                "The focused-search controller could not initialize Seeker perception.");
        }

        Character seeker =
            main.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource").Character;
        PassiveNoticingCandidate noticing = main
            .GetNode<TemporaryPassiveNoticingCandidateSource>("TemporaryPassiveNoticingCandidateSource")
            .Candidate;
        WorldObject noticedObject = worldObjects.FindWorldObject(noticing.TargetObjectId)
            ?? throw new InvalidOperationException(
                "The passive-noticing candidate target is absent from live world objects.");
        if (!searches.TryProducePassiveNoticingCandidate(noticing, seeker, noticedObject))
        {
            throw new InvalidOperationException(
                "The passive-noticing candidate could not be produced for the Seeker.");
        }

        if (!searches.TryResolveNextPassiveNoticingCandidate())
        {
            throw new InvalidOperationException(
                "The initial passive-noticing candidate could not be resolved for the Seeker.");
        }

        OpeningRegionStreamResiduePassiveNoticingCoordinator streamResidue =
            main.GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
                "OpeningRegionStreamResiduePassiveNoticingCoordinator");
        if (!streamResidue.TryResolve(
                streamResidue.Perception,
                streamResidue.Candidates,
                streamResidue.Candidate))
        {
            throw new InvalidOperationException(
                "The opening-region stream residue could not be passively noticed.");
        }

        ObservationReinterpretationCandidate reinterpretation = main
            .GetNode<TemporaryObservationReinterpretationCandidateSource>(
                "TemporaryObservationReinterpretationCandidateSource")
            .Candidate;
        WorldObject reinterpretedObject = worldObjects.FindWorldObject(reinterpretation.TargetObjectId)
            ?? throw new InvalidOperationException(
                "The observation-reinterpretation candidate target is absent from live world objects.");
        if (!searches.TryProduceObservationReinterpretationCandidate(
                reinterpretation,
                seeker,
                reinterpretedObject))
        {
            throw new InvalidOperationException(
                "The observation-reinterpretation candidate could not be produced for the Seeker.");
        }

        if (!searches.TryResolveNextObservationReinterpretationCandidate(seeker))
        {
            throw new InvalidOperationException(
                "The initial observation-reinterpretation candidate could not be resolved for the Seeker.");
        }
    }
}
