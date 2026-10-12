using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Advances the exact ordinary healer-garden preparation one input at a time.</summary>
public partial class OpeningRegionRemedyPreparationInputCoordinator : Node
{
    /// <summary>Presentation-only notification after an exact committed grinding step.</summary>
    public event Action<IngredientPreparationRecipe, IngredientPreparationStep, Vector2>? GrindingCompleted;

    /// <summary>Independent presentation notification after the exact successful heating commit.</summary>
    internal event Action<HeatingPreparationCommit>? HeatingCompleted;
    private HeatingPreparationCommit? heatingCommit;

    /// <summary>Admits only the exact currently owned committed token and current activity.</summary>
    internal bool OwnsHeating(HeatingPreparationCommit? commit) => commit is not null
        && ReferenceEquals(heatingCommit, commit) && ReferenceEquals(commit.Owner, this)
        && ReferenceEquals(scene.Binding, commit.Binding) && ReferenceEquals(preparation.Activity, commit.After)
        && ReferenceEquals(preparation.Recipe, commit.Binding.Recipe);

    internal const string ReadyStatusText = "Preparation: go to the station and press P.";
    internal const string MoveCloserStatusText = "Preparation: move closer to the station.";
    internal const string StartedStatusText = "Preparation: started. Press P for step 1.";
    internal const string CompletedStatusText =
        "Preparation: complete. Press P to prepare the batch.";
    internal const string BatchReadyStatusText =
        "Preparation: batch ready. Go to the sickhouse and press T.";

    private const float MaximumInteractionDistancePixels = 48.0f;
    private static readonly StringName PreparationAction = new("remedy_prepare");

    private FirstPatientPreparationController preparation = null!;
    private OpeningRegionRemedyPreparationSceneSource scene = null!;
    private CharacterBody2D seeker = null!;
    private FirstPatientStateController state = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        preparation = GetNode<FirstPatientPreparationController>(
            "../FirstPatientPreparationController");
        scene = GetNode<OpeningRegionRemedyPreparationSceneSource>(
            "../OpeningRegionRemedyPreparationSceneSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        status.Text = ReadyStatusText;
        var restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        void ResetHeating() => heatingCommit = null;
        restoration.PresentationReset += ResetHeating;
        TreeExiting += () => { restoration.PresentationReset -= ResetHeating; heatingCommit = null; };
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!@event.IsActionPressed(PreparationAction))
        {
            return;
        }

        OpeningRegionRemedyPreparationSceneBinding? binding = scene.Binding;
        FirstPatientTreatmentContext? context = binding?.TreatmentContext;
        IngredientPreparationRecipe? recipe = preparation.Recipe;
        IngredientStateSnapshot? ingredients = preparation.Ingredients;
        FirstPatientSaveState? firstPatientState = state.Capture();
        NumericsVector2 seekerPosition = ToNumerics(seeker.GlobalPosition);
        if (binding is not null
            && context is not null
            && recipe is not null
            && ingredients is not null
            && firstPatientState is not null
            && TryProgress(
                binding,
                context,
                recipe,
                ingredients,
                firstPatientState,
                seekerPosition))
        {
            status.Text = CurrentStatusText(recipe);
        }
        else if (binding is not null
            && NumericsVector2.Distance(seekerPosition, binding.StationPosition)
                > MaximumInteractionDistancePixels)
        {
            status.Text = MoveCloserStatusText;
        }
        else if (recipe is not null)
        {
            status.Text = CurrentStatusText(recipe);
        }

