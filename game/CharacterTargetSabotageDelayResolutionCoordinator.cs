using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Resolves and acknowledges each exact completed target-delay result once.</summary>
public partial class CharacterTargetSabotageDelayResolutionCoordinator : Node
{
    private CharacterTargetSabotageDelayConsiderationResult? observedResult;
    private WorldEventLogSnapshot? resolvedEventLog;
    private WorldTimeSnapshot? resolvedWorldTime;
    private CharacterTargetSabotageDelayResult? resolvedAppliedResult;
    private CharacterTargetSabotageDelayController state = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;

    internal CharacterTargetSabotageDelayConsiderationResult? AcknowledgedResult
    {
        get;
        private set;
    }

    internal CharacterTargetSabotageDelayResult? AcknowledgedAppliedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        _ = TryResolveCompletedResult();
    }

    /// <summary>Resolves one fresh exact completed result from current world state.</summary>
    public bool TryResolveCompletedResult()
    {
        CharacterTargetSabotageDelayConsiderationResult? result = task.Result;
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || result is null
            || ReferenceEquals(observedResult, result))
        {
            return false;
        }

        observedResult = result;
        resolvedEventLog = null;
        resolvedWorldTime = null;
        resolvedAppliedResult = null;
        if (!state.TryResolveConsideration(result))
        {
            return false;
        }

        resolvedEventLog = state.EventLog;
        resolvedWorldTime = state.WorldTime;
        if (resolvedEventLog is null || resolvedWorldTime is null)
        {
            throw new InvalidOperationException(
                "Successful target-delay resolution must retain an exact world-state pair.");
        }

        resolvedAppliedResult = state.LatestAppliedResult;
        return true;
    }

    /// <summary>Resets only the observed result whose exact resolved authority remains current.</summary>
    public bool TryAcknowledgeCompletedResult()
    {
        CharacterTargetSabotageDelayConsiderationResult? result = task.Result;
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Completed
            || result is null
            || !ReferenceEquals(observedResult, result)
            || resolvedEventLog is null
            || resolvedWorldTime is null
            || !ReferenceEquals(state.EventLog, resolvedEventLog)
            || !ReferenceEquals(state.WorldTime, resolvedWorldTime)
            || !ReferenceEquals(state.LatestAppliedResult, resolvedAppliedResult)
            || !task.TryReset())
        {
            return false;
        }

        AcknowledgedResult = result;
        AcknowledgedAppliedResult = resolvedAppliedResult;
        observedResult = null;
        resolvedEventLog = null;
        resolvedWorldTime = null;
        resolvedAppliedResult = null;
        return true;
    }
}
