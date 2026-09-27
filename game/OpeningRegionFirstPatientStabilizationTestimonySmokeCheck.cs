using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the exact one-shot heard testimony after stabilization when enabled.</summary>
public partial class OpeningRegionFirstPatientStabilizationTestimonySmokeCheck : Node
{
    private const string AdministrationAction = "remedy_administer";
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_FIRST_PATIENT_STABILIZATION_TESTIMONY_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PreparationAction = "remedy_prepare";
    private static readonly string[] ExpectedCoordinatorMethods =
        ["TryPresent", "_Process", "_Ready"];

    private DialogueEvent? previousDialogue;
    private RetainedUnrelatedState? unrelated;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            ArrangeStabilization();
            stage = 1;
            return;
        }

        SetProcess(false);
        VerifyTestimony();
    }

    private void ArrangeStabilization()
    {
        OpeningRegionFirstPatientStabilizationTestimonyCoordinator coordinator =
            GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
                "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        OpeningRegionFirstPatientSceneBinding patientBinding =
            GetNode<OpeningRegionFirstPatientSceneSource>(
                "../OpeningRegionFirstPatientSceneSource").Binding!;
        FirstPatientStateController state =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        TemporarySeekerCharacterSource seekerSource =
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        TemporaryWorldEventSource eventSource =
            GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        DialogueEventController dialogue =
            GetNode<DialogueEventController>("../DialogueEventController");
        unrelated = CaptureUnrelatedState();
        previousDialogue = dialogue.LastOccurredEvent;

        bool nullRejected = ThrowsArgumentNull(() => coordinator.TryPresent(
            null!,
            patientBinding.TreatmentContext,
            state.DiseaseTruth!,
            state.AdministrationExperiments!,
            eventSource.FirstPatientRemedyAdministeredWorldEvent,
            seekerSource.Character.Identity.Id,
            patientBinding.PatientPosition,
            patientBinding.PatientPosition));
        if (!nullRejected
            || coordinator.TryPresent(
                patientBinding,
                patientBinding.TreatmentContext,
                state.DiseaseTruth!,
                state.AdministrationExperiments!,
                eventSource.FirstPatientRemedyAdministeredWorldEvent,
                seekerSource.Character.Identity.Id,
                patientBinding.PatientPosition,
                patientBinding.PatientPosition)
            || coordinator.Testimony is not null
            || !ReferenceEquals(dialogue.LastOccurredEvent, previousDialogue)
            || !HasClosedSurface())
        {
            Fail("Testimony accepted null or unstabilized provenance, or exposed a broad surface.");
            return;
        }

        OpeningRegionRemedyPreparationSceneBinding preparationBinding =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource").Binding!;
        OpeningRegionRemedyPreparationInputCoordinator preparation =
            GetNode<OpeningRegionRemedyPreparationInputCoordinator>(
                "../OpeningRegionRemedyPreparationInputCoordinator");
        seeker.GlobalPosition = ToGodot(preparationBinding.StationPosition);
        int operationCount = preparationBinding.Recipe.Plan.Steps.Count + 3;
        for (int index = 0; index < operationCount; index++)
        {
            preparation._UnhandledInput(Action(PreparationAction));
        }

        seeker.GlobalPosition = ToGodot(patientBinding.PatientPosition);
        GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>(
            "../OpeningRegionFirstPatientAdministrationInputCoordinator")
            ._UnhandledInput(Action(AdministrationAction));
        if (state.DiseaseTruth!.Infections.Single().CareState != DiseaseCareState.Stabilized)
        {
            Fail("Testimony smoke could not arrange exact first-patient stabilization.");
        }
    }

    private void VerifyTestimony()
    {
        OpeningRegionFirstPatientStabilizationTestimonyCoordinator coordinator =
            GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
                "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        DialogueEvent? testimony = coordinator.Testimony;
        CharacterId seekerId = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource").Character.Identity.Id;
        FirstPatientTreatmentContext context = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource").Binding!.TreatmentContext;
        if (testimony is null
            || ReferenceEquals(testimony, previousDialogue)
            || testimony.Utterance.Text
                != FirstPatientStabilizationTestimonyProjection.TestimonyText
            || !ReferenceEquals(
                testimony.Utterance.SpeakerCharacterId,
                context.Scenario.PatientId)
            || !ReferenceEquals(testimony.Delivery.Mode, DialogueDeliveryMode.OrdinarySpeech)
            || testimony.Audience.ListenerCharacterIds.Count != 1
            || !ReferenceEquals(testimony.Audience.ListenerCharacterIds[0], seekerId)
            || !ReferenceEquals(
                GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
                testimony))
        {
            Fail("Exact stabilized patient testimony was not safely heard by the Seeker.");
            return;
        }

        coordinator._Process(0.0d);
        if (!ReferenceEquals(coordinator.Testimony, testimony)
            || !ReferenceEquals(
                GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
                testimony)
            || !HasUnrelatedState(unrelated!))
        {
            Fail("Repeated testimony processing changed dialogue or unrelated state.");
            return;
        }

        GD.Print(
            "Opening-region first-patient stabilization testimony smoke passed: unstabilized and "
            + "null rejection, exact administration provenance, fixed provider-neutral copy, "
            + "one-shot ordinary delivery, exact Seeker audience, and isolation were correct.");
        GetTree().Quit();
    }

    private RetainedUnrelatedState CaptureUnrelatedState() => new(
        GetNode<JournalController>("../JournalController").Journal,
        GetNode<SeekerMapController>("../SeekerMapController").Map,
        GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController").Progress,
        GetNode<WorldObjectController>("../WorldObjectController").WorldObjects);

    private bool HasUnrelatedState(RetainedUnrelatedState retained) =>
        ReferenceEquals(GetNode<JournalController>("../JournalController").Journal, retained.Journal)
        && ReferenceEquals(GetNode<SeekerMapController>("../SeekerMapController").Map, retained.Map)
        && ReferenceEquals(
            GetNode<OpeningRegionInvestigationRouteProgressController>(
                "../OpeningRegionInvestigationRouteProgressController").Progress,
            retained.RouteProgress)
        && ReferenceEquals(
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            retained.WorldObjects);

    private static bool HasClosedSurface()
    {
        Type type = typeof(OpeningRegionFirstPatientStabilizationTestimonyCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        return methods.SequenceEqual(ExpectedCoordinatorMethods.Order(StringComparer.Ordinal))
            && property.Name == "Testimony"
            && property.PropertyType == typeof(DialogueEvent)
            && property.SetMethod?.IsPublic != true;
    }

    private static InputEventAction Action(string action) => new()
    {
        Action = new StringName(action),
        Pressed = true,
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

    private static T AssertSingle<T>(IReadOnlyList<T> values) =>
        values.Count == 1
            ? values[0]
            : throw new InvalidOperationException("Expected one closed-surface member.");

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
        WorldObjectSnapshot? WorldObjects);
}
