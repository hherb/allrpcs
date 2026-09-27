using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks mapped first-patient administration and stabilization when enabled.</summary>
public partial class OpeningRegionFirstPatientAdministrationInputSmokeCheck : Node
{
    private const string AdministrationAction = "remedy_administer";
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_FIRST_PATIENT_ADMINISTRATION_INPUT_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PreparationAction = "remedy_prepare";
    private static readonly string[] ExpectedCoordinatorMethods =
        ["TryAdminister", "_Ready", "_UnhandledInput"];

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
        OpeningRegionFirstPatientAdministrationInputCoordinator coordinator =
            GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>(
                "../OpeningRegionFirstPatientAdministrationInputCoordinator");
        OpeningRegionRemedyPreparationInputCoordinator preparationInput =
            GetNode<OpeningRegionRemedyPreparationInputCoordinator>(
                "../OpeningRegionRemedyPreparationInputCoordinator");
        OpeningRegionRemedyPreparationSceneBinding preparationBinding =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource").Binding!;
        OpeningRegionFirstPatientSceneBinding patientBinding =
            GetNode<OpeningRegionFirstPatientSceneSource>(
                "../OpeningRegionFirstPatientSceneSource").Binding!;
        FirstPatientPreparationController preparation =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        FirstPatientStateController state =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        WorldEventController worldEvents =
            GetNode<WorldEventController>("../WorldEventController");
        TemporaryWorldEventSource eventSource =
            GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        Label status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        RetainedUnrelatedState unrelated = CaptureUnrelatedState();

        if (!HasMappedInput()
            || !HasClosedSurface()
            || state.PreparedBatches!.Batches.Count != 0
            || state.DiseaseTruth!.Infections.Single().CareState != DiseaseCareState.Unassisted)
        {
            Fail("Ordinary administration was not inert, mapped, and closed at startup.");
            return;
        }

        seeker.GlobalPosition = ToGodot(preparationBinding.StationPosition);
        int operationCount = preparationBinding.Recipe.Plan.Steps.Count + 3;
        for (int index = 0; index < operationCount; index++)
        {
            preparationInput._UnhandledInput(Action(PreparationAction, pressed: true));
        }

        PreparedIngredientBatch batch = state.PreparedBatches!.Batches.Single();
        WorldEvent administrationEvent = eventSource.FirstPatientRemedyAdministeredWorldEvent;
        FirstPatientTreatmentContext context = patientBinding.TreatmentContext;
        WorldEventLogSnapshot initialEventLog = worldEvents.EventLog!;
        WorldTimeSnapshot initialWorldTime = worldEvents.WorldTime!;
        IngredientExperimentJournalSnapshot initialAdministrationExperiments =
            state.AdministrationExperiments!;
        ExperimentJournalSnapshot initialExperiments = state.Experiments!;
        OpeningRegionFirstPatientSceneBinding rebuiltBinding = new(
            patientBinding.Projection,
            patientBinding.SickhouseAnchor,
            context,
            patientBinding.PatientPosition,
            patientBinding.MaximumPatientDistancePixels);
        FirstPatientTreatmentContext rebuiltContext =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        WorldEvent rebuiltEvent = new(
            administrationEvent.EventId,
            administrationEvent.TimeEffect);
        PreparedIngredientBatch rebuiltBatch = new(batch.Preparation);

        seeker.GlobalPosition = ToGodot(patientBinding.PatientPosition) + new Vector2(65.0f, 0.0f);
        coordinator._UnhandledInput(Action(AdministrationAction, pressed: false));
        coordinator._UnhandledInput(Action("administration_unrelated", pressed: true));
        coordinator._UnhandledInput(Action(AdministrationAction, pressed: true));
        bool nullRejected = ThrowsArgumentNull(() => coordinator.TryAdminister(
            null!,
            context,
            batch,
            administrationEvent,
            patientBinding.PatientPosition));
        if (!nullRejected
            || coordinator.TryAdminister(
                rebuiltBinding,
                context,
                batch,
                administrationEvent,
                patientBinding.PatientPosition)
            || coordinator.TryAdminister(
                patientBinding,
                rebuiltContext,
                batch,
                administrationEvent,
                patientBinding.PatientPosition)
            || coordinator.TryAdminister(
                patientBinding,
                context,
                rebuiltBatch,
                administrationEvent,
                patientBinding.PatientPosition)
            || coordinator.TryAdminister(
                patientBinding,
                context,
                batch,
                rebuiltEvent,
                patientBinding.PatientPosition)
            || !ReferenceEquals(worldEvents.EventLog, initialEventLog)
            || !ReferenceEquals(worldEvents.WorldTime, initialWorldTime)
            || status.Text
                != OpeningRegionFirstPatientAdministrationInputCoordinator.MoveCloserStatusText
            || state.DiseaseTruth!.Infections.Single().CareState != DiseaseCareState.Unassisted)
        {
            Fail("Far, released, unrelated, rebuilt, stale, wrong-context, or null input changed state.");
            return;
        }

