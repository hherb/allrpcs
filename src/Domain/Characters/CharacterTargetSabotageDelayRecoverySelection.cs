using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact eligible target-delay recovery without applying it.</summary>
public static class CharacterTargetSabotageDelayRecoverySelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first eligible candidate in stable caller order, or null.</summary>
    public static CharacterTargetSabotageDelayRecoveryCandidate? SelectFirstEligible(
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot currentEventLog,
        WorldTimeSnapshot currentWorldTime,
        IEnumerable<CharacterTargetSabotageDelayRecoveryCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(currentEventLog);
        ArgumentNullException.ThrowIfNull(currentWorldTime);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterTargetSabotageDelayRecoveryCandidate> retainedCandidates = [];
        HashSet<CharacterTargetSabotageDelayRecoveryCandidate> distinctCandidates = new(
            ReferenceEqualityComparer.Instance);

        foreach (CharacterTargetSabotageDelayRecoveryCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Target-delay recovery candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Target-delay recovery candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Target-delay recovery selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                worldEvents,
                currentEventLog,
                currentWorldTime,
                candidate));
    }
}
