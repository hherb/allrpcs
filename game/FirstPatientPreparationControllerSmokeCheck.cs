using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the first-patient preparation lifecycle when explicitly enabled.</summary>
public partial class FirstPatientPreparationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FIRST_PATIENT_PREPARATION_CHECK";
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
        FirstPatientPreparationController ordinary =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        FirstPatientTreatmentContext context = source.Context;
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        IngredientStateSnapshot initialIngredients = context.Scenario.Ingredients;
        if (!ReferenceEquals(ordinary.Recipe, recipe)
            || !ReferenceEquals(ordinary.Ingredients, initialIngredients)
            || ordinary.Activity is not null
            || ordinary.Outcome is not null
            || ordinary.TryInitialize(recipe, new IngredientStateSnapshot([])))
        {
            Fail("Ordinary first-patient preparation state was not initialized exactly once.");
            return;
        }

        FirstPatientPreparationController controller = new();
        AddChild(controller);
        IngredientPreparationRecipe replacementRecipe =
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.Remedy.Recipe;
        if (!controller.TryInitialize(recipe, initialIngredients)
            || controller.TryStart(replacementRecipe)
            || !controller.TryStart(recipe)
            || !ReferenceEquals(controller.Recipe, recipe)
            || ReferenceEquals(controller.Ingredients, initialIngredients)
            || controller.Activity is null
            || !ReferenceEquals(controller.Activity.Recipe, recipe)
            || controller.Activity.Progress.NextStepIndex != 0
            || controller.Outcome is not null)
        {
            Fail("First-patient preparation did not consume exact inputs and start atomically.");
            return;
        }

        IngredientStateSnapshot remainingIngredients = controller.Ingredients!;
        IngredientPreparationActivity startedActivity = controller.Activity;
        IngredientPreparationStep replacementStep = new(recipe.Plan.Steps[0].Kind);
        if (controller.TryStart(recipe)
            || controller.TryAdvance(replacementStep)
            || !ReferenceEquals(controller.Ingredients, remainingIngredients)
            || !ReferenceEquals(controller.Activity, startedActivity))
        {
            Fail("Rejected first-patient preparation operations changed controller state.");
            return;
        }

        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            if (!controller.TryAdvance(step)
                || !ReferenceEquals(controller.Activity!.Recipe, recipe))
            {
                Fail("First-patient preparation rejected an exact next authored step.");
                return;
            }
        }

        IngredientPreparationActivity completedActivity = controller.Activity!;
        if (!controller.TryComplete()
            || controller.Outcome is null
            || !ReferenceEquals(controller.Outcome.Activity, completedActivity)
            || !ReferenceEquals(controller.Outcome.Definition, recipe.ResultDefinition)
            || !ReferenceEquals(controller.Outcome.Quantity, recipe.ResultQuantity))
        {
            Fail("First-patient preparation did not retain the exact completed outcome.");
            return;
        }

        IngredientPreparationOutcome outcome = controller.Outcome;
        if (controller.TryAdvance(recipe.Plan.Steps[0])
            || controller.TryComplete()
            || !ReferenceEquals(controller.Ingredients, remainingIngredients)
            || !ReferenceEquals(controller.Activity, completedActivity)
            || !ReferenceEquals(controller.Outcome, outcome)
            || !ReferenceEquals(ordinary.Recipe, recipe)
            || !ReferenceEquals(ordinary.Ingredients, initialIngredients)
            || ordinary.Activity is not null
            || ordinary.Outcome is not null
            || !HasClosedSurface())
        {
            Fail("Completed or isolated preparation changed retained or ordinary scene state.");
            return;
        }

        FirstPatientStateController stateController =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        DiseaseTruthSnapshot diseaseTruth = stateController.DiseaseTruth!;
        PreparedIngredientBatchSnapshot initialBatches = stateController.PreparedBatches!;
        object? administrationExperiments = stateController.AdministrationExperiments;
        object? experiments = stateController.Experiments;
        FirstPatientTreatmentContext replacementContext =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        if (initialBatches.Batches.Count != 0
            || stateController.TryAddPreparedBatch(replacementContext, outcome)
            || !ReferenceEquals(stateController.PreparedBatches, initialBatches)
            || !stateController.TryAddPreparedBatch(context, outcome))
        {
            Fail("First-patient prepared-batch installation did not require exact scene state.");
            return;
        }

        PreparedIngredientBatchSnapshot installedBatches = stateController.PreparedBatches!;
        PreparedIngredientBatch installedBatch = AssertSingle(installedBatches.Batches);
        if (!ReferenceEquals(installedBatch.Preparation, outcome)
            || !ReferenceEquals(installedBatch.RemainingQuantity, outcome.Quantity)
            || stateController.TryAddPreparedBatch(context, outcome)
            || !ReferenceEquals(stateController.PreparedBatches, installedBatches)
            || !ReferenceEquals(stateController.DiseaseTruth, diseaseTruth)
            || !ReferenceEquals(
                stateController.AdministrationExperiments,
                administrationExperiments)
            || !ReferenceEquals(stateController.Experiments, experiments))
        {
            Fail("Prepared-batch installation was not atomic or duplicate-safe.");
            return;
        }

        GD.Print(
            "First-patient preparation controller smoke passed: exact scene ingredients, atomic "
            + "input consumption, exact ordered progression and completion, exact prepared-batch "
            + "installation, and rejected operations preserved all retained state.");
        GetTree().Quit();
    }

    private static PreparedIngredientBatch AssertSingle(
        IReadOnlyList<PreparedIngredientBatch> batches) => batches.Count == 1
            ? batches[0]
            : throw new InvalidOperationException("Expected one prepared batch.");

    private static bool HasClosedSurface()
    {
        Type type = typeof(FirstPatientPreparationController);
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
            && methods.Length == 4
            && methods.Any(method => method.Name == nameof(FirstPatientPreparationController.TryInitialize))
            && methods.Any(method => method.Name == nameof(FirstPatientPreparationController.TryStart))
            && methods.Any(method => method.Name == nameof(FirstPatientPreparationController.TryAdvance))
            && methods.Any(method => method.Name == nameof(FirstPatientPreparationController.TryComplete));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
