using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact eligible target-delay candidate without applying it.</summary>
public static class CharacterTargetSabotageDelaySelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first eligible candidate in stable caller order, or null.</summary>
    public static CharacterTargetSabotageDelayCandidate? SelectFirstEligible(
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
                    "Target-delay candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Target-delay candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Target-delay selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterTargetSabotageDelayPolicy.CanApply(
                actor,
                interactionAccess,
                worldEvents,
                eventLog,
                worldTime,
                candidate));
    }
}
