using System.Runtime.CompilerServices;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact authoritative target-delay world-state transition.</summary>
public partial class CharacterTargetSabotageDelayController : Node
{
    private readonly ConditionalWeakTable<
        CharacterTargetSabotageDelayConsiderationRequest,
        object> resolvedRequests = new();
    private readonly object resolvedRequestMarker = new();

    /// <summary>Gets the initialized exact current world-event log, if any.</summary>
    public WorldEventLogSnapshot? EventLog { get; private set; }

    /// <summary>Gets the initialized exact current qualitative world time, if any.</summary>
    public WorldTimeSnapshot? WorldTime { get; private set; }

    /// <summary>Gets the latest exact applied target-delay result, if any.</summary>
    public CharacterTargetSabotageDelayResult? LatestAppliedResult { get; private set; }

    /// <summary>Initializes this controller once from one exact current world-state pair.</summary>
    public bool TryInitialize(
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        if (EventLog is not null || WorldTime is not null)
        {
            return false;
        }

        EventLog = eventLog;
        WorldTime = worldTime;
        return true;
    }

    /// <summary>Resolves one exact accepted consideration result from current world state.</summary>
    public bool TryResolveConsideration(
        CharacterTargetSabotageDelayConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageDelayConsiderationRequest request =
            considerationResult.Request;
        if (resolvedRequests.TryGetValue(request, out _)
            || !ReferenceEquals(EventLog, request.EventLog)
            || !ReferenceEquals(WorldTime, request.WorldTime))
        {
            return false;
        }

        CharacterTargetSabotageDelayResult? result =
            CharacterTargetSabotageDelayConsiderationTransition.Apply(considerationResult);
        if (result is not null)
        {
            EventLog = result.EventLog;
            WorldTime = result.WorldTime;
            LatestAppliedResult = result;
        }

        resolvedRequests.Add(request, resolvedRequestMarker);
        return true;
    }
}
