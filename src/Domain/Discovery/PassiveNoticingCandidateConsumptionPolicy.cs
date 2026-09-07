namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one complete passive-noticing candidate is retained for consumption without
/// changing the candidate snapshot.
/// </summary>
public static class PassiveNoticingCandidateConsumptionPolicy
{
    /// <summary>
    /// Returns true when a value-equal complete candidate is retained in the snapshot.
    /// </summary>
    public static bool CanConsume(
        PassiveNoticingCandidate candidate,
        PassiveNoticingCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        PassiveNoticingCandidate? retained = PassiveNoticingCandidateLookup.Find(
            candidate,
            candidates);

        return retained == candidate;
    }
}
