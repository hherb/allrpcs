namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one passive-noticing candidate can be recorded without changing the snapshot.
/// </summary>
public static class PassiveNoticingCandidateRecordingPolicy
{
    /// <summary>
    /// Returns true when no value-equal complete candidate is already retained.
    /// </summary>
    public static bool CanRecord(
        PassiveNoticingCandidate candidate,
        PassiveNoticingCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return PassiveNoticingCandidateLookup.Find(candidate, candidates) is null;
    }
}
