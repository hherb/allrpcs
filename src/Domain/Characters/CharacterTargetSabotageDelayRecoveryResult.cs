using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains one accepted delay recovery and its fresh authoritative world state.</summary>
public sealed record CharacterTargetSabotageDelayRecoveryResult
{
    /// <summary>Creates one immutable delay-recovery result.</summary>
    public CharacterTargetSabotageDelayRecoveryResult(
        CharacterTargetSabotageDelayRecoveryCandidate candidate,
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

    /// <summary>Gets the exact recovery candidate that was applied.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate Candidate { get; }

    /// <summary>Gets the fresh authoritative world-event log.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the fresh qualitative world-time state.</summary>
    public WorldTimeSnapshot WorldTime { get; }
}