        GetViewport().SetInputAsHandled();
    }

    /// <summary>Performs only the exact next preparation operation from exact current state.</summary>
    public bool TryProgress(
        OpeningRegionRemedyPreparationSceneBinding requestedBinding,
        FirstPatientTreatmentContext requestedContext,
        IngredientPreparationRecipe requestedRecipe,
        IngredientStateSnapshot requestedIngredients,
        FirstPatientSaveState requestedFirstPatientState,
        NumericsVector2 seekerPosition)
    {
        ArgumentNullException.ThrowIfNull(requestedBinding);
        ArgumentNullException.ThrowIfNull(requestedContext);
        ArgumentNullException.ThrowIfNull(requestedRecipe);
        ArgumentNullException.ThrowIfNull(requestedIngredients);
        ArgumentNullException.ThrowIfNull(requestedFirstPatientState);

        OpeningRegionRemedyPreparationSceneBinding? currentBinding = scene.Binding;
        if (!float.IsFinite(seekerPosition.X)
            || !float.IsFinite(seekerPosition.Y)
            || currentBinding is null
            || !ReferenceEquals(requestedBinding, currentBinding)
            || !ReferenceEquals(requestedContext, currentBinding.TreatmentContext)
            || !ReferenceEquals(requestedRecipe, currentBinding.Recipe)
            || !ReferenceEquals(preparation.Recipe, requestedRecipe)
            || !ReferenceEquals(preparation.Ingredients, requestedIngredients)
            || !RetainsExactFirstPatientState(requestedFirstPatientState)
            || NumericsVector2.Distance(seekerPosition, currentBinding.StationPosition)
                > MaximumInteractionDistancePixels)
        {
            return false;
        }

        IngredientPreparationActivity? activity = preparation.Activity;
        if (activity is null)
        {
            return preparation.Outcome is null && preparation.TryStart(requestedRecipe);
        }

        if (preparation.Outcome is null)
        {
            int nextStepIndex = activity.Progress.NextStepIndex;
            if (nextStepIndex >= requestedRecipe.Plan.Steps.Count) { return preparation.TryComplete(); }
            IngredientPreparationStep step = requestedRecipe.Plan.Steps[nextStepIndex];
            bool advanced = preparation.TryAdvance(step);
            if (advanced && OpeningRegionMaterialAudioProjection.IsGrinding(requestedRecipe, step))
            {
                foreach (Delegate listener in GrindingCompleted?.GetInvocationList() ?? [])
                {
                    try
                    {
                        ((Action<IngredientPreparationRecipe, IngredientPreparationStep, Vector2>)listener)(
                            requestedRecipe, step, new Vector2(currentBinding.StationPosition.X, currentBinding.StationPosition.Y));
                    }
                    catch (Exception exception) { GD.PrintErr($"Grinding presentation failed: {exception.Message}"); }
                }
            }
            if (advanced && RemedyHeatingPresentationProjection.IsCommit(requestedRecipe, step, activity, preparation.Activity)
                && RemedyHeatingPresentationProjection.Local(ToNumerics(seeker.GlobalPosition), seekerPosition, currentBinding.StationPosition))
            {
                heatingCommit = new(this, currentBinding, activity, preparation.Activity!, step, seeker.GlobalPosition);
                foreach (Delegate listener in HeatingCompleted?.GetInvocationList() ?? [])
                {
                    try { ((Action<HeatingPreparationCommit>)listener)(heatingCommit); }
                    catch (Exception exception) { GD.PrintErr($"Heating presentation failed: {exception.Message}"); }
                }
            }
            return advanced;
        }

        IngredientPreparationOutcome outcome = preparation.Outcome;
        if (state.AdministrationExperiments?.Records.Any(record =>
                ReferenceEquals(record.Administration.Dosage.Preparation, outcome)) == true)
        {
            return false;
        }

        return state.TryAddPreparedBatch(requestedContext, outcome);
    }

    private bool RetainsExactFirstPatientState(FirstPatientSaveState requested) =>
        ReferenceEquals(state.DiseaseTruth, requested.DiseaseTruth)
        && ReferenceEquals(state.PreparedBatches, requested.PreparedBatches)
        && ReferenceEquals(
            state.AdministrationExperiments,
            requested.AdministrationExperiments)
        && ReferenceEquals(state.Experiments, requested.Experiments);

    internal static string StepStatusText(int completedStepCount, int totalStepCount) =>
        completedStepCount == totalStepCount
            ? $"Preparation: step {completedStepCount}/{totalStepCount} complete. Press P to finish."
            : $"Preparation: step {completedStepCount}/{totalStepCount} complete. Press P for the next step.";

    private string CurrentStatusText(IngredientPreparationRecipe recipe)
    {
        IngredientPreparationOutcome? outcome = preparation.Outcome;
        if (outcome is not null
            && state.PreparedBatches?.Batches.Any(batch =>
                ReferenceEquals(batch.Preparation, outcome)) == true)
        {
            return BatchReadyStatusText;
        }

        if (outcome is not null)
        {
            return CompletedStatusText;
        }

        IngredientPreparationActivity? activity = preparation.Activity;
        if (activity is null)
        {
            return ReadyStatusText;
        }

        int completedStepCount = activity.Progress.NextStepIndex;
        return completedStepCount == 0
            ? StartedStatusText
            : StepStatusText(completedStepCount, recipe.Plan.Steps.Count);
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
