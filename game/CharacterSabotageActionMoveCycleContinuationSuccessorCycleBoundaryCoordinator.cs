using Godot;

namespace Allrpcs.Game;

/// <summary>Consumes the exact fifth-cycle authorization at its successor boundary.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator
        authorization = null!;

    /// <summary>Gets the one exact successor boundary retained here.</summary>
    public CharacterSabotageCycleBoundary? Boundary { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authorization = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator");
    }

    /// <summary>Enters once only with the exact current fifth-cycle authorization.</summary>
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

    /// <summary>Checks the exact successor boundary against its current fifth-cycle graph.</summary>
    internal bool IsCurrentBoundary(CharacterSabotageCycleBoundary? expectedBoundary) =>
        expectedBoundary is not null
        && ReferenceEquals(Boundary, expectedBoundary)
        && ReferenceEquals(expectedBoundary.Authorization, authorization.Authorization)
        && authorization.IsCurrentAuthorization(expectedBoundary.Authorization);
}
