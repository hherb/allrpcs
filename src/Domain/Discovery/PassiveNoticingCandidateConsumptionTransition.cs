namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Removes one eligible passive-noticing candidate from a candidate snapshot.
/// </summary>
public static class PassiveNoticingCandidateConsumptionTransition
{
    /// <summary>
    /// Returns a new snapshot without the value-equal complete retained candidate.
    /// </summary>
    public static PassiveNoticingCandidateSnapshot Consume(
        PassiveNoticingCandidate candidate,
        PassiveNoticingCandidateSnapshot candidates)
    {
        if (!PassiveNoticingCandidateConsumptionPolicy.CanConsume(candidate, candidates))
        {
            throw new InvalidOperationException(
                "Only a complete retained passive-noticing candidate may be consumed.");
        }

        return new PassiveNoticingCandidateSnapshot(
            candidates.Entries.Where(entry => entry != candidate));
    }
}
