using Godot;

namespace Allrpcs.Game;

/// <summary>Admits one exact current successor cycle before future authoring.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringAdmissionCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator boundary =
        null!;

    /// <summary>Gets the one exact successor action-authoring admission retained here.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        boundary =
            GetNode<CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator");
    }

    /// <summary>Admits once only after revalidating the complete exact successor graph.</summary>
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

    /// <summary>Checks the exact admission against its complete current successor cycle.</summary>
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
