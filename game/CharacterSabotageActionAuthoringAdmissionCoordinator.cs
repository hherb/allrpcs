using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains exact provenance admitted at the first sabotage action-authoring seam.</summary>
public sealed class CharacterSabotageActionAuthoringAdmission
{
    /// <summary>Creates one fresh admission from the exact current sabotage-cycle boundary.</summary>
    internal CharacterSabotageActionAuthoringAdmission(CharacterSabotageCycleBoundary boundary)
    {
        Boundary = boundary;
    }

    /// <summary>Gets the exact sabotage-cycle boundary admitted for future authoring.</summary>
    public CharacterSabotageCycleBoundary Boundary { get; }

    /// <summary>Gets the actor by following the retained boundary's exact completion.</summary>
    public Character Actor => Boundary.Authorization.Admission.Actor;
}

/// <summary>Admits one exact current sabotage-cycle boundary before action authoring.</summary>
public partial class CharacterSabotageActionAuthoringAdmissionCoordinator : Node
{
    private CharacterSabotageCycleBoundaryCoordinator boundary = null!;
    private CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator authorization = null!;
    private CharacterSabotageTurnAdmissionCoordinator turnAdmission = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the one exact action-authoring admission retained here.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        boundary = GetNode<CharacterSabotageCycleBoundaryCoordinator>(
            "../CharacterSabotageCycleBoundaryCoordinator");
        authorization =
            GetNode<CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator");
        turnAdmission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Admits once only after revalidating the complete exact current cycle graph.</summary>
    public bool TryAdmit(CharacterSabotageCycleBoundary? expectedBoundary)
    {
        CharacterSabotageCycleBoundary? currentBoundary = boundary.Boundary;
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? currentAuthorization =
            authorization.Authorization;
        CharacterSabotageTurnCompletion? currentAdmission = turnAdmission.Admission;
        CharacterTurnActivity? currentTurnActivity = turns.Activity;
        if (Admission is not null
            || currentBoundary is null
            || !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                currentBoundary,
                expectedBoundary,
                currentAuthorization,
                currentAdmission,
                currentTurnActivity))
        {
            return false;
        }

        Admission = new CharacterSabotageActionAuthoringAdmission(currentBoundary);
        return true;
    }
}
