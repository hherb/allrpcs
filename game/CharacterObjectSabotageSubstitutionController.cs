using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns exact scene substitution and authoritative sabotage-evidence snapshots.</summary>
public partial class CharacterObjectSabotageSubstitutionController : Node
{
    /// <summary>Gets the initialized exact active substitution snapshot, if any.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot? Substitutions { get; private set; }

    /// <summary>Gets the initialized exact authoritative evidence snapshot, if any.</summary>
    public DiscoveryEvidenceSnapshot? Evidence { get; private set; }

    /// <summary>Atomically initializes exact substitution and evidence state once.</summary>
    public bool TryInitialize(
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence)
    {
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidence);

        if (Substitutions is not null || Evidence is not null)
        {
            return false;
        }

        Substitutions = substitutions;
        Evidence = evidence;
        return true;
    }

    /// <summary>Applies one eligible exact substitution only from expected current scene state.</summary>
    public bool TryApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        CharacterObjectSabotageSubstitutionSnapshot expectedSubstitutions,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(expectedSubstitutions);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Substitutions, expectedSubstitutions)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageSubstitutionResult result;
        try
        {
            result = CharacterObjectSabotageSubstitutionTransition.Apply(
                character,
                physicalAccess,
                worldObjects,
                expectedSubstitutions,
                expectedEvidence,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Substitutions = result.Substitutions;
        Evidence = result.EvidenceResult.Evidence;
        return true;
    }

    /// <summary>Recovers one exact substitution only from expected current scene state.</summary>
    public bool TryRecover(
        CharacterObjectSabotageSubstitutionSnapshot expectedSubstitutions,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(expectedSubstitutions);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Substitutions, expectedSubstitutions)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageSubstitutionRecoveryResult result;
        try
        {
            result = CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                expectedSubstitutions,
                expectedEvidence,
                evidenceView,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Substitutions = result.Substitutions;
        return true;
    }
}
