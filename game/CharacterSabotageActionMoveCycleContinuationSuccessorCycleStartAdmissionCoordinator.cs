using Godot;

namespace Allrpcs.Game;

/// <summary>Admits the exact fifth-cycle successor through the pure cycle-start gate.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorCycleStartAdmissionCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator boundary =
        null!;

    /// <summary>Gets the one exact cycle-start admission retained here.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        boundary = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator");
    }

    /// <summary>Admits once only after revalidating the complete exact fifth-cycle graph.</summary>
    public bool TryAdmit(CharacterSabotageCycleBoundary? expectedBoundary)
    {
        CharacterSabotageCycleBoundary? currentBoundary = boundary.Boundary;
        if (Admission is not null
            || currentBoundary is null
            || !boundary.IsCurrentBoundary(expectedBoundary)
            || !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                currentBoundary,
                expectedBoundary,
                currentBoundary.Authorization,
                currentBoundary.Authorization.Admission,
                currentBoundary.Authorization.TurnActivity))
        {
            return false;
        }

        Admission = new CharacterSabotageActionAuthoringAdmission(currentBoundary);
        return true;
    }

    /// <summary>Checks the exact admission against its complete current fifth-cycle graph.</summary>
    internal bool IsCurrentAdmission(
        CharacterSabotageActionAuthoringAdmission? expectedAdmission) =>
        expectedAdmission is not null
        && ReferenceEquals(Admission, expectedAdmission)
        && boundary.IsCurrentBoundary(expectedAdmission.Boundary)
        && CharacterSabotageCycleStartEligibilityPolicy.CanStart(
            boundary.Boundary,
            expectedAdmission.Boundary,
            expectedAdmission.Boundary.Authorization,
            expectedAdmission.Boundary.Authorization.Admission,
            expectedAdmission.Boundary.Authorization.TurnActivity);
}
