using Godot;

namespace Allrpcs.Game;

/// <summary>Consumes one exact third-cycle Move authorization at a continuation boundary.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationBoundaryCoordinator : Node
{
    private CharacterSabotageActionMoveCycleSchedulingAuthorizationCoordinator authorization = null!;

    /// <summary>Gets the one exact continuation boundary retained here.</summary>
    public CharacterSabotageCycleBoundary? Boundary { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authorization = GetNode<CharacterSabotageActionMoveCycleSchedulingAuthorizationCoordinator>(
            "../CharacterSabotageActionMoveCycleSchedulingAuthorizationCoordinator");
    }

    /// <summary>Enters once only with the exact current third-cycle authorization.</summary>
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

    /// <summary>Checks the exact continuation boundary against its current authorization.</summary>
    internal bool IsCurrentBoundary(CharacterSabotageCycleBoundary? expectedBoundary) =>
        expectedBoundary is not null
        && ReferenceEquals(Boundary, expectedBoundary)
        && ReferenceEquals(expectedBoundary.Authorization, authorization.Authorization)
        && authorization.IsCurrentAuthorization(expectedBoundary.Authorization);
}
