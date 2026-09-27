using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Occurs one fixed provider-neutral sage consultation from mapped player input.</summary>
public partial class OpeningRegionSageConsultationInputCoordinator : Node
{
    internal const string TestimonyRequiredStatusText =
        "Consultation: hear the patient testimony first.";
    internal const string MoveCloserStatusText =
        "Consultation: move closer to the village sage.";
    internal const string ConsultedStatusText = "Sage consulted. Press Q to save.";

    private const float ConsultationRangePixels = 64.0f;
    private const float WhisperHearingRangePixels = 24.0f;
    private static readonly StringName ConsultationAction = new("sage_consult");

    private DialogueEventController dialogue = null!;
    private FirstPatientStateController firstPatientState = null!;
    private TemporaryOrdinaryCharacterParticipantSource participants = null!;
    private OpeningRegionFirstPatientSceneSource patientScene = null!;
    private Marker2D sagePosition = null!;
    private CharacterBody2D seekerBody = null!;
    private TemporarySeekerCharacterSource seekerSource = null!;
    private Label status = null!;
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator testimony = null!;

    /// <summary>Gets the exact one-shot sage consultation occurrence, if completed.</summary>
    public DialogueEvent? Consultation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogue = GetNode<DialogueEventController>("../DialogueEventController");
        firstPatientState = GetNode<FirstPatientStateController>(
            "../FirstPatientStateController");
        participants = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        patientScene = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource");
        sagePosition = GetNode<Marker2D>("../SageHearingPosition");
        seekerBody = GetNode<CharacterBody2D>("../Seeker");
        seekerSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        testimony = GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
            "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!@event.IsActionPressed(ConsultationAction))
        {
            return;
        }

        if (testimony.Testimony is not { } patientTestimony
            || firstPatientState.DiseaseTruth is not { } diseaseTruth
            || firstPatientState.AdministrationExperiments is not { } administrationExperiments)
        {
            status.Text = TestimonyRequiredStatusText;
            GetViewport().SetInputAsHandled();
            return;
        }

        Character sage = participants.Participants[1];
        CharacterId seekerId = seekerSource.Character.Identity.Id;
        FirstPatientTreatmentContext? context = patientScene.Binding?.TreatmentContext;
        if (context is null)
        {
            status.Text = TestimonyRequiredStatusText;
            GetViewport().SetInputAsHandled();
            return;
        }

        NumericsVector2 seekerPosition = ToNumerics(seekerBody.GlobalPosition);
        NumericsVector2 currentSagePosition = ToNumerics(sagePosition.Position);
        if (NumericsVector2.Distance(seekerPosition, currentSagePosition)
            > ConsultationRangePixels)
        {
            status.Text = MoveCloserStatusText;
        }
        else if (TryConsult(
                sage,
                context,
                patientTestimony,
                seekerId,
                diseaseTruth,
                administrationExperiments,
                seekerPosition,
                currentSagePosition))
        {
            status.Text = ConsultedStatusText;
        }

        GetViewport().SetInputAsHandled();
    }

    /// <summary>Occurs fixed guidance only from exact current stabilization and testimony.</summary>
    public bool TryConsult(
        Character requestedSage,
        FirstPatientTreatmentContext requestedContext,
        DialogueEvent requestedPatientTestimony,
        CharacterId requestedSeekerId,
        DiseaseTruthSnapshot requestedDiseaseTruth,
        IngredientExperimentJournalSnapshot requestedAdministrationExperiments,
        NumericsVector2 requestedSeekerPosition,
        NumericsVector2 requestedSagePosition)
    {
        ArgumentNullException.ThrowIfNull(requestedSage);
        ArgumentNullException.ThrowIfNull(requestedContext);
        ArgumentNullException.ThrowIfNull(requestedPatientTestimony);
        ArgumentNullException.ThrowIfNull(requestedSeekerId);
        ArgumentNullException.ThrowIfNull(requestedDiseaseTruth);
        ArgumentNullException.ThrowIfNull(requestedAdministrationExperiments);

        Character currentSage = participants.Participants[1];
        FirstPatientTreatmentContext? currentContext = patientScene.Binding?.TreatmentContext;
        if (Consultation is not null
            || currentContext is null
            || !ReferenceEquals(requestedSage, currentSage)
            || !ReferenceEquals(requestedContext, currentContext)
            || !ReferenceEquals(requestedPatientTestimony, testimony.Testimony)
            || !ReferenceEquals(requestedSeekerId, seekerSource.Character.Identity.Id)
            || !ReferenceEquals(requestedDiseaseTruth, firstPatientState.DiseaseTruth)
            || !ReferenceEquals(
                requestedAdministrationExperiments,
                firstPatientState.AdministrationExperiments)
            || requestedSeekerPosition != ToNumerics(seekerBody.GlobalPosition)
            || requestedSagePosition != ToNumerics(sagePosition.Position)
            || NumericsVector2.Distance(requestedSeekerPosition, requestedSagePosition)
                > ConsultationRangePixels)
        {
            return false;
        }

        CharacterDiseaseState? infection = requestedDiseaseTruth.Infections.SingleOrDefault(
            candidate => ReferenceEquals(candidate.CharacterId, requestedContext.Scenario.PatientId)
                && ReferenceEquals(candidate.Disease, requestedContext.Scenario.Remedy.Disease));
        IngredientAdministrationExperimentRecord? latestAdministration =
            requestedAdministrationExperiments.Records.Count == 0
                ? null
                : requestedAdministrationExperiments.Records[^1];
        if (infection is null
            || !ReferenceEquals(infection.CareState, DiseaseCareState.Stabilized)
            || latestAdministration is null
            || !ReferenceEquals(
                latestAdministration.Administration.PatientId,
                requestedContext.Scenario.PatientId)
            || !ReferenceEquals(
                latestAdministration.Administration.Dosage.Preparation.Activity.Recipe,
                requestedContext.Scenario.Remedy.Recipe))
        {
            return false;
        }

        try
        {
            DialogueDelivery delivery = OpeningRegionSageConsultationProjection.Project(
                requestedSage,
                requestedContext,
                requestedPatientTestimony,
                requestedSeekerId);
            if (!dialogue.TryOccur(
                    delivery,
                    requestedSagePosition,
                    ConsultationRangePixels,
                    WhisperHearingRangePixels,
                    [new DialogueHearingCandidate(requestedSeekerId, requestedSeekerPosition)]))
            {
                return false;
            }

            DialogueEvent? occurred = dialogue.LastOccurredEvent;
            if (occurred is null
                || !ReferenceEquals(occurred.Delivery, delivery)
                || occurred.Audience.ListenerCharacterIds.Count != 1
                || !ReferenceEquals(
                    occurred.Audience.ListenerCharacterIds[0],
                    requestedSeekerId))
            {
                return false;
            }

            Consultation = occurred;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);

    internal void InstallRestored(DialogueEvent? consultation)
    {
        Consultation = consultation;
        if (consultation is not null)
        {
            status.Text = ConsultedStatusText;
        }
    }
}
