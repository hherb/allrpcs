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

    private const float MaximumInteractionDistancePixels = 64.0f;
    private const string ObservedOutcome =
        "The patient's breathing becomes quieter and steadier.";
    private const string Procedure = "Administer the complete cooled remedy batch.";
    private static readonly StringName AdministrationAction = new("remedy_administer");

    private FirstPatientAdministrationController administration = null!;
    private ExperimentId experimentId = null!;
    private TemporaryHealerCharacterSource healer = null!;
    private CharacterConditionSet observedConditions = null!;
    private OpeningRegionFirstPatientSceneSource scene = null!;
    private CharacterBody2D seeker = null!;
    private FirstPatientStateController state = null!;
    private Label status = null!;
    private TemporaryWorldEventSource worldEventSource = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        administration = GetNode<FirstPatientAdministrationController>(
            "../FirstPatientAdministrationController");
        experimentId = new ExperimentId("first-patient-remedy-administration");
        healer = GetNode<TemporaryHealerCharacterSource>(
            "../TemporaryHealerCharacterSource");
        observedConditions = new CharacterConditionSet(
            [new CharacterCondition("breathing", "Breathing becomes quieter and steadier.")]);
        scene = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        status = GetNode<Label>("../CommandOverlay/Panel/RecoveryStatus");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!@event.IsActionPressed(AdministrationAction))
        {
            return;
        }

        OpeningRegionFirstPatientSceneBinding? binding = scene.Binding;
        IReadOnlyList<PreparedIngredientBatch>? batches = state.PreparedBatches?.Batches;
        PreparedIngredientBatch? batch = batches is { Count: 1 } ? batches[0] : null;
        NumericsVector2 seekerPosition = ToNumerics(seeker.GlobalPosition);
        if (batch is null)
        {
            status.Text = state.AdministrationExperiments?.Records.Count > 0
                ? AdministeredStatusText
                : BatchRequiredStatusText;
        }
        else if (binding is not null
            && NumericsVector2.Distance(seekerPosition, binding.PatientPosition)
                > MaximumInteractionDistancePixels)
        {
            status.Text = MoveCloserStatusText;
        }
        else if (binding is not null
            && TryAdminister(
                binding,
                binding.TreatmentContext,
                batch,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                seekerPosition))
        {
            status.Text = AdministeredStatusText;
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
            || !ReferenceEquals(
                requestedWorldEvent,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent)
            || NumericsVector2.Distance(seekerPosition, currentBinding.PatientPosition)
                > MaximumInteractionDistancePixels)
        {
            return false;
        }

        IngredientQuantity dosage = new(
            requestedBatch.RemainingQuantity.Amount,
            requestedBatch.RemainingQuantity.Unit);
        bool administered = administration.TryAdminister(
            experimentId,
            requestedContext,
            healer.Character.Identity.Id,
            requestedWorldEvent,
            requestedBatch,
            dosage,
            observedConditions,
            Procedure,
            ObservedOutcome);
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

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
