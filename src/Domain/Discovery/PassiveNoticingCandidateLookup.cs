namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Selects a passive-noticing candidate by its complete value without changing the snapshot.
/// </summary>
public static class PassiveNoticingCandidateLookup
{
    /// <summary>
    /// Returns the exact retained candidate for a value-equal candidate, or null when none matches.
    /// </summary>
    public static PassiveNoticingCandidate? Find(
        PassiveNoticingCandidate candidate,
        PassiveNoticingCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(candidates);

        return candidates.Entries.FirstOrDefault(entry => entry == candidate);
    }
}
