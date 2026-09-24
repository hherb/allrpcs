using Godot;

namespace Allrpcs.Game;

/// <summary>Consumes one exact continuation Move authorization at a successor boundary.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator
    : Node
{
    private CharacterSabotageActionMoveCycleContinuationSchedulingAuthorizationCoordinator
        authorization = null!;

    /// <summary>Gets the one exact successor boundary retained here.</summary>
    public CharacterSabotageCycleBoundary? Boundary { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authorization =
            GetNode<CharacterSabotageActionMoveCycleContinuationSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSchedulingAuthorizationCoordinator");
    }

    /// <summary>Enters once only with the exact current continuation authorization.</summary>
    public bool TryEnter(
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? expectedAuthorization)
    {
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? currentAuthorization =
            authorization.Authorization;
        if (Boundary is not null
            || currentAuthorization is null
            || !ReferenceEquals(currentAuthorization, expectedAuthorization)
            || !authorization.IsCurrentAuthorization(currentAuthorization))
        {
            return false;
        }

        Boundary = new CharacterSabotageCycleBoundary(currentAuthorization);
        return true;
    }

    /// <summary>Checks the exact successor boundary against its current authorization.</summary>
    internal bool IsCurrentBoundary(CharacterSabotageCycleBoundary? expectedBoundary) =>
        expectedBoundary is not null
        && ReferenceEquals(Boundary, expectedBoundary)
        && ReferenceEquals(expectedBoundary.Authorization, authorization.Authorization)
        && authorization.IsCurrentAuthorization(expectedBoundary.Authorization);
}
