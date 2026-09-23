using Godot;

namespace Allrpcs.Game;

/// <summary>Consumes one exact follow-up Move authorization at an explicit cycle boundary.</summary>
public partial class CharacterSabotageActionMoveCycleBoundaryCoordinator : Node
{
    private CharacterSabotageActionMoveSchedulingAuthorizationCoordinator authorization = null!;

    /// <summary>Gets the one exact follow-up sabotage-cycle boundary retained here.</summary>
    public CharacterSabotageCycleBoundary? Boundary { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authorization = GetNode<CharacterSabotageActionMoveSchedulingAuthorizationCoordinator>(
            "../CharacterSabotageActionMoveSchedulingAuthorizationCoordinator");
    }

    /// <summary>Enters once only with the exact current follow-up authorization.</summary>
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

    /// <summary>Checks the exact cycle boundary against its complete current authorization.</summary>
    internal bool IsCurrentBoundary(CharacterSabotageCycleBoundary? expectedBoundary) =>
        expectedBoundary is not null
        && ReferenceEquals(Boundary, expectedBoundary)
        && ReferenceEquals(expectedBoundary.Authorization, authorization.Authorization)
        && authorization.IsCurrentAuthorization(expectedBoundary.Authorization);
}
