using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents each exact completed model-selected move result at most once.</summary>
public partial class CharacterObjectSabotageMovePresentationCoordinator : Node
{
    private CharacterObjectSabotageMoveConsiderationResult? observedResult;
    private CharacterObjectSabotageMovePresentationController presentation = null!;
    private CharacterObjectSabotageMoveController state = null!;
    private CharacterObjectSabotageMoveTaskController task = null!;

    internal CharacterObjectSabotageMoveConsiderationResult? AcknowledgedResult
    {
        get;
        private set;
    }

    internal WorldObjectPlacementSnapshot? AcknowledgedPlacements { get; private set; }

    internal DiscoveryEvidenceSnapshot? AcknowledgedEvidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        task = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CharacterObjectSabotageMoveTaskController");
        presentation = GetNode<CharacterObjectSabotageMovePresentationController>(
            "../CharacterObjectSabotageMovePresentationController");
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
        CharacterObjectSabotageMoveConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        if (result.SelectedCandidate is null
            || state.Placements is not { } currentPlacements)
        {
            return false;
        }

        WorldObjectPlacementSnapshot expectedPlacements = result.Request.Placements;
        return presentation.TryPresent(expectedPlacements, currentPlacements);
    }

    /// <summary>Explicitly resets only the exact completed result already observed in current state.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterObjectSabotageMoveConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result))
        {
            return false;
        }

        bool isCurrent = result.SelectedCandidate is null
            ? ReferenceEquals(state.Placements, result.Request.Placements)
                && ReferenceEquals(state.Evidence, result.Request.Evidence)
            : ReferenceEquals(state.Placements, presentation.LastPresentedPlacements);
        if (!isCurrent || !task.TryReset())
        {
            return false;
        }

        AcknowledgedResult = result;
        AcknowledgedPlacements = state.Placements;
        AcknowledgedEvidence = state.Evidence;
        observedResult = null;
        return true;
    }
}
