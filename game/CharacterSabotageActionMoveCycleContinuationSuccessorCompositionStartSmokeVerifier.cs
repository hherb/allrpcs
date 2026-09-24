using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies explicit start of only the exact fifth-cycle generic Move request.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
{
    /// <summary>Checks rejected substitutes and one exact start without observing its result.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest request,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator composition =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator");
        TemporaryCharacterSabotageActionModelSource models =
            scene.GetNode<TemporaryCharacterSabotageActionModelSource>(
                "../SuccessorTemporaryCharacterSabotageActionModelSource");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../SuccessorCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            request.Actor,
            request.ActionKinds);
        CharacterSabotageActionConsiderationRequest foreignActorRequest = new(
            RepresentativeEnemyAgentCharacterSource.Create(),
            request.ActionKinds);
        CharacterSabotageActionConsiderationRequest wrongKindRequest = new(
            request.Actor,
            [CharacterSabotageActionKind.Conceal]);
        RecordingModel model = new();

        bool startedSuccessfully = models.TrySupplyModel(model)
            && target.StartedRequest is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && !target.TryStart(null)
            && !target.TryStart(composition.AuthorRequest)
            && !target.TryStart(rebuiltRequest)
            && !target.TryStart(foreignActorRequest)
            && !target.TryStart(wrongKindRequest)
            && model.CallCount == 0
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && target.TryStart(request)
            && ReferenceEquals(target.StartedRequest, request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && task.Result is null
            && model.CallCount == 1
            && !target.TryStart(request)
            && model.CallCount == 1
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return startedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    request,
                    recovered,
                    evidence,
                    model,
                    continuedActionModel,
                    continuedMoveModel,
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
