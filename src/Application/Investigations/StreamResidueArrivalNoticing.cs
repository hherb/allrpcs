using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Passively notices the stream residue when the Seeker arrives at the stream.</summary>
/// <remarks>
/// Stateless on purpose: the only record of the noticing is the evidence it routes into the saved
/// route progress, so a load can never leave a stale "already noticed" flag behind.
/// </remarks>
public static class StreamResidueArrivalNoticing
{
    private const string StreamLocationId = "stream";
    private const string ResidueEvidenceId = "residue";

    /// <summary>
    /// Returns the observer's residue-only evidence view when the verified location is the stream,
    /// resolved from the scenario's authored passive-noticing candidate; otherwise null.
    /// </summary>
    public static CharacterEvidenceViewSnapshot? Notice(DiscoveryScenario scenario, LocationId verifiedLocationId)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(verifiedLocationId);

        if (verifiedLocationId.Value != StreamLocationId)
        {
            return null;
        }

        // Notice from the authored starting perception, then keep only the residue the noticing exposed.
        PassiveNoticingCandidateResolutionResult result = PassiveNoticingCandidateResolutionSequence.ResolveNext(
            scenario.ObserverCharacterId,
            scenario.InitialPerception,
            scenario.PassiveNoticingCandidates);
        EvidenceId residue = DiscoveryEvidenceProjection.ForCharacter(scenario.Evidence, result.Perception)
            .EvidenceIds.Single(evidenceId => evidenceId.Value == ResidueEvidenceId);
        return new CharacterEvidenceViewSnapshot(scenario.ObserverCharacterId, [residue]);
    }
}
