using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies explicit start of only the exact continuation Move request.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier
{
    /// <summary>Checks rejected substitute requests and one explicit exact start.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest request,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationCompositionCoordinator composition =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationCompositionCoordinator");
        CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator");
        TemporaryCharacterSabotageActionModelSource models =
            scene.GetNode<TemporaryCharacterSabotageActionModelSource>(
                "../ContinuedTemporaryCharacterSabotageActionModelSource");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../ContinuedCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.ActionKinds);
        RecordingModel continuedModel = new();

        bool startedSuccessfully = models.TrySupplyModel(continuedModel)
            && target.StartedRequest is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && !target.TryStart(null)
            && !target.TryStart(composition.AuthorRequest)
            && !target.TryStart(rebuiltRequest)
            && continuedModel.CallCount == 0
            && target.TryStart(request)
            && ReferenceEquals(target.StartedRequest, request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && task.Result is null
            && continuedModel.CallCount == 1
            && !target.TryStart(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return startedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationResultObservationSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    request,
                    recovered,
                    evidence,
                    continuedModel,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }

    internal sealed class RecordingModel : ICharacterSabotageActionDecisionModel
    {
        internal int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    request.Actor.Identity.Id,
                    CharacterSabotageActionKind.Move));
        }
    }
}
