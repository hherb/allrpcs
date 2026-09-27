using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Occurs one fixed heard testimony after exact first-patient stabilization.</summary>
public partial class OpeningRegionFirstPatientStabilizationTestimonyCoordinator : Node
{
    private const float OrdinaryHearingRangePixels = 64.0f;
    private const float WhisperHearingRangePixels = 24.0f;

    private DialogueEventController dialogue = null!;
    private OpeningRegionFirstPatientSceneSource scene = null!;
    private CharacterBody2D seekerBody = null!;
    private TemporarySeekerCharacterSource seekerSource = null!;
    private FirstPatientStateController state = null!;
    private TemporaryWorldEventSource worldEventSource = null!;

    /// <summary>Gets the exact one-shot testimony occurrence, if completed.</summary>
    public DialogueEvent? Testimony { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogue = GetNode<DialogueEventController>("../DialogueEventController");
        scene = GetNode<OpeningRegionFirstPatientSceneSource>(
            "../OpeningRegionFirstPatientSceneSource");
        seekerBody = GetNode<CharacterBody2D>("../Seeker");
        seekerSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        state = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        OpeningRegionFirstPatientSceneBinding? binding = scene.Binding;
        DiseaseTruthSnapshot? diseaseTruth = state.DiseaseTruth;
        IngredientExperimentJournalSnapshot? administrationExperiments =
            state.AdministrationExperiments;
        if (Testimony is null
            && binding is not null
            && diseaseTruth is not null
            && administrationExperiments is not null)
        {
            _ = TryPresent(
                binding,
                binding.TreatmentContext,
                diseaseTruth,
                administrationExperiments,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent,
                seekerSource.Character.Identity.Id,
                binding.PatientPosition,
                ToNumerics(seekerBody.GlobalPosition));
        }
    }

    /// <summary>Occurs fixed testimony only from exact current stabilized scene provenance.</summary>
    public bool TryPresent(
        OpeningRegionFirstPatientSceneBinding requestedBinding,
        FirstPatientTreatmentContext requestedContext,
        DiseaseTruthSnapshot requestedDiseaseTruth,
        IngredientExperimentJournalSnapshot requestedAdministrationExperiments,
        WorldEvent requestedAdministrationWorldEvent,
        CharacterId requestedSeekerId,
        NumericsVector2 requestedPatientPosition,
        NumericsVector2 requestedSeekerPosition)
    {
        ArgumentNullException.ThrowIfNull(requestedBinding);
        ArgumentNullException.ThrowIfNull(requestedContext);
        ArgumentNullException.ThrowIfNull(requestedDiseaseTruth);
        ArgumentNullException.ThrowIfNull(requestedAdministrationExperiments);
        ArgumentNullException.ThrowIfNull(requestedAdministrationWorldEvent);
        ArgumentNullException.ThrowIfNull(requestedSeekerId);

        OpeningRegionFirstPatientSceneBinding? currentBinding = scene.Binding;
        if (Testimony is not null
            || currentBinding is null
            || !ReferenceEquals(requestedBinding, currentBinding)
            || !ReferenceEquals(requestedContext, currentBinding.TreatmentContext)
            || !ReferenceEquals(requestedDiseaseTruth, state.DiseaseTruth)
            || !ReferenceEquals(
                requestedAdministrationExperiments,
                state.AdministrationExperiments)
            || !ReferenceEquals(
                requestedAdministrationWorldEvent,
                worldEventSource.FirstPatientRemedyAdministeredWorldEvent)
            || !ReferenceEquals(requestedSeekerId, seekerSource.Character.Identity.Id)
            || requestedPatientPosition != currentBinding.PatientPosition
            || requestedSeekerPosition != ToNumerics(seekerBody.GlobalPosition)
            || NumericsVector2.Distance(requestedPatientPosition, requestedSeekerPosition)
                > OrdinaryHearingRangePixels)
        {
            return false;
        }

        try
        {
            DialogueDelivery delivery = FirstPatientStabilizationTestimonyProjection.Project(
                requestedContext,
                requestedDiseaseTruth,
                requestedAdministrationExperiments,
                requestedAdministrationWorldEvent);
            if (!dialogue.TryOccur(
                    delivery,
                    requestedPatientPosition,
                    OrdinaryHearingRangePixels,
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

            Testimony = occurred;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);

    internal void InstallRestored(DialogueEvent? testimony)
    {
        Testimony = testimony;
    }
}
