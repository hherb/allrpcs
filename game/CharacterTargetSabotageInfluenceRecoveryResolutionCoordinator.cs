using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Resolves and acknowledges each exact completed influence-recovery result once.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator : Node
{
    private CharacterTargetSabotageInfluenceRecoveryConsiderationResult? observedResult;
    private CharacterRelationshipMemory? resolvedRelationship;
    private CharacterTargetSabotageInfluenceRecoveryResult? resolvedAppliedResult;
    private CharacterTargetSabotageInfluenceRecoveryController state = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = TryResolveCompletedResult();
    }

    /// <summary>Resolves one fresh exact completed result from current relationship state.</summary>
    public bool TryResolveCompletedResult()
    {
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult? result = task.Result;
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        resolvedRelationship = null;
        resolvedAppliedResult = null;
        if (!state.TryResolveConsideration(result))
        {
            return false;
        }

        resolvedRelationship = state.Relationship;
        if (resolvedRelationship is null)
        {
            throw new InvalidOperationException(
                "Successful influence-recovery resolution must retain an exact relationship.");
        }

        resolvedAppliedResult = state.LatestAppliedResult;
        return true;
    }

    /// <summary>Resets only the observed result whose exact resolved authority remains current.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult? result = task.Result;
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result)
            || resolvedRelationship is null
            || !ReferenceEquals(state.Relationship, resolvedRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, resolvedAppliedResult)
            || !task.TryReset())
        {
            return false;
        }

        observedResult = null;
        resolvedRelationship = null;
        resolvedAppliedResult = null;
        return true;
    }
}
