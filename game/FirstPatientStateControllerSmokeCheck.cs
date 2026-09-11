using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free first-patient scene-state checks when explicitly enabled.</summary>
public partial class FirstPatientStateControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FIRST_PATIENT_STATE_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        FirstPatientStateController controller =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        FirstPatientScenario scenario = context.Scenario;
        WorldEvent worldEvent = CreateWorldEvent();
        IngredientAdministrationOutcome administration = CreateAdministration(scenario, worldEvent);
        PreparedIngredientBatch batch = new(administration.Dosage.Preparation);
        PreparedIngredientBatchSnapshot batches = new([batch]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([worldEvent.EventId]);

        if (!controller.TryInitialize(
                scenario.DiseaseTruth,
                batches,
                administrationExperiments,
                experiments)
            || !controller.TryStabilize(
                new ExperimentId("first-patient-scene-dose"),
                context,
                administration,
                new CharacterConditionSet([]),
                "Administer one cooled stabilizing remedy dose.",
                "The patient's breathing becomes steadier.",
                worldEvents,
                eventLog))
        {
            Fail("First-patient state controller rejected exact initialization or stabilization.");
            return;
        }

        DiseaseTruthSnapshot resultingTruth = controller.DiseaseTruth!;
        PreparedIngredientBatchSnapshot resultingBatches = controller.PreparedBatches!;
        IngredientExperimentJournalSnapshot resultingAdministrationExperiments =
            controller.AdministrationExperiments!;
        ExperimentJournalSnapshot resultingExperiments = controller.Experiments!;
        if (!ReferenceEquals(
                AssertSingle(resultingTruth.Infections).CareState,
                DiseaseCareState.Stabilized)
            || resultingBatches.Batches.Count != 0
            || resultingAdministrationExperiments.Records.Count != 1
            || resultingExperiments.Records.Count != 1
            || !ReferenceEquals(
                resultingAdministrationExperiments.Records[0].Administration,
                administration)
            || !ReferenceEquals(resultingExperiments.Records[0].WorldEvent, worldEvent)
            || controller.TryStabilize(
                new ExperimentId("stale-repeat"),
                context,
                administration,
                new CharacterConditionSet([]),
                "Repeat a stale dose.",
                "No second result should be installed.",
                worldEvents,
                eventLog)
            || !ReferenceEquals(controller.DiseaseTruth, resultingTruth)
            || !ReferenceEquals(controller.PreparedBatches, resultingBatches)
            || !ReferenceEquals(
                controller.AdministrationExperiments,
                resultingAdministrationExperiments)
            || !ReferenceEquals(controller.Experiments, resultingExperiments)
            || controller.TryInitialize(
                scenario.DiseaseTruth,
                batches,
                administrationExperiments,
                experiments)
            || !HasClosedSurface())
        {
            Fail("First-patient state controller failed atomic result or rejection preservation.");
            return;
        }

        FirstPatientStateController uninitialized = new();
        AddChild(uninitialized);
        if (uninitialized.TryStabilize(
                new ExperimentId("uninitialized-dose"),
                context,
                administration,
                new CharacterConditionSet([]),
                "Attempt one dose.",
                "No state is available.",
                worldEvents,
                eventLog)
            || uninitialized.DiseaseTruth is not null
            || uninitialized.PreparedBatches is not null
            || uninitialized.AdministrationExperiments is not null
            || uninitialized.Experiments is not null)
        {
            Fail("First-patient state controller changed uninitialized state.");
            return;
        }

        GD.Print(
            "First-patient state controller smoke passed: exact one-time initialization, atomic "
            + "prepared-dose stabilization and dual-journal installation, and rejected stale and "
            + "uninitialized operations preserved every snapshot.");
        GetTree().Quit();
    }

    private static CharacterDiseaseState AssertSingle(
        IReadOnlyList<CharacterDiseaseState> infections) => infections.Count == 1
            ? infections[0]
            : throw new InvalidOperationException("Expected one first-patient infection.");

    private static WorldEvent CreateWorldEvent() => new(
        new WorldEventId("first-patient-scene-stabilized"),
        WorldEventTimeEffect.NoAdvance);

    private static IngredientAdministrationOutcome CreateAdministration(
        FirstPatientScenario scenario,
        WorldEvent worldEvent)
    {
        IngredientPreparationRecipe recipe = scenario.Remedy.Recipe;
        IngredientPreparationProgress progress = new(recipe.Plan, recipe.Plan.Steps.Count);
        IngredientPreparationActivity activity = new(recipe, progress);
        IngredientPreparationOutcome preparation = new(
            activity,
            recipe.ResultDefinition,
            recipe.ResultQuantity);
        return new IngredientAdministrationOutcome(
            new CharacterId("healer"),
            scenario.PatientId,
            new IngredientDosage(preparation, recipe.ResultQuantity),
            worldEvent);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(FirstPatientStateController);
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
        return properties.Length == 4
            && properties.All(property => property.CanRead && property.SetMethod?.IsPrivate == true)
            && methods.Length == 2
            && methods.Any(method => method.Name == nameof(FirstPatientStateController.TryInitialize))
            && methods.Any(method => method.Name == nameof(FirstPatientStateController.TryStabilize));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
