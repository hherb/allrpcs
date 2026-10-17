using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded world state for one provider-neutral delay choice.</summary>
/// <remarks>Instances use reference identity; copied collections do not define value equality.</remarks>
public sealed class CharacterTargetSabotageDelayConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying a delay.</summary>
    public CharacterTargetSabotageDelayConsiderationRequest(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        IEnumerable<CharacterTargetSabotageDelayCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterTargetSabotageDelayCandidate> retainedCandidates = [];
        HashSet<CharacterTargetSabotageDelayCandidate> distinctCandidates = new(
            ReferenceEqualityComparer.Instance);

        foreach (CharacterTargetSabotageDelayCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Delay consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Delay consideration candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count
                > CharacterTargetSabotageDelaySelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Delay consideration exceeded the bounded candidate count.");
            }
        }

        Actor = actor;
        InteractionAccess = interactionAccess;
        WorldEvents = worldEvents;
        EventLog = eventLog;
        WorldTime = worldTime;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering a target delay.</summary>
    public Character Actor { get; }

    /// <summary>Gets the exact authoritative ordinary-interaction access snapshot.</summary>
    public CharacterInteractionAccessSnapshot InteractionAccess { get; }

    /// <summary>Gets the exact authoritative world-event definitions.</summary>
    public WorldEventSnapshot WorldEvents { get; }

    /// <summary>Gets the exact current authoritative world-event log.</summary>
    public WorldEventLogSnapshot EventLog { get; }

    /// <summary>Gets the exact current qualitative world-time state.</summary>
    public WorldTimeSnapshot WorldTime { get; }

    /// <summary>Gets stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterTargetSabotageDelayCandidate> Candidates { get; }
}
