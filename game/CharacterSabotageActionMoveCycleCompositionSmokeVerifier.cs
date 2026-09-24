using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact one-shot composition of the guarded cycle Move output.</summary>
internal static class CharacterSabotageActionMoveCycleCompositionSmokeVerifier
{
    /// <summary>Checks rejected substitute requests and one fresh exact Move request.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel,
        RecordingMoveModel moveModel)
    {
        CharacterSabotageActionMoveCycleCompositionCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleCompositionCoordinator");
        CharacterSabotageActionMoveCycleAuthoringCoordinator authoring =
            scene.GetNode<CharacterSabotageActionMoveCycleAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleAuthoringCoordinator");
        CharacterSabotageActionMoveRequestController author =
            scene.GetNode<CharacterSabotageActionMoveRequestController>(
                "../CycledCharacterSabotageActionMoveRequestController");
        if (authoring.Admission is not { } admission
            || authoring.AuthorRequest is not { } authorRequest
            || author.CandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    binding
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            source.Character,
            [CharacterSabotageActionKind.Move]);
        CharacterSabotageActionConsiderationRequest foreignActorRequest = new(
            RepresentativeEnemyAgentCharacterSource.Create(),
            [CharacterSabotageActionKind.Move]);
        CharacterSabotageActionConsiderationRequest wrongKindRequest = new(
            source.Character,
            [CharacterSabotageActionKind.Conceal]);

        if (target.Admission is not null
            || target.AuthorRequest is not null
            || target.Request is not null
            || target.TryCompose(null)
            || target.TryCompose(rebuiltRequest)
            || target.TryCompose(foreignActorRequest)
            || target.TryCompose(wrongKindRequest)
            || !target.TryCompose(authorRequest)
            || target.Request is not { } request
            || ReferenceEquals(request, authorRequest)
            || !ReferenceEquals(target.Admission, admission)
            || !ReferenceEquals(target.AuthorRequest, authorRequest)
            || !ReferenceEquals(request.Actor, source.Character)
            || request.ActionKinds.Count != 1
            || !ReferenceEquals(request.ActionKinds.Single(), CharacterSabotageActionKind.Move)
            || !ReferenceEquals(authoring.Admission, admission)
            || !ReferenceEquals(authoring.AuthorRequest, authorRequest)
            || !ReferenceEquals(author.Request, authorRequest)
            || !ReferenceEquals(author.CandidateBinding, binding)
            || !ReferenceEquals(binding.Request, authorRequest)
            || !ReferenceEquals(binding.Candidate, source.CycleCandidate)
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence)
            || actionModel.CallCount != 1
            || moveModel.CallCount != 1)
        {
            return false;
        }

        return !target.TryCompose(authorRequest)
            && ReferenceEquals(target.Admission, admission)
            && ReferenceEquals(target.AuthorRequest, authorRequest)
            && ReferenceEquals(target.Request, request)
            && ReferenceEquals(authoring.Admission, admission)
            && ReferenceEquals(authoring.AuthorRequest, authorRequest)
            && ReferenceEquals(author.Request, authorRequest)
            && ReferenceEquals(author.CandidateBinding, binding)
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && moveModel.CallCount == 1
            && CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.PassesMainScene(
                scene,
                source,
                state,
                actionModel,
                moveModel);
    }
}
