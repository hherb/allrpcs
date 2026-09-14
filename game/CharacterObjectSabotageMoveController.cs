using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns exact scene placement and authoritative sabotage-evidence snapshots.</summary>
public partial class CharacterObjectSabotageMoveController : Node
{
    private CharacterObjectSabotageMoveConsiderationRequest? lastResolvedRequest;

    /// <summary>Gets the initialized exact world-object placement snapshot, if any.</summary>
    public WorldObjectPlacementSnapshot? Placements { get; private set; }

    /// <summary>Gets the initialized exact authoritative evidence snapshot, if any.</summary>
    public DiscoveryEvidenceSnapshot? Evidence { get; private set; }

    /// <summary>Atomically initializes exact placement and evidence state once.</summary>
    public bool TryInitialize(
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence)
    {
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidence);

        if (Placements is not null || Evidence is not null)
        {
            return false;
        }

        Placements = placements;
        Evidence = evidence;
        return true;
    }

    /// <summary>Applies exact eligible move sabotage only from expected current scene state.</summary>
    public bool TryApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectPlacementSnapshot expectedPlacements,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterObjectSabotageMoveCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(expectedPlacements);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Placements, expectedPlacements)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageMoveResult result;
        try
        {
            result = CharacterObjectSabotageMoveTransition.Apply(
                character,
                physicalAccess,
                worldObjects,
                expectedPlacements,
                expectedEvidence,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Placements = result.Placements;
        Evidence = result.EvidenceResult.Evidence;
        return true;
    }

    /// <summary>Resolves one exact accepted consideration result from current scene state.</summary>
    public bool TryResolveConsideration(
        CharacterObjectSabotageMoveConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterObjectSabotageMoveConsiderationRequest request = considerationResult.Request;
        if (ReferenceEquals(lastResolvedRequest, request)
            || !ReferenceEquals(Placements, request.Placements)
            || !ReferenceEquals(Evidence, request.Evidence))
        {
            return false;
        }

        CharacterObjectSabotageMoveResult? result =
            CharacterObjectSabotageMoveConsiderationTransition.Apply(considerationResult);
        if (result is not null)
        {
            Placements = result.Placements;
            Evidence = result.EvidenceResult.Evidence;
        }

        lastResolvedRequest = request;
        return true;
    }

    /// <summary>Recovers one exact moved target only from expected current scene state.</summary>
    public bool TryRecover(
        WorldObjectPlacementSnapshot expectedPlacements,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(expectedPlacements);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Placements, expectedPlacements)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageMoveRecoveryResult result;
        try
        {
            result = CharacterObjectSabotageMoveRecoveryTransition.Recover(
                expectedPlacements,
                expectedEvidence,
                evidenceView,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Placements = result.Placements;
        return true;
    }
}
