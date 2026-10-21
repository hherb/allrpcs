using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the mapped fixed sage consultation recovery path when enabled.</summary>
public partial class OpeningRegionSageConsultationInputSmokeCheck : Node
{
    private const string AdministrationAction = "remedy_administer";
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_SAGE_CONSULTATION_INPUT_CHECK";
    private const string ConsultationAction = "sage_consult";
    private const string SageLabelText = "VILLAGE SAGE · K";
    private const string EnabledEnvironmentValue = "1";
    private const string PreparationAction = "remedy_prepare";
    private static readonly string[] ExpectedCoordinatorMethods =
        ["TryConsult", "_Ready", "_UnhandledInput"];

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
            ArrangeTestimony();
            stage = 1;
            return;
        }

        SetProcess(false);
        VerifyConsultation();
    }

    private void ArrangeTestimony()
    {
        OpeningRegionRemedyPreparationSceneBinding preparationBinding =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource").Binding!;
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        // The remedy needs gathered moonwort; without it the station prepares the look-alike.
        _ = GetNode<InventoryController>("../InventoryController").TryTake(
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id,
            InventoryController.MoonwortLeaves);
        seeker.GlobalPosition = ToGodot(preparationBinding.StationPosition);
        OpeningRegionRemedyPreparationInputCoordinator preparation =
            GetNode<OpeningRegionRemedyPreparationInputCoordinator>(
                "../OpeningRegionRemedyPreparationInputCoordinator");
        int operationCount = preparationBinding.Recipe.Plan.Steps.Count + 3;
        for (int index = 0; index < operationCount; index++)
        {
            preparation._UnhandledInput(Action(PreparationAction, pressed: true));
        }

        OpeningRegionFirstPatientSceneBinding patientBinding =
            GetNode<OpeningRegionFirstPatientSceneSource>(
                "../OpeningRegionFirstPatientSceneSource").Binding!;
        seeker.GlobalPosition = ToGodot(patientBinding.PatientPosition);
        GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>(
            "../OpeningRegionFirstPatientAdministrationInputCoordinator")
            ._UnhandledInput(Action(AdministrationAction, pressed: true));
    }

    private void VerifyConsultation()
    {
        OpeningRegionSageConsultationInputCoordinator coordinator =
            GetNode<OpeningRegionSageConsultationInputCoordinator>(
                "../OpeningRegionSageConsultationInputCoordinator");
        OpeningRegionFirstPatientStabilizationTestimonyCoordinator testimonyCoordinator =
            GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
                "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        DialogueEvent testimony = testimonyCoordinator.Testimony!;
        FirstPatientStateController state =
            GetNode<FirstPatientStateController>("../FirstPatientStateController");
        FirstPatientTreatmentContext context = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource").Binding!.TreatmentContext;
        TemporaryOrdinaryCharacterParticipantSource participants =
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "../TemporaryOrdinaryCharacterParticipantSource");
        Character sage = participants.Participants[1];
        TemporarySeekerCharacterSource seekerSource =
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        Marker2D sageMarker = GetNode<Marker2D>("../SageHearingPosition");
        Polygon2D sageBody = GetNode<Polygon2D>("../SageHearingPosition/Body");
        Label sageLabel = GetNode<Label>("../SageHearingPosition/Label");
        Label status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        RetainedUnrelatedState unrelated = CaptureUnrelatedState();
        DialogueEvent? previousDialogue = GetNode<DialogueEventController>(
            "../DialogueEventController").LastOccurredEvent;
        Character rebuiltSage = RepresentativeSageCharacterSource.Create();
        DialogueEvent rebuiltTestimony = new(testimony.Delivery, testimony.Audience);

        if (!sageBody.Visible
            || !sageLabel.Visible
            || sageLabel.Text != SageLabelText
            || sageMarker.Position != new Vector2(2240.0f, 950.0f))
        {
            Fail("Sage consultation target was not visibly identified at the village square.");
            return;
        }

        seeker.GlobalPosition = sageMarker.Position + new Vector2(65.0f, 0.0f);
        coordinator._UnhandledInput(Action(ConsultationAction, pressed: false));
        coordinator._UnhandledInput(Action("consultation_unrelated", pressed: true));
        coordinator._UnhandledInput(Action(ConsultationAction, pressed: true));
        bool nullRejected = ThrowsArgumentNull(() => coordinator.TryConsult(
            null!,
            context,
            testimony,
            seekerSource.Character.Identity.Id,
            state.DiseaseTruth!,
            state.AdministrationExperiments!,
            ToNumerics(sageMarker.Position),
            ToNumerics(sageMarker.Position)));
        if (!nullRejected
            || coordinator.TryConsult(
                rebuiltSage,
                context,
                testimony,
                seekerSource.Character.Identity.Id,
                state.DiseaseTruth!,
                state.AdministrationExperiments!,
                ToNumerics(sageMarker.Position),
                ToNumerics(sageMarker.Position))
            || coordinator.TryConsult(
                sage,
                context,
                rebuiltTestimony,
                seekerSource.Character.Identity.Id,
                state.DiseaseTruth!,
                state.AdministrationExperiments!,
                ToNumerics(sageMarker.Position),
                ToNumerics(sageMarker.Position))
            || coordinator.Consultation is not null
            || !ReferenceEquals(
                GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
                previousDialogue)
            || status.Text != OpeningRegionSageConsultationInputCoordinator.MoveCloserStatusText
            || !HasClosedSurface())
        {
            Fail("Consultation accepted far, released, unrelated, rebuilt, or null provenance.");
            return;
        }

        seeker.GlobalPosition = sageMarker.Position;
        coordinator._UnhandledInput(Action(ConsultationAction, pressed: true));
        DialogueEvent consultation = coordinator.Consultation!;
        if (consultation is null
            || consultation.Utterance.Text != OpeningRegionSageConsultationProjection.ConsultationText
            || !ReferenceEquals(consultation.Utterance.SpeakerCharacterId, sage.Identity.Id)
            || !ReferenceEquals(consultation.Delivery.Mode, DialogueDeliveryMode.OrdinarySpeech)
            || consultation.Audience.ListenerCharacterIds.Count != 1
            || !ReferenceEquals(
                consultation.Audience.ListenerCharacterIds[0],
                seekerSource.Character.Identity.Id)
            || !ReferenceEquals(
                GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
                consultation)
            || status.Text != OpeningRegionSageConsultationInputCoordinator.ConsultedStatusText)
        {
            Fail("Exact mapped sage consultation did not occur as fixed heard guidance.");
            return;
        }

        coordinator._UnhandledInput(Action(ConsultationAction, pressed: true));
        if (!ReferenceEquals(coordinator.Consultation, consultation)
            || !ReferenceEquals(
                GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
                consultation)
            || !HasUnrelatedState(unrelated))
        {
            Fail("Repeated consultation changed dialogue or unrelated state.");
            return;
        }

        RepresentativeJournalCompositionCoordinator journalComposition =
            GetNode<RepresentativeJournalCompositionCoordinator>(
                "../RepresentativeJournalCompositionCoordinator");
        if (!journalComposition.TryComposeAndInitialize()
            || GetNode<JournalController>("../JournalController").Journal?.Testimony?.Entries
                is not { Count: 2 } entries
            || entries[0].Text
                != FirstPatientStabilizationTestimonyProjection.TestimonyText
            || entries[1].Text != OpeningRegionSageConsultationProjection.ConsultationText)
        {
            Fail("Journal composition did not retain patient and sage testimony in order.");
            return;
        }

        GD.Print(
            "Opening-region sage consultation input smoke passed: inert gating, K mapping, exact "
            + "stabilization and heard-testimony provenance, fixed provider-neutral recovery "
            + "guidance, visible proximity and completion status, ordered safe journal testimony, "
            + "one-shot Seeker audience, rejection paths, and isolation were correct.");
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
        Type type = typeof(OpeningRegionSageConsultationInputCoordinator);
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
            && property.Name == "Consultation"
            && property.PropertyType == typeof(DialogueEvent)
            && property.SetMethod?.IsPublic != true;
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

    private static T AssertSingle<T>(IReadOnlyList<T> values) =>
        values.Count == 1
            ? values[0]
            : throw new InvalidOperationException("Expected one closed-surface member.");

    private static Vector2 ToGodot(System.Numerics.Vector2 position) =>
        new(position.X, position.Y);

    private static System.Numerics.Vector2 ToNumerics(Vector2 position) =>
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
