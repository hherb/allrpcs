using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Gives the first patient's treatment stakes (slice step 8). Each frame it lets the illness
/// worsen once enough time-advancing events have passed, shows the worsened condition, and opens
/// the end panel when the slice is won or lost. The rules are Application code; this node only
/// reads the owning controllers and installs the result.
/// </summary>
public partial class FirstPatientStakesController : Node
{
    private FirstPatientStateController state = null!;
    private WorldEventController worldEvents = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private TemporaryFirstPatientTreatmentContextSource context = null!;
    private OpeningRegionSageConsultationInputCoordinator sageConsultation = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private SliceEndPanelController endPanel = null!;
    private Label status = null!;
    private FirstPatientSliceOutcome presented = FirstPatientSliceOutcome.InProgress;

    /// <summary>
    /// Off by default so proof scenes keep their authored, never-worsening patient. Main turns it
    /// on for a plain launch, and the stakes check turns it on explicitly.
    /// </summary>
    internal bool Active { get; set; }

    /// <summary>Gets the most recently evaluated slice outcome.</summary>
    internal FirstPatientSliceOutcome Outcome { get; private set; } = FirstPatientSliceOutcome.InProgress;

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        worldEvents = GetNode<WorldEventController>("../WorldEventController");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        context = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
        sageConsultation = GetNode<OpeningRegionSageConsultationInputCoordinator>(
            "../OpeningRegionSageConsultationInputCoordinator");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        endPanel = GetNode<SliceEndPanelController>("../SliceEndOverlay/Panel");
        status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        restoration.PresentationReset += OnRestored;
    }

    /// <inheritdoc />
    public override void _ExitTree() => restoration.PresentationReset -= OnRestored;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Active
            || state.DiseaseTruth is not { } truth
            || worldEvents.WorldEvents is not { } events
            || worldEvents.EventLog is not { } log
            || worldEvents.WorldTime is not { } time)
        {
            return;
        }

        CharacterId patientId = context.Context.Scenario.PatientId;
        DiseaseTruthSnapshot current = FirstPatientDiseaseProgressionSchedule.Apply(truth, patientId, events, log, time);
        if (!ReferenceEquals(current, truth))
        {
            state.InstallDiseaseTruth(current);
            CharacterDiseaseState infection = current.Infections.First(candidate => candidate.CharacterId == patientId);
            if (DiseaseConditionProjection.Project(infection, current) is { } condition)
            {
                ControlSettingsController.SetGuidance(status, FirstPatientStakesCopy.WorsenedStatus(condition));
            }
        }

        // Only "sage consulted" matters to the outcome, so the earlier recovery stages are not told apart.
        FirstPatientRecoveryStage recovery = sageConsultation.Consultation is null
            ? FirstPatientRecoveryStage.NotHeard
            : FirstPatientRecoveryStage.SageConsulted;
        Outcome = FirstPatientSliceOutcomePolicy.Evaluate(
            current,
            patientId,
            events,
            log,
            worldEventSource.FirstPatientRemedyDoseWorldEvents.Select(dose => dose.EventId),
            recovery);
        if (Outcome != presented)
        {
            presented = Outcome;
            if (Outcome != FirstPatientSliceOutcome.InProgress) { endPanel.Present(Outcome); }
        }
    }

    // A load replaces the whole state: close any ending and let the next frame present the loaded one.
    private void OnRestored()
    {
        endPanel.Close();
        presented = FirstPatientSliceOutcome.InProgress;
        Outcome = FirstPatientSliceOutcome.InProgress;
    }
}
