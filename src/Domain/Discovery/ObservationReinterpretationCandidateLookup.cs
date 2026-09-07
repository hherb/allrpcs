namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Finds an observation-reinterpretation candidate by its complete value without changing the
/// snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateLookup
{
    /// <summary>
    /// Returns the exact retained candidate for a value-equal candidate, or null when none matches.
    /// </summary>
    public static ObservationReinterpretationCandidate? Find(
        ObservationReinterpretationCandidate candidate,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(candidates);

        return candidates.Entries.FirstOrDefault(entry => entry == candidate);
    }
}
