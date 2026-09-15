using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded world state for one provider-neutral substitution choice.</summary>
public sealed record CharacterObjectSabotageSubstitutionConsiderationRequest
{
    /// <summary>The maximum number of exact substitution candidates in one consideration.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Creates one immutable request without selecting or applying a substitution.</summary>
    public CharacterObjectSabotageSubstitutionConsiderationRequest(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageSubstitutionCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterObjectSabotageSubstitutionCandidate> retainedCandidates = [];
        HashSet<EvidenceId> evidenceIds = [];
        foreach (CharacterObjectSabotageSubstitutionCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Substitution consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!evidenceIds.Add(candidate.Evidence.Evidence.EvidenceId))
            {
                throw new ArgumentException(
                    "Substitution consideration candidates require unique evidence identifiers.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Substitution consideration exceeded the bounded candidate count.");
            }
        }

        Character = character;
        PhysicalAccess = physicalAccess;
        WorldObjects = worldObjects;
        Substitutions = substitutions;
        Evidence = evidence;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering a substitution.</summary>
    public Character Character { get; }

    /// <summary>Gets the exact physical-access snapshot.</summary>
    public CharacterPhysicalAccessSnapshot PhysicalAccess { get; }

    /// <summary>Gets the exact authoritative world-object snapshot.</summary>
    public WorldObjectSnapshot WorldObjects { get; }

    /// <summary>Gets the exact current substitution snapshot.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot Substitutions { get; }

    /// <summary>Gets the exact current authoritative evidence snapshot.</summary>
    public DiscoveryEvidenceSnapshot Evidence { get; }

    /// <summary>Gets the stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterObjectSabotageSubstitutionCandidate> Candidates { get; }
}
