using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the ordinary mapped healer-garden preparation flow when enabled.</summary>
public partial class OpeningRegionRemedyPreparationInputSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_REMEDY_PREPARATION_INPUT_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PreparationAction = "remedy_prepare";
    private static readonly string[] ExpectedCoordinatorMethods =
        ["TryProgress", "_Ready", "_UnhandledInput"];

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
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        RunChecks();
    }

    private void RunChecks()
    {
        OpeningRegionRemedyPreparationInputCoordinator coordinator =
            GetNode<OpeningRegionRemedyPreparationInputCoordinator>(
                "../OpeningRegionRemedyPreparationInputCoordinator");
        OpeningRegionRemedyPreparationSceneBinding binding =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource").Binding!;
        FirstPatientPreparationController preparation =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        FirstPatientStateController state =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        Label status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        FirstPatientTreatmentContext context = binding.TreatmentContext;
        IngredientPreparationRecipe recipe = binding.Recipe;
        IngredientStateSnapshot initialIngredients = preparation.Ingredients!;
        FirstPatientSaveState initialState = state.Capture()!;
        RetainedUnrelatedState unrelated = CaptureUnrelatedState();

        if (preparation.Activity is not null
            || preparation.Outcome is not null
            || initialState.PreparedBatches.Batches.Count != 0
            || !HasMappedInput()
            || !HasClosedSurface(coordinator)
            || status.Text != OpeningRegionRemedyPreparationInputCoordinator.ReadyStatusText)
        {
            Fail("Ordinary remedy preparation was not inert, mapped, and closed at startup.");
            return;
        }

        seeker.GlobalPosition = ToGodot(binding.StationPosition) + new Vector2(49.0f, 0.0f);
        coordinator._UnhandledInput(Action(PreparationAction, pressed: false));
        coordinator._UnhandledInput(Action("remedy_unrelated", pressed: true));
        coordinator._UnhandledInput(Action(PreparationAction, pressed: true));
        FirstPatientTreatmentContext rebuiltContext =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        OpeningRegionRemedyPreparationSceneBinding rebuiltBinding = new(
            binding.Projection,
            binding.GardenAnchor,
            context,
            binding.StationPosition,
            binding.MaximumStationDistancePixels);
        bool nullRejected = ThrowsArgumentNull(() => coordinator.TryProgress(
            null!,
            context,
            recipe,
            initialIngredients,
            initialState,
            binding.StationPosition));
        if (!nullRejected
            || coordinator.TryProgress(
                rebuiltBinding,
                context,
                recipe,
                initialIngredients,
                initialState,
                binding.StationPosition)
            || coordinator.TryProgress(
                binding,
                rebuiltContext,
                recipe,
                initialIngredients,
                initialState,
                binding.StationPosition)
            || preparation.Activity is not null
            || !ReferenceEquals(preparation.Ingredients, initialIngredients)
            || status.Text
                != OpeningRegionRemedyPreparationInputCoordinator.MoveCloserStatusText
            || !HasUnrelatedState(unrelated))
        {
            Fail("Far, released, unrelated, rebuilt, wrong-context, or null input changed state.");
            return;
        }

        // The remedy needs gathered moonwort; without it the station prepares the look-alike.
        if (!GetNode<InventoryController>("../InventoryController").TryTake(
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id,
            InventoryController.MoonwortLeaves))
        {
            Fail("The Seeker could not be given gathered moonwort.");
            return;
        }

        seeker.GlobalPosition = ToGodot(binding.StationPosition);
        Press(coordinator);
        IngredientPreparationActivity started = preparation.Activity!;
        IngredientStateSnapshot remainingIngredients = preparation.Ingredients!;
        if (started is null
            || started.Progress.NextStepIndex != 0
            || ReferenceEquals(remainingIngredients, initialIngredients)
            || status.Text
                != OpeningRegionRemedyPreparationInputCoordinator.StartedStatusText
            || coordinator.TryProgress(
                binding,
                context,
                recipe,
                initialIngredients,
                initialState,
                binding.StationPosition))
        {
            Fail("Exact start or stale-ingredient rejection was incorrect.");
            return;
        }

        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            int expectedIndex = preparation.Activity!.Progress.NextStepIndex;
            Press(coordinator);
            if (preparation.Activity!.Progress.NextStepIndex != expectedIndex + 1
                || !ReferenceEquals(recipe.Plan.Steps[expectedIndex], step))
            {
                Fail("Mapped preparation did not advance exactly one authored next step.");
                return;
            }

            string expectedStatus =
                OpeningRegionRemedyPreparationInputCoordinator.StepStatusText(
                    expectedIndex + 1,
                    recipe.Plan.Steps.Count);
            if (status.Text != expectedStatus)
            {
                Fail("Mapped preparation did not present exact authored step progress.");
                return;
            }
        }

        Press(coordinator);
        IngredientPreparationOutcome outcome = preparation.Outcome!;
        if (outcome is null
            || !ReferenceEquals(outcome.Activity, preparation.Activity)
            || state.PreparedBatches!.Batches.Count != 0
            || status.Text
                != OpeningRegionRemedyPreparationInputCoordinator.CompletedStatusText)
        {
            Fail("Mapped preparation did not complete exactly once before installation.");
            return;
        }

        Press(coordinator);
        PreparedIngredientBatchSnapshot installed = state.PreparedBatches!;
        if (installed.Batches.Count != 1
            || !ReferenceEquals(installed.Batches[0].Preparation, outcome)
            || !ReferenceEquals(installed.Batches[0].RemainingQuantity, outcome.Quantity)
            || status.Text
                != OpeningRegionRemedyPreparationInputCoordinator.BatchReadyStatusText
            || GetNode<InventoryController>("../InventoryController").Ownerships!.Entries.Any(
                entry => entry.ObjectId == InventoryController.MoonwortLeaves))
        {
            Fail("Mapped preparation did not install one exact full prepared batch using the moonwort.");
            return;
        }

        Press(coordinator);
        if (!ReferenceEquals(state.PreparedBatches, installed)
            || !ReferenceEquals(state.DiseaseTruth, initialState.DiseaseTruth)
            || !ReferenceEquals(
                state.AdministrationExperiments,
                initialState.AdministrationExperiments)
            || !ReferenceEquals(state.Experiments, initialState.Experiments)
            || !ReferenceEquals(preparation.Ingredients, remainingIngredients)
            || !ReferenceEquals(preparation.Activity, outcome.Activity)
            || !ReferenceEquals(preparation.Outcome, outcome)
            || !HasUnrelatedState(unrelated))
        {
            Fail("Repeated preparation changed installed, patient, or unrelated state.");
            return;
        }

        GD.Print(
            "Opening-region remedy-preparation input smoke passed: inert startup, P mapping, "
            + "exact station range and provenance, one-step authored progression, full-batch "
            + "installation, visible proximity and progress feedback, rejection paths, and "
            + "unrelated-state isolation were correct.");
        GetTree().Quit();
    }

    private static void Press(OpeningRegionRemedyPreparationInputCoordinator coordinator) =>
        coordinator._UnhandledInput(Action(PreparationAction, pressed: true));

    private RetainedUnrelatedState CaptureUnrelatedState() => new(
        GetNode<JournalController>("../JournalController").Journal,
        GetNode<SeekerMapController>("../SeekerMapController").Map,
        GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController").Progress,
        GetNode<WorldEventController>("../WorldEventController").EventLog,
        GetNode<CharacterBody2D>("../Healer").GlobalPosition);

    private bool HasUnrelatedState(RetainedUnrelatedState retained) =>
        ReferenceEquals(GetNode<JournalController>("../JournalController").Journal, retained.Journal)
        && ReferenceEquals(GetNode<SeekerMapController>("../SeekerMapController").Map, retained.Map)
        && ReferenceEquals(
            GetNode<OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController").Progress,
            retained.RouteProgress)
        && ReferenceEquals(
            GetNode<WorldEventController>("../WorldEventController").EventLog,
            retained.WorldEventLog)
        && GetNode<CharacterBody2D>("../Healer").GlobalPosition == retained.HealerPosition;

    private static bool HasMappedInput()
    {
        StringName action = new(PreparationAction);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey key
            && key.PhysicalKeycode == Key.P;
    }

    private static bool HasClosedSurface(
        OpeningRegionRemedyPreparationInputCoordinator coordinator)
    {
        _ = coordinator;
        string[] methods = typeof(OpeningRegionRemedyPreparationInputCoordinator).GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(ExpectedCoordinatorMethods.Order(StringComparer.Ordinal));
    }

    private static InputEventAction Action(string action, bool pressed) => new()
    {
        Action = new StringName(action),
        Pressed = pressed,
    };

    private static bool ThrowsArgumentNull(System.Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private static Vector2 ToGodot(System.Numerics.Vector2 position) =>
        new(position.X, position.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed record RetainedUnrelatedState(
        JournalSnapshot? Journal,
        SeekerMapSnapshot? Map,
        object? RouteProgress,
        WorldEventLogSnapshot? WorldEventLog,
        Vector2 HealerPosition);
}
