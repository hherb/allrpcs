using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes one exact completed applied typed third-cycle Move result.</summary>
public partial class CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator : Node
{
    private CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator start = null!;
    private CharacterObjectSabotageMoveTaskController task = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact completed applied typed Move result retained here.</summary>
    public CharacterObjectSabotageMoveConsiderationResult? ObservedResult { get; private set; }

    /// <summary>Gets the exact fresh authoritative placements observed with the result.</summary>
    public WorldObjectPlacementSnapshot? ObservedPlacements { get; private set; }

    /// <summary>Gets the exact fresh authoritative evidence observed with the result.</summary>
    public DiscoveryEvidenceSnapshot? ObservedEvidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        start = GetNode<CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator");
        task = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CycledCharacterObjectSabotageMoveTaskController");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Observes once after revalidating the exact result and its fresh state.</summary>
    public bool TryObserveCompletedResult(
        CharacterObjectSabotageMoveConsiderationResult? expectedResult)
    {
        WorldObjectPlacementSnapshot? placements = state.Placements;
        DiscoveryEvidenceSnapshot? evidence = state.Evidence;
        if (ObservedResult is not null
            || ObservedPlacements is not null
            || ObservedEvidence is not null
            || placements is null
            || evidence is null
            || !start.IsCurrentCompletedResult(expectedResult)
            || !ReferenceEquals(task.Result, expectedResult))
        {
            return false;
        }

        ObservedResult = expectedResult;
        ObservedPlacements = placements;
        ObservedEvidence = evidence;
        return true;
    }
}
