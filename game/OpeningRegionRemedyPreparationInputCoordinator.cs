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
    private const float MaximumInteractionDistancePixels = 48.0f;
    private static readonly StringName PreparationAction = new("remedy_prepare");

    private FirstPatientPreparationController preparation = null!;
    private OpeningRegionRemedyPreparationSceneSource scene = null!;
    private CharacterBody2D seeker = null!;
    private FirstPatientStateController state = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        preparation = GetNode<FirstPatientPreparationController>(
            "../FirstPatientPreparationController");
        scene = GetNode<OpeningRegionRemedyPreparationSceneSource>(
            "../OpeningRegionRemedyPreparationSceneSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
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
                ToNumerics(seeker.GlobalPosition)))
        {
            GetViewport().SetInputAsHandled();
        }
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
            return nextStepIndex < requestedRecipe.Plan.Steps.Count
                ? preparation.TryAdvance(requestedRecipe.Plan.Steps[nextStepIndex])
                : preparation.TryComplete();
        }

        return state.TryAddPreparedBatch(requestedContext, preparation.Outcome);
    }

    private bool RetainsExactFirstPatientState(FirstPatientSaveState requested) =>
        ReferenceEquals(state.DiseaseTruth, requested.DiseaseTruth)
        && ReferenceEquals(state.PreparedBatches, requested.PreparedBatches)
        && ReferenceEquals(
            state.AdministrationExperiments,
            requested.AdministrationExperiments)
        && ReferenceEquals(state.Experiments, requested.Experiments);

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
