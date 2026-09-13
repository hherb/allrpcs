using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains one accepted sabotage candidate and its resulting evidence snapshot.</summary>
public sealed record CharacterObjectSabotageEvidenceResult
{
    /// <summary>Creates one immutable evidence-leave result.</summary>
    public CharacterObjectSabotageEvidenceResult(
        CharacterObjectSabotageEvidence candidate,
        DiscoveryEvidenceSnapshot evidence)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(evidence);

        Candidate = candidate;
        Evidence = evidence;
    }

    /// <summary>Gets the exact accepted evidence-bound sabotage candidate.</summary>
    public CharacterObjectSabotageEvidence Candidate { get; }

    /// <summary>Gets the fresh snapshot containing the appended exact evidence.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; }
}
