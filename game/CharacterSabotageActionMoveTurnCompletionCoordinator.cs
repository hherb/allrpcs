using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Exposes one exact completed follow-up Move turn after route reset.</summary>
public partial class CharacterSabotageActionMoveTurnCompletionCoordinator : Node
{
    private CharacterSabotageActionMoveTerminalRouteCoordinator terminalRoute = null!;

    /// <summary>Gets the one exact completed-turn token exposed by this coordinator.</summary>
    public CharacterSabotageTurnCompletion? Completion { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        terminalRoute = GetNode<CharacterSabotageActionMoveTerminalRouteCoordinator>(
            "../CharacterSabotageActionMoveTerminalRouteCoordinator");
    }

    /// <summary>Exposes completion only for the exact reset route and current recovery.</summary>
    public bool TryComplete(
        Character? expectedActor,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements)
    {
        if (Completion is not null
            || expectedActor is null
            || expectedAuthorRequest is null
            || !ReferenceEquals(expectedAuthorRequest.Actor, expectedActor)
            || expectedAuthorRequest.ActionKinds.Count != 1
            || !ReferenceEquals(expectedAuthorRequest.ActionKinds[0], expectedActionKind)
            || !terminalRoute.IsCurrentReset(
                expectedAuthorRequest,
                expectedActionKind,
                expectedRecoveredPlacements))
        {
            return false;
        }

        Completion = new CharacterSabotageTurnCompletion(
            expectedActor,
            expectedAuthorRequest,
            expectedActionKind!);
        return true;
    }

    /// <summary>Checks the exact completion token against its current reset route.</summary>
    internal bool IsCurrentCompletion(
        CharacterSabotageTurnCompletion? expectedCompletion,
        WorldObjectPlacementSnapshot? expectedRecoveredPlacements) =>
        expectedCompletion is not null
        && ReferenceEquals(Completion, expectedCompletion)
        && ReferenceEquals(expectedCompletion.Actor, expectedCompletion.AuthorRequest.Actor)
        && expectedCompletion.AuthorRequest.ActionKinds.Count == 1
        && ReferenceEquals(
            expectedCompletion.AuthorRequest.ActionKinds[0],
            expectedCompletion.ActionKind)
        && terminalRoute.IsCurrentReset(
            expectedCompletion.AuthorRequest,
            expectedCompletion.ActionKind,
            expectedRecoveredPlacements);
}
