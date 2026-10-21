using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Administers the exact prepared first-patient remedy from one mapped input.</summary>
public partial class OpeningRegionFirstPatientAdministrationInputCoordinator : Node
{
    /// <summary>Presentation-only notification after the exact live administration commits.</summary>
    public event Action<OpeningRegionFirstPatientSceneBinding>? TreatmentCompleted;

    internal const string BatchRequiredStatusText =
        "Treatment: prepare the remedy batch first.";
    internal const string MoveCloserStatusText = "Treatment: move closer to the patient.";
    internal const string AdministeredStatusText =
        "Treatment: administered. Stay near the patient for testimony.";
    internal const string NoChangeStatusText =
        "Treatment: the patient's breathing does not change. Prepare another dose.";
    internal const string NoDoseLeftStatusText =
        "Treatment: the patient cannot take another dose.";

    private const float MaximumInteractionDistancePixels = 64.0f;
    private const string ObservedOutcome =
        "The patient's breathing becomes quieter and steadier.";
    private const string NoChangeOutcome = "The patient's breathing does not change.";
    private const string Procedure = "Administer the complete cooled remedy batch.";
    private static readonly ExperimentId[] DoseExperimentIds =
        [new("first-patient-remedy-administration"), new("first-patient-remedy-second-dose")];
    private static readonly StringName AdministrationAction = new("remedy_administer");

    private FirstPatientAdministrationController administration = null!;
    private TemporaryHealerCharacterSource healer = null!;
    private CharacterConditionSet observedConditions = null!;
    private CharacterConditionSet unchangedConditions = null!;
    private OpeningRegionFirstPatientSceneSource scene = null!;
    private CharacterBody2D seeker = null!;
    private FirstPatientStateController state = null!;
    private Label status = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private WorldEventController worldEventController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        administration = GetNode<FirstPatientAdministrationController>(
            "../FirstPatientAdministrationController");
        healer = GetNode<TemporaryHealerCharacterSource>(
            "../TemporaryHealerCharacterSource");
        observedConditions = new CharacterConditionSet(
            [new CharacterCondition("breathing", "Breathing becomes quieter and steadier.")]);
        unchangedConditions = new CharacterConditionSet(
            [new CharacterCondition("breathing", "Breathing is unchanged.")]);
        scene = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.WorldAllowed(this)) { return; }
        ArgumentNullException.ThrowIfNull(@event);

        if (!ControlInputMatcher.Pressed(@event, AdministrationAction))
        {
            return;
        }

        OpeningRegionFirstPatientSceneBinding? binding = scene.Binding;
        IReadOnlyList<PreparedIngredientBatch>? batches = state.PreparedBatches?.Batches;
        PreparedIngredientBatch? batch = batches is { Count: 1 } ? batches[0] : null;
        NumericsVector2 seekerPosition = ToNumerics(seeker.GlobalPosition);
        WorldEvent? dose = NextDose();
        if (batch is null)
        {
            ControlSettingsController.SetGuidance(status, AfterDoseStatusText() ?? BatchRequiredStatusText);
        }
        else if (dose is null)
        {
            ControlSettingsController.SetGuidance(status, NoDoseLeftStatusText);
        }
        else if (binding is not null
            && NumericsVector2.Distance(seekerPosition, binding.PatientPosition)
                > MaximumInteractionDistancePixels)
        {
            ControlSettingsController.SetGuidance(status, MoveCloserStatusText);
        }
        else if (binding is not null
            && TryAdminister(
                binding,
                binding.TreatmentContext,
                batch,
                dose,
                seekerPosition))
        {
            ControlSettingsController.SetGuidance(status, AfterDoseStatusText()!);
        }

        GetViewport().SetInputAsHandled();
    }

    /// <summary>Atomically administers and stabilizes from exact current authored state.</summary>
    public bool TryAdminister(
        OpeningRegionFirstPatientSceneBinding requestedBinding,
        FirstPatientTreatmentContext requestedContext,
        PreparedIngredientBatch requestedBatch,
        WorldEvent requestedWorldEvent,
        NumericsVector2 seekerPosition)
    {
        ArgumentNullException.ThrowIfNull(requestedBinding);
        ArgumentNullException.ThrowIfNull(requestedContext);
        ArgumentNullException.ThrowIfNull(requestedBatch);
        ArgumentNullException.ThrowIfNull(requestedWorldEvent);

        OpeningRegionFirstPatientSceneBinding? currentBinding = scene.Binding;
        PreparedIngredientBatchSnapshot? currentBatches = state.PreparedBatches;
        if (!float.IsFinite(seekerPosition.X)
            || !float.IsFinite(seekerPosition.Y)
            || currentBinding is null
            || currentBatches is null
            || !ReferenceEquals(requestedBinding, currentBinding)
            || !ReferenceEquals(requestedContext, currentBinding.TreatmentContext)
            || !ReferenceEquals(state.DiseaseTruth, requestedContext.Scenario.DiseaseTruth)
            || currentBatches.Batches.Count != 1
            || !ReferenceEquals(currentBatches.Batches[0], requestedBatch)
            || !ReferenceEquals(requestedWorldEvent, NextDose())
            || NumericsVector2.Distance(seekerPosition, currentBinding.PatientPosition)
                > MaximumInteractionDistancePixels)
        {
            return false;
        }

        IngredientQuantity dosage = new(
            requestedBatch.RemainingQuantity.Amount,
            requestedBatch.RemainingQuantity.Unit);
        // Only the remedy changes the patient; the engine records what a wrong dose did.
        bool remedy = ReferenceEquals(
            requestedBatch.Preparation.Activity.Recipe,
            requestedContext.Scenario.Remedy.Recipe);
        int doseIndex = worldEventSource.FirstPatientRemedyDoseWorldEvents.ToList().IndexOf(requestedWorldEvent);
        bool administered = administration.TryAdminister(
            DoseExperimentIds[doseIndex],
            requestedContext,
            healer.Character.Identity.Id,
            requestedWorldEvent,
            requestedBatch,
            dosage,
            remedy ? observedConditions : unchangedConditions,
            Procedure,
            remedy ? ObservedOutcome : NoChangeOutcome);
        if (administered)
        {
            foreach (Delegate listener in TreatmentCompleted?.GetInvocationList() ?? [])
            {
                try { ((Action<OpeningRegionFirstPatientSceneBinding>)listener)(currentBinding); }
                catch (Exception exception) { GD.PrintErr($"Treatment presentation failed: {exception.Message}"); }
            }
        }
        return administered;
    }

    // The next authored dose whose event has not occurred yet, or none.
    private WorldEvent? NextDose() => worldEventSource.FirstPatientRemedyDoseWorldEvents.FirstOrDefault(
        dose => worldEventController.EventLog?.Entries.Contains(dose.EventId) == false);

    private string? AfterDoseStatusText()
    {
        IReadOnlyList<IngredientAdministrationExperimentRecord>? records = state.AdministrationExperiments?.Records;
        if (records is not { Count: > 0 })
        {
            return null;
        }

        CharacterDiseaseState? infection = state.DiseaseTruth?.Infections.FirstOrDefault(
            candidate => candidate.CharacterId == records[^1].Administration.PatientId);
        return infection?.CareState == DiseaseCareState.Stabilized ? AdministeredStatusText
            : NextDose() is null ? NoDoseLeftStatusText
            : NoChangeStatusText;
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
