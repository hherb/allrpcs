using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns exact scene concealment and authoritative evidence snapshots.</summary>
public partial class CharacterObjectSabotageConcealmentController : Node
{
    /// <summary>Gets the initialized exact concealment snapshot, if any.</summary>
    public WorldObjectConcealmentSnapshot? Concealment { get; private set; }

    /// <summary>Gets the initialized exact authoritative evidence snapshot, if any.</summary>
    public DiscoveryEvidenceSnapshot? Evidence { get; private set; }

    /// <summary>Atomically initializes exact concealment and evidence state once.</summary>
    public bool TryInitialize(
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence)
    {
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidence);

        if (Concealment is not null || Evidence is not null)
        {
            return false;
        }

        Concealment = concealment;
        Evidence = evidence;
        return true;
    }

    /// <summary>Applies exact eligible concealment only from expected current scene state.</summary>
    public bool TryApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot expectedConcealment,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(expectedConcealment);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Concealment, expectedConcealment)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageConcealmentResult result;
        try
        {
            result = CharacterObjectSabotageConcealmentTransition.Apply(
                character,
                physicalAccess,
                worldObjects,
                expectedConcealment,
                expectedEvidence,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Concealment = result.Concealment;
        Evidence = result.EvidenceResult.Evidence;
        return true;
    }

    /// <summary>Selects and applies the first eligible candidate from exact current scene state.</summary>
    public bool TryApplyFirstEligible(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot expectedConcealment,
        DiscoveryEvidenceSnapshot expectedEvidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(expectedConcealment);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(candidates);

        if (!ReferenceEquals(Concealment, expectedConcealment)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageConcealmentResult result;
        try
        {
            result = CharacterObjectSabotageConcealmentSequence.ApplyFirstEligible(
                character,
                physicalAccess,
                worldObjects,
                expectedConcealment,
                expectedEvidence,
                candidates);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Concealment = result.Concealment;
        Evidence = result.EvidenceResult.Evidence;
        return true;
    }

    /// <summary>Recovers exact eligible concealment only from expected current scene state.</summary>
    public bool TryRecover(
        WorldObjectConcealmentSnapshot expectedConcealment,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(expectedConcealment);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Concealment, expectedConcealment)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageConcealmentRecoveryResult result;
        try
        {
            result = CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                expectedConcealment,
                expectedEvidence,
                evidenceView,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Concealment = result.Concealment;
        return true;
    }
}
