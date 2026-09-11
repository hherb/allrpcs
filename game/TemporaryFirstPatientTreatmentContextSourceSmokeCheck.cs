using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact first-patient scene initialization when explicitly enabled.</summary>
public partial class TemporaryFirstPatientTreatmentContextSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FIRST_PATIENT_CONTEXT_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        TemporaryFirstPatientTreatmentContextSource source =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "../TemporaryFirstPatientTreatmentContextSource");
        FirstPatientStateController controller =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        CharacterMemoryController memoryController =
            GetNode<CharacterMemoryController>("../CharacterMemoryController");
        WorldEventController worldEventController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnActivityController turnController =
            GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        CharacterModelStreamController streamController =
            GetNode<CharacterModelStreamController>("../CharacterModelStreamController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        Label modeStatus = GetNode<Label>("../CommandOverlay/Panel/ModeStatus");

        FirstPatientTreatmentContext firstContext = source.Context;
        FirstPatientTreatmentContext secondContext = source.Context;
        DiseaseTruthSnapshot diseaseTruth = controller.DiseaseTruth!;
        PreparedIngredientBatchSnapshot preparedBatches = controller.PreparedBatches!;
        IngredientExperimentJournalSnapshot administrationExperiments =
            controller.AdministrationExperiments!;
        ExperimentJournalSnapshot experiments = controller.Experiments!;
        CharacterMemorySnapshot memory = memoryController.Memory;
        object? eventLog = worldEventController.EventLog;
        object? turnActivity = turnController.Activity;
        object? stream = streamController.Snapshot;
        Vector2 seekerPosition = seeker.Position;
        Vector2 seekerVelocity = seeker.Velocity;
        string modeText = modeStatus.Text;

        if (!ReferenceEquals(firstContext, secondContext) || !HasClosedSourceSurface())
        {
            Fail("First-patient context source failed stable access or its closed surface.");
            return;
        }

        if (!ReferenceEquals(diseaseTruth, firstContext.Scenario.DiseaseTruth)
            || diseaseTruth.Infections.Count != 1
            || preparedBatches.Batches.Count != 0
            || administrationExperiments.Records.Count != 0
            || experiments.Records.Count != 0)
        {
            Fail("First-patient context source failed exact empty scene initialization.");
            return;
        }

        bool repeated = controller.TryInitialize(
            firstContext.Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        if (repeated
            || !ReferenceEquals(source.Context, firstContext)
            || !ReferenceEquals(controller.DiseaseTruth, diseaseTruth)
            || !ReferenceEquals(controller.PreparedBatches, preparedBatches)
            || !ReferenceEquals(controller.AdministrationExperiments, administrationExperiments)
            || !ReferenceEquals(controller.Experiments, experiments)
            || !ReferenceEquals(memoryController.Memory, memory)
            || !ReferenceEquals(worldEventController.EventLog, eventLog)
            || !ReferenceEquals(turnController.Activity, turnActivity)
            || !ReferenceEquals(streamController.Snapshot, stream)
            || seeker.Position != seekerPosition
            || seeker.Velocity != seekerVelocity
            || !string.Equals(modeStatus.Text, modeText, StringComparison.Ordinal))
        {
            Fail("Repeated first-patient initialization changed scene state.");
            return;
        }

        GD.Print(
            "Temporary first-patient context source smoke passed: one stable exact context, exact "
            + "disease installation, explicit empty batches and journals, and fail-closed repeated "
            + "initialization preserved world events, memory, movement, dialogue, model, and "
            + "presentation state.");
        GetTree().Quit();
    }

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryFirstPatientTreatmentContextSource);
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        return properties.Length == 1
            && properties[0].Name == "Context"
            && properties[0].PropertyType == typeof(FirstPatientTreatmentContext)
            && properties[0].CanRead
            && properties[0].SetMethod?.IsPrivate == true
            && methods.Length == 1
            && methods[0].Name == nameof(TemporaryFirstPatientTreatmentContextSource._Ready);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
