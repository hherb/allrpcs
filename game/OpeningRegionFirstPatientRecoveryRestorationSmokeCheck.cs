using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves save restoration at each ordinary first-patient recovery boundary.</summary>
public partial class OpeningRegionFirstPatientRecoveryRestorationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_FIRST_PATIENT_RECOVERY_RESTORATION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private SaveSnapshotCaptureController capture = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private FirstPatientPreparationController preparation = null!;
    private FirstPatientStateController firstPatient = null!;
    private OpeningRegionFirstPatientAdministrationInputCoordinator administration = null!;
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator testimony = null!;
    private OpeningRegionSageConsultationInputCoordinator consultation = null!;
    private TemporaryFirstPatientTreatmentContextSource contextSource = null!;
    private OpeningRegionFirstPatientSceneSource patientScene = null!;
    private TemporaryOrdinaryCharacterParticipantSource participants = null!;
    private TemporarySeekerCharacterSource seekerSource = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private SeekerMapController map = null!;
    private OpeningRegionOldShrineSceneSource shrine = null!;
    private OpeningRegionInvestigationRouteProgressController routes = null!;
    private CharacterBody2D seeker = null!;
    private Marker2D sagePosition = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool enabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
        restoration = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
        preparation = GetNode<FirstPatientPreparationController>(
            "../FirstPatientPreparationController");
        firstPatient = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        administration = GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>(
            "../OpeningRegionFirstPatientAdministrationInputCoordinator");
        testimony = GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
            "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        consultation = GetNode<OpeningRegionSageConsultationInputCoordinator>(
            "../OpeningRegionSageConsultationInputCoordinator");
        contextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
        patientScene = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource");
        participants = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        seekerSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        journalComposition = GetNode<RepresentativeJournalCompositionCoordinator>(
            "../RepresentativeJournalCompositionCoordinator");
        map = GetNode<SeekerMapController>("../SeekerMapController");
        shrine = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        routes = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        sagePosition = GetNode<Marker2D>("../SageHearingPosition");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        if (!journalComposition.TryComposeAndInitialize())
        {
            Fail("Recovery restoration smoke could not initialize the safe journal.");
            return;
        }

        CharacterId seekerId = seekerSource.Character.Identity.Id;
        MapSaveStateRestorationProjection.Restore(
            new MapSaveState(seekerId, [], [], [])).InstallInto(map);
        if (!SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map, shrine))
        {
            Fail("Recovery restoration smoke could not initialize route state.");
            return;
        }

        SaveSnapshot beforePreparation = capture.Capture()
            ?? throw new InvalidOperationException("Pre-preparation state was not capturable.");
        FirstPatientTreatmentContext context = contextSource.Context;
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        if (!preparation.TryStart(recipe))
        {
            Fail("Recovery restoration smoke could not start ordinary preparation.");
            return;
        }

        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            if (!preparation.TryAdvance(step))
            {
                Fail("Recovery restoration smoke could not advance ordinary preparation.");
                return;
            }
        }

        if (!preparation.TryComplete()
            || !firstPatient.TryAddPreparedBatch(context, preparation.Outcome!)
            || capture.Capture() is not { } batchSave)
        {
            Fail("Full prepared-batch state was not capturable.");
            return;
        }

        OpeningRegionFirstPatientSceneBinding binding = patientScene.Binding!;
        seeker.GlobalPosition = ToGodot(binding.PatientPosition);
        PreparedIngredientBatch batch = firstPatient.PreparedBatches!.Batches[0];
        if (!administration.TryAdminister(
                binding,
                binding.TreatmentContext,
                batch,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                binding.PatientPosition)
            || capture.Capture() is not { } stabilizedSave)
        {
            Fail("Post-stabilization state was not capturable.");
            return;
        }

        if (!testimony.TryPresent(
                binding,
                binding.TreatmentContext,
                firstPatient.DiseaseTruth!,
                firstPatient.AdministrationExperiments!,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                seekerId,
                binding.PatientPosition,
                binding.PatientPosition)
            || capture.Capture() is not { } testimonySave)
        {
            Fail("Post-testimony state was not capturable.");
            return;
        }

        Character sage = participants.Participants[1];
        seeker.GlobalPosition = sagePosition.GlobalPosition;
        NumericsVector2 sagePoint = ToNumerics(sagePosition.GlobalPosition);
        if (!consultation.TryConsult(
                sage,
                binding.TreatmentContext,
                testimony.Testimony!,
                seekerId,
                firstPatient.DiseaseTruth!,
                firstPatient.AdministrationExperiments!,
                sagePoint,
                sagePoint)
            || capture.Capture() is not { } consultedSave)
        {
            Fail("Post-consultation state was not capturable.");
            return;
        }

        if (testimonySave.Journal?.Testimony is not { Count: 1 } testimonyEntries
            || testimonyEntries[0].Text
                != FirstPatientStabilizationTestimonyProjection.TestimonyText
            || consultedSave.Journal?.Testimony is not { Count: 2 } consultedEntries
            || consultedEntries[0].Text
                != FirstPatientStabilizationTestimonyProjection.TestimonyText
            || consultedEntries[1].Text != OpeningRegionSageConsultationProjection.ConsultationText)
        {
            Fail("Live capture did not retain recovery dialogue as ordered safe journal copies.");
            return;
        }

        if (!restoration.TryRestore(beforePreparation)
            || preparation.Activity is not null
            || preparation.Outcome is not null
            || !preparation.TryStart(recipe))
        {
            Fail("Recovery restoration did not reset the pre-preparation continuation.");
            return;
        }

        if (!restoration.TryRestore(batchSave)
            || firstPatient.PreparedBatches?.Batches.Count != 1
            || !ReferenceEquals(
                preparation.Outcome,
                firstPatient.PreparedBatches.Batches[0].Preparation))
        {
            Fail("Recovery restoration did not reconstruct the full prepared batch boundary.");
            return;
        }

        seeker.GlobalPosition = ToGodot(binding.PatientPosition);
        PreparedIngredientBatch restoredBatch = firstPatient.PreparedBatches.Batches[0];
        if (!administration.TryAdminister(
                binding,
                binding.TreatmentContext,
                restoredBatch,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                binding.PatientPosition))
        {
            Fail("A restored full batch could not continue through administration.");
            return;
        }

        if (!restoration.TryRestore(stabilizedSave)
            || testimony.Testimony is not null
            || consultation.Consultation is not null
            || !testimony.TryPresent(
                binding,
                binding.TreatmentContext,
                firstPatient.DiseaseTruth!,
                firstPatient.AdministrationExperiments!,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                seekerId,
                binding.PatientPosition,
                binding.PatientPosition))
        {
            Fail("Post-stabilization restoration did not preserve the pending testimony.");
            return;
        }

        if (!restoration.TryRestore(testimonySave)
            || testimony.Testimony is null
            || consultation.Consultation is not null
            || testimony.TryPresent(
                binding,
                binding.TreatmentContext,
                firstPatient.DiseaseTruth!,
                firstPatient.AdministrationExperiments!,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                seekerId,
                binding.PatientPosition,
                binding.PatientPosition))
        {
            Fail("Post-testimony restoration replayed or lost the one-shot testimony.");
            return;
        }

        seeker.GlobalPosition = sagePosition.GlobalPosition;
        if (!consultation.TryConsult(
                sage,
                binding.TreatmentContext,
                testimony.Testimony,
                seekerId,
                firstPatient.DiseaseTruth!,
                firstPatient.AdministrationExperiments!,
                sagePoint,
                sagePoint))
        {
            Fail("A restored testimony could not continue through sage consultation.");
            return;
        }

        if (!restoration.TryRestore(consultedSave)
            || testimony.Testimony is null
            || consultation.Consultation is null
            || consultation.TryConsult(
                sage,
                binding.TreatmentContext,
                testimony.Testimony,
                seekerId,
                firstPatient.DiseaseTruth!,
                firstPatient.AdministrationExperiments!,
                sagePoint,
                sagePoint))
        {
            Fail("Post-consultation restoration replayed or lost completed recovery dialogue.");
            return;
        }

        string retainedSave = SaveSnapshotJsonSerializer.Serialize(capture.Capture()!);
        DialogueEvent completedTestimony = testimony.Testimony!;
        DialogueEvent completedConsultation = consultation.Consultation!;
        DialogueEvent? retainedDialogue = GetNode<DialogueEventController>(
            "../DialogueEventController").LastOccurredEvent;
        Vector2 retainedPosition = seeker.Position;
        Vector2 retainedVelocity = seeker.Velocity;
        JournalTestimonySaveEntry patientEntry = new(
            context.Scenario.PatientId,
            FirstPatientStabilizationTestimonyProjection.TestimonyText,
            DialogueDeliveryMode.OrdinarySpeech);
        SaveSnapshot unstabilizedHeard = WithRecovery(
            beforePreparation,
            beforePreparation.FirstPatient!,
            beforePreparation.WorldEvents,
            FirstPatientRecoveryStage.PatientTestimonyHeard,
            [patientEntry]);
        SaveSnapshot missingAdministrationOccurrence = WithRecovery(
            beforePreparation,
            stabilizedSave.FirstPatient!,
            beforePreparation.WorldEvents,
            FirstPatientRecoveryStage.PatientTestimonyHeard,
            [patientEntry]);
        if (restoration.TryRestore(unstabilizedHeard)
            || restoration.TryRestore(missingAdministrationOccurrence)
            || !string.Equals(
                SaveSnapshotJsonSerializer.Serialize(capture.Capture()!),
                retainedSave,
                StringComparison.Ordinal)
            || !ReferenceEquals(testimony.Testimony, completedTestimony)
            || !ReferenceEquals(consultation.Consultation, completedConsultation)
            || !ReferenceEquals(
                GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
                retainedDialogue)
            || seeker.Position != retainedPosition
            || seeker.Velocity != retainedVelocity)
        {
            Fail("Incompatible recovery progress changed persisted or unrelated scene state.");
            return;
        }

        GD.Print(
            "First-patient recovery restoration smoke passed: before preparation, full batch, "
            + "post-stabilization, post-testimony, and post-consultation saves resumed exactly "
            + "once; incompatible recovery progress stayed inert.");
        GetTree().Quit();
    }

    private static SaveSnapshot WithRecovery(
        SaveSnapshot baseline,
        FirstPatientSaveState firstPatient,
        WorldEventSaveState eventState,
        FirstPatientRecoveryStage stage,
        IReadOnlyList<JournalTestimonySaveEntry> testimonyEntries)
    {
        JournalSaveState journal = baseline.Journal!;
        return new SaveSnapshot(
            SaveSchemaVersion.FirstPatientRecovery,
            baseline.Discovery,
            eventState,
            baseline.CharacterMemory,
            firstPatient,
            new JournalSaveState(
                journal.SeekerId,
                journal.Observations,
                testimonyEntries,
                journal.Hypotheses,
                journal.Experiments,
                journal.OpenQuestions,
                journal.SelectedSectionKey),
            baseline.Map,
            baseline.InvestigationRoutes,
            stage);
    }

    private static Vector2 ToGodot(NumericsVector2 value) => new(value.X, value.Y);

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
