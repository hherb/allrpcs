namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one observation-reinterpretation candidate can be recorded without changing
/// the snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateRecordingPolicy
{
    /// <summary>
    /// Returns true when no value-equal complete candidate is already retained.
    /// </summary>
    public static bool CanRecord(
        ObservationReinterpretationCandidate candidate,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return ObservationReinterpretationCandidateLookup.Find(candidate, candidates) is null;
    }
}
