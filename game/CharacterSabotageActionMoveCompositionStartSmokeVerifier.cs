using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies explicit start of only the exact fresh admission-guarded Move request.</summary>
internal static class CharacterSabotageActionMoveCompositionStartSmokeVerifier
{
    /// <summary>Checks rejected substitute requests and one explicit exact start.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state)
    {
        CharacterSabotageActionMoveCompositionCoordinator composition =
            scene.GetNode<CharacterSabotageActionMoveCompositionCoordinator>(
                "../CharacterSabotageActionMoveCompositionCoordinator");
        CharacterSabotageActionMoveCompositionStartCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCompositionStartCoordinator>(
                "../CharacterSabotageActionMoveCompositionStartCoordinator");
        TemporaryCharacterSabotageActionModelSource models =
            scene.GetNode<TemporaryCharacterSabotageActionModelSource>(
                "../ScheduledTemporaryCharacterSabotageActionModelSource");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../ScheduledCharacterSabotageActionTaskController");
        if (composition.Request is not { } request
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            source.Character,
            [CharacterSabotageActionKind.Move]);
        RecordingModel model = new();
        if (!models.TrySupplyModel(model)
            || target.StartedRequest is not null
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || task.Result is not null
            || target.TryStart(null)
            || target.TryStart(composition.AuthorRequest)
            || target.TryStart(rebuiltRequest)
            || model.CallCount != 0
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || !target.TryStart(request)
            || !ReferenceEquals(target.StartedRequest, request)
            || task.Status != CharacterSabotageActionTaskStatus.Running
            || task.Result is not null
            || model.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return !target.TryStart(request)
            && ReferenceEquals(target.StartedRequest, request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && task.Result is null
            && model.CallCount == 1
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
    }

    private sealed class RecordingModel : ICharacterSabotageActionDecisionModel
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
