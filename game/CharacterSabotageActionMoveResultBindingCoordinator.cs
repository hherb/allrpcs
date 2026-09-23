using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds and acknowledges the exact observed fresh Move result and typed candidate.</summary>
public partial class CharacterSabotageActionMoveResultBindingCoordinator : Node
{
    private CharacterSabotageActionMoveCompositionCoordinator composition = null!;
    private CharacterSabotageActionMoveCompositionStartCoordinator start = null!;
    private CharacterSabotageActionMoveResultObservationCoordinator observation = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private CharacterSabotageActionTaskController task = null!;
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterSabotageActionConsiderationResult? boundResult;

    /// <summary>Gets the exact guarded follow-up Move candidate binding selected here.</summary>
    public CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
        CandidateBinding
    { get; private set; }

    /// <summary>Gets the exact bound result after successful acknowledgement.</summary>
    internal CharacterSabotageActionConsiderationResult? AcknowledgedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<CharacterSabotageActionMoveCompositionCoordinator>(
            "../CharacterSabotageActionMoveCompositionCoordinator");
        start = GetNode<CharacterSabotageActionMoveCompositionStartCoordinator>(
            "../CharacterSabotageActionMoveCompositionStartCoordinator");
        observation = GetNode<CharacterSabotageActionMoveResultObservationCoordinator>(
            "../CharacterSabotageActionMoveResultObservationCoordinator");
        moveAuthor = GetNode<CharacterSabotageActionMoveRequestController>(
            "../ScheduledCharacterSabotageActionMoveRequestController");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../ScheduledCharacterSabotageActionTaskController");
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
    }

    /// <summary>Binds once only from the exact observed result and current typed candidate.</summary>
    public bool TryBindObservedResult(CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (boundResult is not null
            || CandidateBinding is not null
            || !HasCurrentObservedGraph(expectedResult)
            || moveAuthor.CandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    candidateBinding
            || !ReferenceEquals(candidateBinding.Request, composition.AuthorRequest)
            || !ReferenceEquals(candidateBinding.ActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(candidateBinding.Candidate, source.FollowUpCandidate))
        {
            return false;
        }

        boundResult = expectedResult;
        CandidateBinding = candidateBinding;
        return true;
    }

    /// <summary>Acknowledges only the exact still-current result already bound here.</summary>
    public bool TryAcknowledgeBoundResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (!ReferenceEquals(boundResult, expectedResult)
            || !HasCurrentObservedGraph(expectedResult)
            || moveAuthor.CandidateBinding is not { } currentBinding
            || !ReferenceEquals(CandidateBinding, currentBinding)
            || !observation.TryAcknowledgeCompletedResult(expectedResult))
        {
            return false;
        }

        AcknowledgedResult = expectedResult;
        return true;
    }

    /// <summary>Checks the retained acknowledgement and candidate against their current graph.</summary>
    internal bool IsCurrentAcknowledgedBinding(
        CharacterSabotageActionConsiderationResult? expectedResult,
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
            expectedBinding) =>
        expectedResult is not null
        && expectedBinding is not null
        && ReferenceEquals(boundResult, expectedResult)
        && ReferenceEquals(AcknowledgedResult, expectedResult)
        && ReferenceEquals(CandidateBinding, expectedBinding)
        && observation.ObservedResult is null
        && task.Status == CharacterSabotageActionTaskStatus.Idle
        && task.Result is null
        && ReferenceEquals(expectedResult.Request, composition.Request)
        && ReferenceEquals(start.StartedRequest, expectedResult.Request)
        && ReferenceEquals(expectedResult.SelectedActionKind, CharacterSabotageActionKind.Move)
        && composition.IsCurrentRequest(expectedResult.Request)
        && ReferenceEquals(moveAuthor.CandidateBinding, expectedBinding)
        && ReferenceEquals(expectedBinding.Request, composition.AuthorRequest)
        && ReferenceEquals(expectedBinding.ActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(expectedBinding.Candidate, source.FollowUpCandidate);

    private bool HasCurrentObservedGraph(
        CharacterSabotageActionConsiderationResult? expectedResult) =>
        expectedResult is not null
        && ReferenceEquals(observation.ObservedResult, expectedResult)
        && ReferenceEquals(expectedResult.Request, composition.Request)
        && ReferenceEquals(expectedResult.SelectedActionKind, CharacterSabotageActionKind.Move)
        && start.IsCurrentCompletedResult(expectedResult);
}
