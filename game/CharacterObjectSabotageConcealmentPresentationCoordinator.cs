using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents each exact completed model-selected concealment result at most once.</summary>
public partial class CharacterObjectSabotageConcealmentPresentationCoordinator : Node
{
    private CharacterObjectSabotageConcealmentConsiderationResult? observedResult;
    private CharacterObjectSabotageConcealmentPresentationController presentation = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private CharacterObjectSabotageConcealmentTaskController task = null!;

    internal CharacterObjectSabotageConcealmentConsiderationResult? AcknowledgedResult
    {
        get;
        private set;
    }

    internal WorldObjectConcealmentSnapshot? AcknowledgedConcealment { get; private set; }

    internal DiscoveryEvidenceSnapshot? AcknowledgedEvidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        task = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        presentation = GetNode<CharacterObjectSabotageConcealmentPresentationController>(
            "../CharacterObjectSabotageConcealmentPresentationController");
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
        CharacterObjectSabotageConcealmentConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        if (result.SelectedCandidate is null
            || state.Concealment is not { } currentConcealment)
        {
            return false;
        }

        WorldObjectConcealmentSnapshot expectedConcealment = result.Request.Concealment;
        return presentation.TryPresent(expectedConcealment, currentConcealment);
    }

    /// <summary>Explicitly resets only the exact completed result already observed in current state.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterObjectSabotageConcealmentConsiderationResult? result = task.Result;
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result))
        {
            return false;
        }

        bool isCurrent = result.SelectedCandidate is null
            ? ReferenceEquals(state.Concealment, result.Request.Concealment)
                && ReferenceEquals(state.Evidence, result.Request.Evidence)
            : ReferenceEquals(state.Concealment, presentation.LastPresentedConcealment);
        if (!isCurrent || !task.TryReset())
        {
            return false;
        }

        AcknowledgedResult = result;
        AcknowledgedConcealment = state.Concealment;
        AcknowledgedEvidence = state.Evidence;
        observedResult = null;
        return true;
    }
}
