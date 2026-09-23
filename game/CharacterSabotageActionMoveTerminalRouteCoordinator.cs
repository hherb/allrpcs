using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact recovered follow-up Move as a terminal route.</summary>
public partial class CharacterSabotageActionMoveTerminalRouteCoordinator : Node
{
    private CharacterSabotageActionMoveCompositionCoordinator composition = null!;
    private CharacterSabotageActionMoveRecoveryCoordinator recovery = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact guarded author request retained at terminal state.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical kind retained with the terminal route.</summary>
    public CharacterSabotageActionKind? ActionKind { get; private set; }

    /// <summary>Gets the exact recovered placement snapshot retained at terminal state.</summary>
    public WorldObjectPlacementSnapshot? RecoveredPlacements { get; private set; }

    /// <summary>Gets the exact guarded author request whose terminal route was reset.</summary>
    public CharacterSabotageActionConsiderationRequest? ResetAuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical kind whose terminal route was reset.</summary>
    public CharacterSabotageActionKind? ResetActionKind { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<CharacterSabotageActionMoveCompositionCoordinator>(
            "../CharacterSabotageActionMoveCompositionCoordinator");
        recovery = GetNode<CharacterSabotageActionMoveRecoveryCoordinator>(
            "../CharacterSabotageActionMoveRecoveryCoordinator");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Retains only the exact current recovered follow-up Move once.</summary>
    public bool TryRetain(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements)
    {
        if (AuthorRequest is not null
            || ActionKind is not null
            || RecoveredPlacements is not null
            || ResetAuthorRequest is not null
            || ResetActionKind is not null
            || expectedAuthorRequest is null
            || !ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(composition.AuthorRequest, expectedAuthorRequest)
            || !composition.IsCurrentRequest(composition.Request)
            || expectedAuthorRequest.ActionKinds.Count != 1
            || !ReferenceEquals(
                expectedAuthorRequest.ActionKinds[0],
                CharacterSabotageActionKind.Move)
            || !ReferenceEquals(recovery.RecoveredPlacements, expectedRecoveredPlacements)
            || !ReferenceEquals(state.Placements, expectedRecoveredPlacements)
            || recovery.RecoveryCandidate is null
            || recovery.EvidenceView is null
            || state.Evidence is null)
        {
            return false;
        }

        AuthorRequest = expectedAuthorRequest;
        ActionKind = expectedActionKind;
        RecoveredPlacements = expectedRecoveredPlacements;
        return true;
    }

    /// <summary>Clears only this seam's exact retained terminal route once.</summary>
    public bool TryReset(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements)
    {
        if (ResetAuthorRequest is not null
            || ResetActionKind is not null
            || !IsCurrentRoute(
                expectedAuthorRequest,
                expectedActionKind,
                expectedRecoveredPlacements))
        {
            return false;
        }

        AuthorRequest = null;
        ActionKind = null;
        RecoveredPlacements = null;
        ResetAuthorRequest = expectedAuthorRequest;
        ResetActionKind = expectedActionKind;
        return true;
    }

    /// <summary>Checks the exact retained route while its recovered Move remains current.</summary>
    internal bool IsCurrentRoute(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements) =>
        expectedAuthorRequest is not null
        && expectedRecoveredPlacements is not null
        && ReferenceEquals(AuthorRequest, expectedAuthorRequest)
        && ReferenceEquals(ActionKind, expectedActionKind)
        && ReferenceEquals(RecoveredPlacements, expectedRecoveredPlacements)
        && ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(composition.AuthorRequest, expectedAuthorRequest)
        && composition.IsCurrentRequest(composition.Request)
        && ReferenceEquals(recovery.RecoveredPlacements, expectedRecoveredPlacements)
        && ReferenceEquals(state.Placements, expectedRecoveredPlacements)
        && recovery.RecoveryCandidate is not null
        && recovery.EvidenceView is not null
        && state.Evidence is not null;

    /// <summary>Checks the exact reset provenance while the recovered Move remains current.</summary>
    internal bool IsCurrentReset(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements) =>
        expectedAuthorRequest is not null
        && expectedRecoveredPlacements is not null
        && AuthorRequest is null
        && ActionKind is null
        && RecoveredPlacements is null
        && ReferenceEquals(ResetAuthorRequest, expectedAuthorRequest)
        && ReferenceEquals(ResetActionKind, expectedActionKind)
        && ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(composition.AuthorRequest, expectedAuthorRequest)
        && composition.IsCurrentRequest(composition.Request)
        && ReferenceEquals(recovery.RecoveredPlacements, expectedRecoveredPlacements)
        && ReferenceEquals(state.Placements, expectedRecoveredPlacements)
        && recovery.RecoveryCandidate is not null
        && recovery.EvidenceView is not null
        && state.Evidence is not null;
}
