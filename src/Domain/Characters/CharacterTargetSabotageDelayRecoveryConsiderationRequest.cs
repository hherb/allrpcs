using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded state for one provider-neutral delay-recovery choice.</summary>
public sealed record CharacterTargetSabotageDelayRecoveryConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying recovery.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest(
        Character actor,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        IEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterTargetSabotageDelayRecoveryCandidate> retainedCandidates = [];
        HashSet<CharacterTargetSabotageDelayRecoveryCandidate> distinctCandidates = new(
            ReferenceEqualityComparer.Instance);

        foreach (CharacterTargetSabotageDelayRecoveryCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Delay-recovery consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Delay-recovery consideration candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count
                > CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Delay-recovery consideration exceeded the bounded candidate count.");
            }
        }

        Actor = actor;
        WorldEvents = worldEvents;
        EventLog = eventLog;
        WorldTime = worldTime;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering target-delay recovery.</summary>
    public Character Actor { get; }

    /// <summary>Gets the exact authoritative world-event definitions.</summary>
    public WorldEventSnapshot WorldEvents { get; }

    /// <summary>Gets the exact current authoritative world-event log.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the exact current qualitative world-time state.</summary>
    public WorldTimeSnapshot WorldTime { get; }

    /// <summary>Gets stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterTargetSabotageDelayRecoveryCandidate> Candidates { get; }
}
