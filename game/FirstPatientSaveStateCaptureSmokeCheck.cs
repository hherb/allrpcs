using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact first-patient scene-state capture when explicitly enabled.</summary>
public partial class FirstPatientSaveStateCaptureSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_FIRST_PATIENT_SAVE_STATE_CAPTURE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FirstPatientStateController stateController =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        FirstPatientPreparationController preparationController =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        WorldEventController worldEventController =
            GetNode<WorldEventController>("../WorldEventController");
        FocusedSearchActivityController discoveryController =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        CharacterMemoryController memoryController =
            GetNode<CharacterMemoryController>("../CharacterMemoryController");
        DialogueEventController dialogueController =
            GetNode<DialogueEventController>("../DialogueEventController");
        CharacterTurnActivityController turnController =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterModelStreamController modelController =
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        Label focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        Label modelStatus = GetNode<Label>("../DialogueOverlay/Panel/ModelStatus");

        DiseaseTruthSnapshot diseaseTruth = stateController.DiseaseTruth!;
        PreparedIngredientBatchSnapshot preparedBatches = stateController.PreparedBatches!;
        IngredientExperimentJournalSnapshot administrationExperiments =
            stateController.AdministrationExperiments!;
        ExperimentJournalSnapshot experiments = stateController.Experiments!;
        IngredientPreparationRecipe? recipe = preparationController.Recipe;
        IngredientStateSnapshot? ingredients = preparationController.Ingredients;
        IngredientPreparationActivity? activity = preparationController.Activity;
        IngredientPreparationOutcome? outcome = preparationController.Outcome;
        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        CharacterPerceptionSnapshot? perception = discoveryController.Perception;
        FocusedSearchActivitySnapshot searchActivities = discoveryController.Activities;
        FocusedSearchOutcomeSnapshot searchOutcomes = discoveryController.Outcomes;
        PassiveNoticingCandidateSnapshot passiveCandidates =
            discoveryController.PassiveNoticingCandidates;
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates =
            discoveryController.ObservationReinterpretationCandidates;
        CharacterMemorySnapshot memory = memoryController.Memory;
        DialogueEvent? dialogueEvent = dialogueController.LastOccurredEvent;
        CharacterTurnActivity? turnActivity = turnController.Activity;
        CharacterModelStreamSnapshot? modelStream = modelController.Snapshot;
        Vector2 seekerPosition = seeker.Position;
        Vector2 seekerVelocity = seeker.Velocity;
        string focusText = focusStatus.Text;
        string modelText = modelStatus.Text;

        FirstPatientSaveState first = stateController.Capture()
            ?? throw new InvalidOperationException("Initialized first-patient state was not captured.");
        FirstPatientSaveState second = stateController.Capture()
            ?? throw new InvalidOperationException("Repeated first-patient state was not captured.");
        FirstPatientStateController uninitialized = new();

        if (ReferenceEquals(first, second)
            || !HasExactFirstPatientState(first, stateController)
            || !HasExactFirstPatientState(second, stateController)
            || uninitialized.Capture() is not null
            || !ReferenceEquals(stateController.DiseaseTruth, diseaseTruth)
            || !ReferenceEquals(stateController.PreparedBatches, preparedBatches)
            || !ReferenceEquals(
                stateController.AdministrationExperiments,
                administrationExperiments)
            || !ReferenceEquals(stateController.Experiments, experiments)
            || !ReferenceEquals(preparationController.Recipe, recipe)
            || !ReferenceEquals(preparationController.Ingredients, ingredients)
            || !ReferenceEquals(preparationController.Activity, activity)
            || !ReferenceEquals(preparationController.Outcome, outcome)
            || !ReferenceEquals(worldEventController.WorldEvents, worldEvents)
            || !ReferenceEquals(worldEventController.EventLog, eventLog)
            || !ReferenceEquals(worldEventController.WorldTime, worldTime)
            || !ReferenceEquals(discoveryController.Perception, perception)
            || !ReferenceEquals(discoveryController.Activities, searchActivities)
            || !ReferenceEquals(discoveryController.Outcomes, searchOutcomes)
            || !ReferenceEquals(discoveryController.PassiveNoticingCandidates, passiveCandidates)
            || !ReferenceEquals(
                discoveryController.ObservationReinterpretationCandidates,
                reinterpretationCandidates)
            || !ReferenceEquals(memoryController.Memory, memory)
            || !ReferenceEquals(dialogueController.LastOccurredEvent, dialogueEvent)
            || !ReferenceEquals(turnController.Activity, turnActivity)
            || !ReferenceEquals(modelController.Snapshot, modelStream)
            || seeker.Position != seekerPosition
            || seeker.Velocity != seekerVelocity
            || !string.Equals(focusStatus.Text, focusText, StringComparison.Ordinal)
            || !string.Equals(modelStatus.Text, modelText, StringComparison.Ordinal))
        {
            Fail("First-patient save-state capture changed live or unrelated scene state.");
            return;
        }

        uninitialized.Free();
        GD.Print(
            "First-patient save-state capture smoke passed: fresh compositions retained all four "
            + "exact initialized snapshots, uninitialized capture failed closed, and world-event, "
            + "preparation, discovery, memory, dialogue, movement, model, and presentation state "
            + "remained unchanged.");
        GetTree().Quit();
    }

    private static bool HasExactFirstPatientState(
        FirstPatientSaveState state,
        FirstPatientStateController controller) =>
        ReferenceEquals(state.DiseaseTruth, controller.DiseaseTruth)
        && ReferenceEquals(state.PreparedBatches, controller.PreparedBatches)
        && ReferenceEquals(
            state.AdministrationExperiments,
            controller.AdministrationExperiments)
        && ReferenceEquals(state.Experiments, controller.Experiments);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
