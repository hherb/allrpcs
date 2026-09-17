using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains one accepted delay candidate and its fresh authoritative world state.</summary>
public sealed record CharacterTargetSabotageDelayResult
{
    /// <summary>Creates one immutable delay result.</summary>
    public CharacterTargetSabotageDelayResult(
        CharacterTargetSabotageDelayCandidate candidate,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        Candidate = candidate;
        EventLog = eventLog;
        WorldTime = worldTime;
    }

    /// <summary>Gets the exact delay candidate that was applied.</summary>
    public CharacterTargetSabotageDelayCandidate Candidate { get; }

    /// <summary>Gets the fresh authoritative world-event log.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the fresh qualitative world-time state.</summary>
    public WorldTimeSnapshot WorldTime { get; }
}
