using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one exact observed fifth-cycle Move result to its author candidate.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator
        composition = null!;
    private CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator
        observation = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private CharacterSabotageActionTaskController task = null!;
    private TemporaryCharacterObjectSabotageMoveSource source = null!;

    /// <summary>Gets the exact observed fifth-cycle result bound here.</summary>
    internal CharacterSabotageActionConsiderationResult? BoundResult { get; private set; }

    /// <summary>Gets the exact fifth-cycle Move candidate binding selected here.</summary>
    public CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
        CandidateBinding
    { get; private set; }

    /// <summary>Gets the exact bound result retained after successful acknowledgement.</summary>
    internal CharacterSabotageActionConsiderationResult? AcknowledgedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator");
        observation = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator");
        moveAuthor = GetNode<CharacterSabotageActionMoveRequestController>(
            "../SuccessorCharacterSabotageActionMoveRequestController");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../SuccessorCharacterSabotageActionTaskController");
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
    }

    /// <summary>Binds once from only the exact observed result and current author candidate.</summary>
    public bool TryBindObservedResult(CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (BoundResult is not null
            || CandidateBinding is not null
            || !HasCurrentObservedGraph(expectedResult)
            || moveAuthor.CandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    candidateBinding
            || !ReferenceEquals(candidateBinding.Request, composition.AuthorRequest)
            || !ReferenceEquals(candidateBinding.ActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(candidateBinding.Candidate, source.SuccessorCandidate))
        {
            return false;
        }

        BoundResult = expectedResult;
        CandidateBinding = candidateBinding;
        return true;
    }

    /// <summary>Acknowledges only the exact still-current result already bound here.</summary>
    public bool TryAcknowledgeBoundResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (!IsCurrentBoundResult(expectedResult, CandidateBinding)
            || !observation.TryAcknowledgeCompletedResult(expectedResult))
        {
            return false;
        }

        AcknowledgedResult = expectedResult;
        return true;
    }

    /// <summary>Checks the retained result and author candidate against their current graph.</summary>
    internal bool IsCurrentBoundResult(
        CharacterSabotageActionConsiderationResult? expectedResult,
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
            expectedBinding) =>
        expectedResult is not null
        && expectedBinding is not null
        && ReferenceEquals(BoundResult, expectedResult)
        && ReferenceEquals(CandidateBinding, expectedBinding)
        && HasCurrentObservedGraph(expectedResult)
        && ReferenceEquals(moveAuthor.CandidateBinding, expectedBinding)
        && ReferenceEquals(expectedBinding.Request, composition.AuthorRequest)
        && ReferenceEquals(expectedBinding.ActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(expectedBinding.Candidate, source.SuccessorCandidate);

    /// <summary>Checks exact acknowledged provenance after the fifth task resets.</summary>
    internal bool IsRetainedAcknowledgedBinding(
        CharacterSabotageActionConsiderationResult? expectedResult,
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>?
            expectedBinding) =>
        expectedResult is not null
        && expectedBinding is not null
        && ReferenceEquals(BoundResult, expectedResult)
        && ReferenceEquals(AcknowledgedResult, expectedResult)
        && ReferenceEquals(CandidateBinding, expectedBinding)
        && observation.ObservedResult is null
        && task.Status == CharacterSabotageActionTaskStatus.Idle
        && task.Result is null
        && ReferenceEquals(expectedResult.Request, composition.Request)
        && ReferenceEquals(expectedResult.SelectedActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(moveAuthor.CandidateBinding, expectedBinding)
        && ReferenceEquals(expectedBinding.Request, composition.AuthorRequest)
        && ReferenceEquals(expectedBinding.ActionKind, CharacterSabotageActionKind.Move)
        && ReferenceEquals(expectedBinding.Candidate, source.SuccessorCandidate);

    private bool HasCurrentObservedGraph(
        CharacterSabotageActionConsiderationResult? expectedResult) =>
        expectedResult is not null
        && ReferenceEquals(observation.ObservedResult, expectedResult)
        && ReferenceEquals(task.Result, expectedResult)
        && task.Status == CharacterSabotageActionTaskStatus.Completed
        && ReferenceEquals(expectedResult.Request, composition.Request)
        && ReferenceEquals(expectedResult.SelectedActionKind, CharacterSabotageActionKind.Move)
        && composition.IsCurrentRequest(expectedResult.Request);
}
