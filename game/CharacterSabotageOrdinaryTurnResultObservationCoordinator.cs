using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes one exact completed generic result from ordinary-turn authoring.</summary>
public partial class CharacterSabotageOrdinaryTurnResultObservationCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring = null!;
    private CharacterSabotageOrdinaryTurnConsiderationStartCoordinator start = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionTaskController task = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;

    /// <summary>Gets the exact completed result retained by successful observation.</summary>
    public CharacterSabotageActionConsiderationResult? ObservedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authoring = GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
            "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        start = GetNode<CharacterSabotageOrdinaryTurnConsiderationStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnConsiderationStartCoordinator");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        authors =
        [
            GetAuthor("CharacterSabotageActionConcealmentRequestController"),
            GetAuthor("CharacterSabotageActionMoveRequestController"),
            GetAuthor("CharacterSabotageActionSubstitutionRequestController"),
            GetAuthor("CharacterSabotageActionContaminationRequestController"),
            GetAuthor("CharacterSabotageActionMisquotationRequestController"),
            GetAuthor("CharacterSabotageActionDelayRequestController"),
            GetAuthor("CharacterSabotageActionInfluenceRequestController"),
        ];
    }

    /// <summary>Observes once after revalidating the exact completed ordinary-turn graph.</summary>
    public bool TryObserveCompletedResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        IReadOnlyList<CharacterSabotageActionConsiderationRequest?>? outputs =
            authoring.AuthorOutputs;
        CharacterSabotageActionKind? selectedKind = expectedResult?.SelectedActionKind;
        int selectedIndex = selectedKind is null ? -1 : IndexOfCanonicalKind(selectedKind);
        if (ObservedResult is not null
            || selection.ObservedResult is not null
            || expectedResult is null
            || selectedKind is null
            || selectedIndex < 0
            || outputs is null
            || outputs.Count != authors.Length
            || !start.IsCurrentCompletedResult(expectedResult)
            || !ReferenceEquals(task.Result, expectedResult)
            || !HasCurrentSelectedAuthor(outputs, selectedIndex, selectedKind))
        {
            return false;
        }

        if (!selection.TryObserveCompletedResult()
            || !ReferenceEquals(selection.ObservedResult, expectedResult))
        {
            throw new InvalidOperationException(
                "A preflighted exact ordinary-turn result must be observed once.");
        }

        ObservedResult = expectedResult;
        return true;
    }

    private bool HasCurrentSelectedAuthor(
        IReadOnlyList<CharacterSabotageActionConsiderationRequest?> outputs,
        int selectedIndex,
        CharacterSabotageActionKind selectedKind)
    {
        CharacterSabotageActionConsiderationRequest? selectedRequest = outputs[selectedIndex];
        CharacterSabotageActionCandidateBinding? selectedBinding =
            authors[selectedIndex].CandidateBinding;
        return selectedRequest is not null
            && ReferenceEquals(authors[selectedIndex].Request, selectedRequest)
            && selectedBinding is not null
            && ReferenceEquals(selectedBinding.Request, selectedRequest)
            && ReferenceEquals(selectedBinding.ActionKind, selectedKind);
    }

    private static int IndexOfCanonicalKind(CharacterSabotageActionKind selectedKind)
    {
        for (int index = 0; index < CharacterSabotageActionKind.All.Count; index++)
        {
            if (ReferenceEquals(CharacterSabotageActionKind.All[index], selectedKind))
            {
                return index;
            }
        }

        return -1;
    }

    private ICharacterSabotageActionRequestAuthor GetAuthor(string nodeName)
    {
        Node node = GetNode<Node>($"../{nodeName}");
        return node as ICharacterSabotageActionRequestAuthor
            ?? throw new InvalidOperationException($"Node '{nodeName}' is not a request author.");
    }
}
