using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Resolves and acknowledges each exact completed influence result once.</summary>
public partial class CharacterTargetSabotageInfluenceResolutionCoordinator : Node
{
    private CharacterTargetSabotageInfluenceConsiderationResult? observedResult;
    private CharacterRelationshipMemory? resolvedRelationship;
    private CharacterTargetSabotageInfluenceResult? resolvedAppliedResult;
    private CharacterTargetSabotageInfluenceController state = null!;
    private CharacterTargetSabotageInfluenceTaskController task = null!;

    internal CharacterTargetSabotageInfluenceConsiderationResult? AcknowledgedResult
    {
        get;
        private set;
    }

    internal CharacterTargetSabotageInfluenceResult? AcknowledgedAppliedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        task = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
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
        CharacterTargetSabotageInfluenceConsiderationResult? result = task.Result;
        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
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
                "Successful influence resolution must retain an exact relationship.");
        }

        resolvedAppliedResult = state.LatestAppliedResult;
        return true;
    }

    /// <summary>Resets only the observed result whose exact resolved authority remains current.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterTargetSabotageInfluenceConsiderationResult? result = task.Result;
        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result)
            || resolvedRelationship is null
            || !ReferenceEquals(state.Relationship, resolvedRelationship)
            || !ReferenceEquals(state.LatestAppliedResult, resolvedAppliedResult)
            || !task.TryReset())
        {
            return false;
        }

        AcknowledgedResult = result;
        AcknowledgedAppliedResult = resolvedAppliedResult;
        observedResult = null;
        resolvedRelationship = null;
        resolvedAppliedResult = null;
        return true;
    }
}
