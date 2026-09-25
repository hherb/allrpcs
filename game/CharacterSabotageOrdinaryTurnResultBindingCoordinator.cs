using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds and acknowledges one exact observed ordinary-turn generic result.</summary>
public partial class CharacterSabotageOrdinaryTurnResultBindingCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring = null!;
    private CharacterSabotageOrdinaryTurnConsiderationStartCoordinator start = null!;
    private CharacterSabotageOrdinaryTurnResultObservationCoordinator observation = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact observed result retained by successful binding.</summary>
    public CharacterSabotageActionConsiderationResult? BoundResult { get; private set; }

    /// <summary>Gets the selected author's exact request retained by successful binding.</summary>
    public CharacterSabotageActionConsiderationRequest? SelectedAuthorRequest { get; private set; }

    /// <summary>Gets the selected author's exact candidate retained by successful binding.</summary>
    public CharacterSabotageActionCandidateBinding? SelectedCandidateBinding { get; private set; }

    /// <summary>Gets the exact bound result acknowledged through the existing task owner.</summary>
    public CharacterSabotageActionConsiderationResult? AcknowledgedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authoring = GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
            "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        start = GetNode<CharacterSabotageOrdinaryTurnConsiderationStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnConsiderationStartCoordinator");
        observation = GetNode<CharacterSabotageOrdinaryTurnResultObservationCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultObservationCoordinator");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
    }

    /// <summary>Binds one exact observed selection to its unchanged author provenance.</summary>
    public bool TryBindObservedResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (BoundResult is not null
            || SelectedAuthorRequest is not null
            || SelectedCandidateBinding is not null
            || expectedResult is null
            || !ReferenceEquals(observation.ObservedResult, expectedResult)
            || !ReferenceEquals(selection.ObservedResult, expectedResult)
            || !start.IsCurrentCompletedResult(expectedResult)
            || !authoring.IsCurrent(start.Turn, start.Request)
            || !ReferenceEquals(task.Result, expectedResult))
        {
            return false;
        }

        if (!binding.TryBindObservedSelection()
            || binding.SelectedAuthorRequest is not { } selectedRequest
            || binding.SelectedCandidateBinding is not { } selectedCandidate
            || !ReferenceEquals(selectedCandidate.Request, selectedRequest)
            || !ReferenceEquals(selectedCandidate.ActionKind, expectedResult.SelectedActionKind))
        {
            throw new InvalidOperationException(
                "A preflighted selected ordinary-turn result must bind exact author provenance.");
        }

        BoundResult = expectedResult;
        SelectedAuthorRequest = selectedRequest;
        SelectedCandidateBinding = selectedCandidate;
        return true;
    }

    /// <summary>Acknowledges only the same exact result with unchanged bound provenance.</summary>
    public bool TryAcknowledgeBoundResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (expectedResult is null
            || AcknowledgedResult is not null
            || !ReferenceEquals(BoundResult, expectedResult)
            || !ReferenceEquals(observation.ObservedResult, expectedResult)
            || !ReferenceEquals(selection.ObservedResult, expectedResult)
            || !start.IsCurrentCompletedResult(expectedResult)
            || !authoring.IsCurrent(start.Turn, start.Request)
            || !ReferenceEquals(binding.SelectedAuthorRequest, SelectedAuthorRequest)
            || !ReferenceEquals(binding.SelectedCandidateBinding, SelectedCandidateBinding))
        {
            return false;
        }

        if (!binding.TryAcknowledgeBoundResult()
            || !ReferenceEquals(binding.AcknowledgedResult, expectedResult)
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || task.Result is not null
            || selection.ObservedResult is not null)
        {
            throw new InvalidOperationException(
                "A preflighted bound ordinary-turn result must acknowledge exactly once.");
        }

        AcknowledgedResult = expectedResult;
        return true;
    }
}
