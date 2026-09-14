using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded world state for one provider-neutral move choice.</summary>
public sealed record CharacterObjectSabotageMoveConsiderationRequest
{
    /// <summary>The maximum number of exact move candidates in one consideration.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Creates one immutable request without selecting or applying a move.</summary>
    public CharacterObjectSabotageMoveConsiderationRequest(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageMoveCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterObjectSabotageMoveCandidate> retainedCandidates = [];
        HashSet<EvidenceId> evidenceIds = [];
        foreach (CharacterObjectSabotageMoveCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Move consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!evidenceIds.Add(candidate.Evidence.Evidence.EvidenceId))
            {
                throw new ArgumentException(
                    "Move consideration candidates require unique evidence identifiers.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Move consideration exceeded the bounded candidate count.");
            }
        }

        Character = character;
        PhysicalAccess = physicalAccess;
        WorldObjects = worldObjects;
        Placements = placements;
        Evidence = evidence;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering a move.</summary>
    public Character Character { get; }

    /// <summary>Gets the exact physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; }

    /// <summary>Gets the exact authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; }

    /// <summary>Gets the exact current authoritative placement snapshot.</summary>
    public WorldObjectPlacementSnapshot Placements { get; }

    /// <summary>Gets the exact current authoritative evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; }

    /// <summary>Gets the stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterObjectSabotageMoveCandidate> Candidates { get; }
}
