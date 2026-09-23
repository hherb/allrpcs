using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents each exact completed model-selected contamination result at most once.</summary>
public partial class CharacterObjectSabotageContaminationPresentationCoordinator : Node
{
    private CharacterObjectSabotageContaminationConsiderationResult? observedResult;
    private CharacterObjectSabotageContaminationPresentationController presentation = null!;
    private CharacterObjectSabotageContaminationController state = null!;
    private CharacterObjectSabotageContaminationTaskController task = null!;

    internal CharacterObjectSabotageContaminationConsiderationResult? AcknowledgedResult
    {
        get;
        private set;
    }

    internal WorldObjectContaminationSnapshot? AcknowledgedContamination { get; private set; }

    internal DiscoveryEvidenceSnapshot? AcknowledgedEvidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        task = GetNode<CharacterObjectSabotageContaminationTaskController>(
            "../CharacterObjectSabotageContaminationTaskController");
        presentation = GetNode<CharacterObjectSabotageContaminationPresentationController>(
            "../CharacterObjectSabotageContaminationPresentationController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = TryPresentCompletedResult();
    }

    /// <summary>Observes one fresh exact terminal selection and presents its state transition.</summary>
    public bool TryPresentCompletedResult()
    {
        CharacterObjectSabotageContaminationConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        if (result.SelectedCandidate is null
            || state.Contamination is not { } currentContamination)
        {
            return false;
        }

        WorldObjectContaminationSnapshot expectedContamination = result.Request.Contamination;
        return presentation.TryPresent(expectedContamination, currentContamination);
    }

    /// <summary>Explicitly resets only the exact completed result already observed in current state.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterObjectSabotageContaminationConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result))
        {
            return false;
        }

        bool isCurrent = result.SelectedCandidate is null
            ? ReferenceEquals(state.Contamination, result.Request.Contamination)
                && ReferenceEquals(state.Evidence, result.Request.Evidence)
            : ReferenceEquals(
                state.Contamination,
                presentation.LastPresentedContamination);
        if (!isCurrent || !task.TryReset())
        {
            return false;
        }

        AcknowledgedResult = result;
        AcknowledgedContamination = state.Contamination;
        AcknowledgedEvidence = state.Evidence;
        observedResult = null;
        return true;
    }
}
