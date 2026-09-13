using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one object sabotage occurrence candidate to exact discoverable evidence.</summary>
public sealed record CharacterObjectSabotageEvidence
{
    /// <summary>Creates one immutable evidence-bound candidate without changing world state.</summary>
    public CharacterObjectSabotageEvidence(
        CharacterObjectSabotageOccurrence occurrence,
        DiscoveryEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(evidence);

        Occurrence = occurrence;
        Evidence = evidence;
    }

    /// <summary>Gets the exact proposed object sabotage occurrence.</summary>
    public CharacterObjectSabotageOccurrence Occurrence { get; }

    /// <summary>Gets the exact evidence definition that the occurrence would leave.</summary>
    public DiscoveryEvidence Evidence { get; }
}
