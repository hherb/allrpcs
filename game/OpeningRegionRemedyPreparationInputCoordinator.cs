using Allrpcs.Application.Audio;
using Allrpcs.Application.Diseases;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
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

    /// <summary>Independent presentation notification after the exact successful steeping commit.</summary>
    internal event Action<SteepingPreparationCommit>? SteepingCompleted;
    private SteepingPreparationCommit? steepingCommit;

    /// <summary>Admits only the exact currently owned committed token and current activity.</summary>
    internal bool OwnsSteeping(SteepingPreparationCommit? commit) => commit is not null
        && ReferenceEquals(steepingCommit, commit) && ReferenceEquals(commit.Owner, this)
        && ReferenceEquals(scene.Binding, commit.Binding) && ReferenceEquals(preparation.Activity, commit.After)
        && ReferenceEquals(preparation.Recipe, commit.Binding.Recipe);

    /// <summary>Independent presentation notification after the exact successful cooling commit.</summary>
    internal event Action<CoolingPreparationCommit>? CoolingCompleted;
    private CoolingPreparationCommit? coolingCommit;

    /// <summary>Admits only the exact currently owned committed token and current activity.</summary>
    internal bool OwnsCooling(CoolingPreparationCommit? commit) => commit is not null
        && ReferenceEquals(coolingCommit, commit) && ReferenceEquals(commit.Owner, this)
        && ReferenceEquals(scene.Binding, commit.Binding) && ReferenceEquals(preparation.Activity, commit.After)
        && ReferenceEquals(preparation.Recipe, commit.Binding.Recipe);

    internal const string ReadyStatusText =
        "Preparation: bring moonwort to the station, or press P there to use its unlabelled dried leaves.";
    internal const string CarryingStatusText =
        "Preparation: you carry moonwort leaves. Press P at the station to prepare the remedy.";
    internal const string MoveCloserStatusText = "Preparation: move closer to the station.";
    internal const string StartedStatusText = "Preparation: started. Press P for step 1.";
    internal const string LookAlikeStartedStatusText =
        "Preparation: started with the station's unidentified dried leaves. Press P for step 1.";
    internal const string FailedDoseStatusText =
        "Preparation: that dose changed nothing. Press P at the station to prepare another.";
    internal const string CompletedStatusText =
        "Preparation: complete. Press P to prepare the batch.";
    internal const string BatchReadyStatusText =
        "Preparation: batch ready. Go to the sickhouse and press T.";

    private const float MaximumInteractionDistancePixels = 48.0f;
    // A following Healer stops 72 px away, so a hand-over reaches a little further than that.
    private const float MaximumHandOverDistancePixels = 120.0f;
    private static readonly StringName PreparationAction = new("remedy_prepare");

    private FirstPatientPreparationController preparation = null!;
    private OpeningRegionRemedyPreparationSceneSource scene = null!;
    private CharacterBody2D seeker = null!;
    private Healer healer = null!;
    // The character sources are ready only after this node, so their ids are read on use.
    private CharacterId SeekerId =>
        GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id;
    private CharacterId HealerId =>
        GetNode<TemporaryHealerCharacterSource>("../TemporaryHealerCharacterSource").Character.Identity.Id;
    private InventoryController inventory = null!;
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
        healer = GetNode<Healer>("../Healer");
        inventory = GetNode<InventoryController>("../InventoryController");
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        ControlSettingsController.SetGuidance(status, ReadyStatusText);
        var restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        void ResetHeating() => heatingCommit = null;
        void ResetSteeping() => steepingCommit = null;
        void ResetCooling() => coolingCommit = null;
        restoration.PresentationReset += ResetCooling;
        TreeExiting += () => { restoration.PresentationReset -= ResetCooling; coolingCommit = null; };
        restoration.PresentationReset += ResetSteeping;
        TreeExiting += () => { restoration.PresentationReset -= ResetSteeping; steepingCommit = null; };
        restoration.PresentationReset += ResetHeating;
        TreeExiting += () => { restoration.PresentationReset -= ResetHeating; heatingCommit = null; };
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.WorldAllowed(this)) { return; }
        ArgumentNullException.ThrowIfNull(@event);

        if (!ControlInputMatcher.Pressed(@event, PreparationAction))
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
            ControlSettingsController.SetGuidance(status, CurrentStatusText(recipe));
        }
        else if (binding is not null
            && NumericsVector2.Distance(seekerPosition, binding.StationPosition)
                > MaximumInteractionDistancePixels)
        {
            ControlSettingsController.SetGuidance(status, MoveCloserStatusText);
        }
        else if (recipe is not null)
        {
            ControlSettingsController.SetGuidance(status, CurrentStatusText(recipe));
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

        if (state.AdministrationExperiments is { } doses)
        {
            _ = preparation.TryRestartAfterFailedDose(doses);
        }

        IngredientPreparationActivity? activity = preparation.Activity;
        if (activity is null)
        {
            return preparation.Outcome is null && TryIdleAction(requestedContext, requestedRecipe);
        }

        if (preparation.Outcome is null)
        {
            // A look-alike preparation follows the same steps from its own recipe.
            IngredientPreparationRecipe activeRecipe = activity.Recipe;
            int nextStepIndex = activity.Progress.NextStepIndex;
            if (nextStepIndex >= activeRecipe.Plan.Steps.Count) { return preparation.TryComplete(); }
            IngredientPreparationStep step = activeRecipe.Plan.Steps[nextStepIndex];
            bool advanced = preparation.TryAdvance(step);
            if (advanced && OpeningRegionMaterialAudioProjection.IsGrinding(activeRecipe, step))
            {
                foreach (Delegate listener in GrindingCompleted?.GetInvocationList() ?? [])
                {
                    try
                    {
                        ((Action<IngredientPreparationRecipe, IngredientPreparationStep, Vector2>)listener)(
                            activeRecipe, step, new Vector2(currentBinding.StationPosition.X, currentBinding.StationPosition.Y));
                    }
                    catch (Exception exception) { GD.PrintErr($"Grinding presentation failed: {exception.Message}"); }
                }
            }
            if (advanced && RemedyHeatingPresentationProjection.IsCommit(activeRecipe, step, activity, preparation.Activity)
                && RemedyHeatingPresentationProjection.Local(ToNumerics(seeker.GlobalPosition), seekerPosition, currentBinding.StationPosition))
            {
                heatingCommit = new(this, currentBinding, activity, preparation.Activity!, step, seeker.GlobalPosition);
                foreach (Delegate listener in HeatingCompleted?.GetInvocationList() ?? [])
                {
                    try { ((Action<HeatingPreparationCommit>)listener)(heatingCommit); }
                    catch (Exception exception) { GD.PrintErr($"Heating presentation failed: {exception.Message}"); }
                }
            }
            if (advanced && RemedySteepingPresentationProjection.IsCommit(activeRecipe, step, activity, preparation.Activity)
                && RemedySteepingPresentationProjection.Local(ToNumerics(seeker.GlobalPosition), seekerPosition, currentBinding.StationPosition))
            {
                steepingCommit = new(this, currentBinding, activity, preparation.Activity!, step, seeker.GlobalPosition);
                foreach (Delegate listener in SteepingCompleted?.GetInvocationList() ?? [])
                {
                    try { ((Action<SteepingPreparationCommit>)listener)(steepingCommit); }
                    catch (Exception exception) { GD.PrintErr($"Steeping presentation failed: {exception.Message}"); }
                }
            }
            if (advanced && RemedyCoolingPresentationProjection.IsCommit(activeRecipe, step, activity, preparation.Activity)
                && RemedyCoolingPresentationProjection.Local(ToNumerics(seeker.GlobalPosition), seekerPosition, currentBinding.StationPosition))
            {
                coolingCommit = new(this, currentBinding, activity, preparation.Activity!, step, seeker.GlobalPosition);
                foreach (Delegate listener in CoolingCompleted?.GetInvocationList() ?? [])
                {
                    try { ((Action<CoolingPreparationCommit>)listener)(coolingCommit); }
                    catch (Exception exception) { GD.PrintErr($"Cooling presentation failed: {exception.Message}"); }
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

        // The remedy's moonwort is used up only when its batch is made, so an unfinished
        // preparation (which a load discards) never loses the herb.
        bool remedy = ReferenceEquals(outcome.Activity.Recipe, requestedRecipe);
        if (remedy && !inventory.Carries(SeekerId, InventoryController.MoonwortLeaves))
        {
            return false;
        }

        if (!state.TryAddPreparedBatch(requestedContext, outcome))
        {
            return false;
        }

        return !remedy || inventory.TryUse(SeekerId, InventoryController.MoonwortLeaves);
    }

    private bool TryIdleAction(FirstPatientTreatmentContext context, IngredientPreparationRecipe remedy)
    {
        // No new preparation once every authored dose has been given; it could never be used.
        WorldEventLogSnapshot? log = GetNode<WorldEventController>("../WorldEventController").EventLog;
        if (log is null || GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource")
                .FirstPatientRemedyDoseWorldEvents.All(dose => log.Entries.Contains(dose.EventId)))
        {
            return false;
        }

        bool healerNear = healer.Visible
            && healer.GlobalPosition.DistanceTo(seeker.GlobalPosition) <= MaximumHandOverDistancePixels
            && inventory.Carries(HealerId, InventoryController.MoonwortLeaves);
        // shortcut: moonwort is the scenario's only gathered herb and the look-alike its only
        // alternative; a second herb needs an object-to-ingredient catalog.
        return RemedyStationPolicy.Choose(
                inventory.Carries(SeekerId, InventoryController.MoonwortLeaves),
                healerNear) switch
        {
            RemedyStationAction.PrepareRemedy => context.Scenario.GatheredIngredients.States.Count == 1
                && preparation.TryStart(remedy, context.Scenario.GatheredIngredients.States[0]),
            RemedyStationAction.ReceiveFromHealer =>
                inventory.TryGive(HealerId, SeekerId, InventoryController.MoonwortLeaves),
            _ => context.Scenario.AlternativeRecipes.Count > 0
                && preparation.TryStart(context.Scenario.AlternativeRecipes[0]),
        };
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

        if (outcome is not null
            && state.AdministrationExperiments?.Records.Any(record =>
                ReferenceEquals(record.Administration.Dosage.Preparation, outcome)) == true)
        {
            return FailedDoseStatusText;
        }

        if (outcome is not null)
        {
            return CompletedStatusText;
        }

        IngredientPreparationActivity? activity = preparation.Activity;
        if (activity is null)
        {
            return inventory.Carries(SeekerId, InventoryController.MoonwortLeaves)
                ? CarryingStatusText
                : ReadyStatusText;
        }

        int completedStepCount = activity.Progress.NextStepIndex;
        return completedStepCount == 0
            ? ReferenceEquals(activity.Recipe, recipe) ? StartedStatusText : LookAlikeStartedStatusText
            : StepStatusText(completedStepCount, activity.Recipe.Plan.Steps.Count);
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
