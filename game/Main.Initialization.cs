using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

// Initializes the Main scene's authored state in a fixed order during _Ready, failing fast when
// any owner rejects it.
public partial class Main
{
    private const int SabotageTurnMeaningfulEventInterval = 2;
    private static readonly TimeSpan IndependentTaskStepInterval = TimeSpan.FromSeconds(3);

    /// <summary>Initializes model, dialogue, transcript, first-patient and world-event state.</summary>
    private void InitializeBeforeExamination()
    {
        CharacterModelStreamController modelStreamController =
            GetNode<CharacterModelStreamController>("CharacterModelStreamController");
        if (!modelStreamController.TryInitialize(new CharacterModelStreamSnapshot([])))
        {
            throw new InvalidOperationException(
                "The character-model stream controller could not initialize empty.");
        }

        if (!GetNode<CharacterModelGenerationFlowController>(
                "CharacterModelGenerationFlowController").TryInitialize(
                modelStreamController,
                GetNode<TemporaryCharacterModelGenerationStatusLabel>(
                    "DialogueOverlay/Panel/ModelStatus"),
                GetNode<TemporaryCharacterModelStreamTextLabel>(
                    "DialogueOverlay/Panel/ModelStreamText")))
        {
            throw new InvalidOperationException(
                "The character-model generation flow could not initialize.");
        }

        TemporarySeekerCharacterSource seeker =
            GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource");
        if (!GetNode<DialogueInputController>("DialogueInputController").TryInitialize(
                seeker.Character.Identity.Id))
        {
            throw new InvalidOperationException(
                "The dialogue-input controller could not initialize the scene Seeker.");
        }

        if (!GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController")
                .TryInitialize(new CharacterTurnTranscriptSnapshot([])))
        {
            throw new InvalidOperationException(
                "The character-turn transcript controller could not initialize empty.");
        }

        TemporaryFirstPatientTreatmentContextSource firstPatient =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "TemporaryFirstPatientTreatmentContextSource");
        if (!GetNode<FirstPatientStateController>("FirstPatientStateController").TryInitialize(
                firstPatient.Context.Scenario.DiseaseTruth,
                new PreparedIngredientBatchSnapshot([]),
                new IngredientExperimentJournalSnapshot([]),
                new ExperimentJournalSnapshot([])))
        {
            throw new InvalidOperationException(
                "The first-patient controller could not initialize the authored scene state.");
        }

        if (!GetNode<FirstPatientPreparationController>("FirstPatientPreparationController")
                .TryInitialize(firstPatient.Context.Scenario))
        {
            throw new InvalidOperationException(
                "The first-patient preparation controller could not initialize scene ingredients.");
        }

        // The Seeker starts with the field lens focused search requires; the Healer carries nothing.
        Character healerCharacter =
            GetNode<TemporaryHealerCharacterSource>("TemporaryHealerCharacterSource").Character;
        CharacterInventory lens =
            GetNode<TemporaryFocusedSearchContextSource>("TemporaryFocusedSearchContextSource").Inventory;
        if (!GetNode<InventoryController>("InventoryController").TryInitialize(
                [
                    (seeker.Character.Identity.Id,
                        new CharacterInventory(seeker.Character.Inventory.ObjectReferences
                            .Concat(lens.ObjectReferences))),
                    (healerCharacter.Identity.Id, healerCharacter.Inventory),
                ]))
        {
            throw new InvalidOperationException("The inventory could not initialize the authored characters.");
        }

        if (!GetNode<WorldEventController>("WorldEventController").TryInitialize(
                GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource").WorldEvents,
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
    private void InitializeAfterExamination()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("WorldEventController");
        RecordInitialWorldEvent(
            worldEvents,
            GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource"));
        InitializeSabotageTurnOpportunity();
        if (!worldObjectController.TryInitializeWorldObjects(
                GetNode<TemporaryWorldObjectSource>("TemporaryWorldObjectSource").WorldObjects))
        {
            throw new InvalidOperationException(
                "The world-object controller could not initialize the scene snapshot.");
        }

        InitializeIndependentTask();
        InitializeSeekerPerception();
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

    private void InitializeSabotageTurnOpportunity()
    {
        TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source =
            GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
                "TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        if (!GetNode<CharacterSabotageTurnOpportunityController>(
                "CharacterSabotageTurnOpportunityController").TryInitialize(
                source.Actor,
                new CharacterSabotageTurnOpportunityCadence(SabotageTurnMeaningfulEventInterval)))
        {
            throw new InvalidOperationException(
                "The sabotage-turn opportunity controller could not initialize inertly.");
        }
    }

    private void InitializeIndependentTask()
    {
        TemporaryCharacterIndependentTaskSource source =
            GetNode<TemporaryCharacterIndependentTaskSource>(
                "TemporaryCharacterIndependentTaskSource");
        CharacterIndependentTaskDepartureController departure =
            GetNode<CharacterIndependentTaskDepartureController>(
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
        CharacterIndependentTaskTravelPlan plan =
            GetNode<TemporaryCharacterIndependentTaskTravelSource>(
                "TemporaryCharacterIndependentTaskTravelSource")
            .CreatePlan(activeActivity, origin);
        if (!GetNode<CharacterIndependentTaskTravelController>(
                "CharacterIndependentTaskTravelController").TryInitialize(plan))
        {
            throw new InvalidOperationException(
                "The authored independent-task travel plan could not initialize.");
        }

        if (!GetNode<CharacterIndependentTaskStepClockController>(
                "CharacterIndependentTaskStepClockController").TryInitialize(
                new CharacterIndependentTaskStepCadence(IndependentTaskStepInterval)))
        {
            throw new InvalidOperationException(
                "The authored independent-task step clock could not initialize.");
        }

        CharacterPartySnapshot party = departure.Party
            ?? throw new InvalidOperationException(
                "The independent-task party state was not initialized for body presentation.");
        if (!GetNode<CharacterIndependentTaskBodyPresentationController>(
                "CharacterIndependentTaskBodyPresentationController").TryInitialize(
                source.DepartingCharacterId,
                healer,
                party))
        {
            throw new InvalidOperationException(
                "The authored independent-task body presentation could not initialize.");
        }
    }

    // Installs the authored starting perception only: nothing is noticed or reinterpreted at boot,
    // so every discovery comes from play (focused search, arrivals, examinations).
    private void InitializeSeekerPerception()
    {
        if (!focusedSearchController.TryInitializePerception(
                GetNode<TemporarySeekerPerceptionSource>("TemporarySeekerPerceptionSource")
                    .Perception))
        {
            throw new InvalidOperationException(
                "The focused-search controller could not initialize Seeker perception.");
        }
    }
}
