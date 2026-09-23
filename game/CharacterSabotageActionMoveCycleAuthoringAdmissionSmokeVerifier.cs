using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact follow-up Move cycle authoring admission.</summary>
internal static class CharacterSabotageActionMoveCycleAuthoringAdmissionSmokeVerifier
{
    /// <summary>Checks substitute boundaries and admits only the exact current cycle once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageCycleBoundary boundary,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator>(
                "../CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator");
        CharacterSabotageCycleBoundary rebuiltBoundary = new(boundary.Authorization);
        CharacterSabotageTurnCompletion completion = boundary.Authorization.Admission;
        CharacterSabotageCycleBoundary malformedBoundary = new(
            new CharacterSabotageOrdinaryTurnSchedulingAuthorization(
                new CharacterSabotageTurnCompletion(
                    completion.Actor,
                    completion.AuthorRequest,
                    CharacterSabotageActionKind.Conceal),
                boundary.Authorization.TurnActivity));

        if (target.Admission is not null
            || target.TryAdmit(null)
            || target.TryAdmit(rebuiltBoundary)
            || target.TryAdmit(malformedBoundary)
            || !target.TryAdmit(boundary)
            || target.Admission is not { } admission
            || !ReferenceEquals(admission.Boundary, boundary)
            || !ReferenceEquals(admission.Actor, completion.Actor)
            || target.TryAdmit(boundary)
            || !ReferenceEquals(target.Admission, admission)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return true;
    }
}