        seeker.GlobalPosition = ToGodot(patientBinding.SickhouseAnchor.Position);
        coordinator._UnhandledInput(Action(AdministrationAction, pressed: true));
        WorldEventLogSnapshot resultingEventLog = worldEvents.EventLog!;
        WorldTimeSnapshot resultingWorldTime = worldEvents.WorldTime!;
        DiseaseTruthSnapshot resultingTruth = state.DiseaseTruth!;
        IngredientExperimentJournalSnapshot resultingAdministrationExperiments =
            state.AdministrationExperiments!;
        ExperimentJournalSnapshot resultingExperiments = state.Experiments!;
        IngredientAdministrationExperimentRecord administrationRecord =
            resultingAdministrationExperiments.Records.Single();
        ExperimentJournalRecord experimentRecord = resultingExperiments.Records.Single();
        if (resultingEventLog.Entries.Count != initialEventLog.Entries.Count + 1
            || !ReferenceEquals(resultingEventLog.Entries[^1], administrationEvent.EventId)
            || ReferenceEquals(resultingWorldTime, initialWorldTime)
            || !ReferenceEquals(resultingWorldTime.CurrentEventId, administrationEvent.EventId)
            || resultingTruth.Infections.Single().CareState != DiseaseCareState.Stabilized
            || state.PreparedBatches!.Batches.Count != 0
            || ReferenceEquals(
                resultingAdministrationExperiments,
                initialAdministrationExperiments)
            || ReferenceEquals(resultingExperiments, initialExperiments)
            || !ReferenceEquals(
                administrationRecord.Administration.Dosage.Preparation,
                batch.Preparation)
            || !ReferenceEquals(administrationRecord.Administration.WorldEvent, administrationEvent)
            || !ReferenceEquals(experimentRecord.WorldEvent, administrationEvent)
            || status.Text
                != OpeningRegionFirstPatientAdministrationInputCoordinator.AdministeredStatusText)
        {
            Fail("Mapped administration did not install the exact atomic stabilization result.");
            return;
        }

        coordinator._UnhandledInput(Action(AdministrationAction, pressed: true));
        if (!ReferenceEquals(worldEvents.EventLog, resultingEventLog)
            || !ReferenceEquals(worldEvents.WorldTime, resultingWorldTime)
            || !ReferenceEquals(state.DiseaseTruth, resultingTruth)
            || !ReferenceEquals(
                state.AdministrationExperiments,
                resultingAdministrationExperiments)
            || !ReferenceEquals(state.Experiments, resultingExperiments)
            || !ReferenceEquals(preparation.Outcome, batch.Preparation)
            || !HasUnrelatedState(unrelated))
        {
            Fail("Repeated administration changed stabilized or unrelated state.");
            return;
        }

        GD.Print(
            "Opening-region first-patient administration input smoke passed: inert startup, T "
            + "mapping, authored patient-approach range and provenance, visible proximity and "
            + "administration feedback, one full dose, atomic stabilization, world-event and "
            + "experiment recording, rejection paths, and isolation were correct.");
        GetTree().Quit();
    }

    private RetainedUnrelatedState CaptureUnrelatedState() => new(
        GetNode<JournalController>("../JournalController").Journal,
        GetNode<SeekerMapController>("../SeekerMapController").Map,
        GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController").Progress,
        GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
        GetNode<CharacterBody2D>("../Healer").GlobalPosition);

    private bool HasUnrelatedState(RetainedUnrelatedState retained) =>
        ReferenceEquals(GetNode<JournalController>("../JournalController").Journal, retained.Journal)
        && ReferenceEquals(GetNode<SeekerMapController>("../SeekerMapController").Map, retained.Map)
        && ReferenceEquals(
            GetNode<OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController").Progress,
            retained.RouteProgress)
        && ReferenceEquals(
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            retained.WorldObjects)
        && GetNode<CharacterBody2D>("../Healer").GlobalPosition == retained.HealerPosition;

    private static bool HasMappedInput()
    {
        StringName action = new(AdministrationAction);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey key
            && key.PhysicalKeycode == Key.T;
    }

    private static bool HasClosedSurface()
    {
        string[] methods = typeof(OpeningRegionFirstPatientAdministrationInputCoordinator)
            .GetMethods(
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
        WorldObjectSnapshot? WorldObjects,
        Vector2 HealerPosition);
}
