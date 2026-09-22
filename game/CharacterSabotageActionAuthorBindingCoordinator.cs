using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one exact observed sabotage-action selection to its author output.</summary>
public partial class CharacterSabotageActionAuthorBindingCoordinator : Node
{
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionTaskController task = null!;
    private CharacterSabotageActionConsiderationResult? boundResult;

    /// <summary>Gets the exact selected action author's request, or null after abstention.</summary>
    public CharacterSabotageActionConsiderationRequest? SelectedAuthorRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
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
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
    }

    /// <summary>Binds one fresh exact observed selection or abstention at most once.</summary>
    public bool TryBindObservedSelection()
    {
        CharacterSabotageActionConsiderationResult? result = selection.ObservedResult;
        CharacterSabotageActionConsiderationRequest? request = composer.Request;
        IReadOnlyList<CharacterSabotageActionConsiderationRequest?>? composedOutputs =
            composer.ComposedAuthorOutputs;
        if (result is null
            || request is null
            || composedOutputs is null
            || boundResult is not null
            || task.Status != CharacterSabotageActionTaskStatus.Completed
            || !ReferenceEquals(task.Result, result)
            || !ReferenceEquals(result.Request, request)
            || !ExactOutputsRemainCurrent(request, composedOutputs))
        {
            return false;
        }

        CharacterSabotageActionKind? selectedKind = result.SelectedActionKind;
        if (selectedKind is null)
        {
            boundResult = result;
            return true;
        }

        int selectedIndex = IndexOfCanonicalKind(selectedKind);
        if (selectedIndex < 0 || composedOutputs[selectedIndex] is not { } selectedRequest)
        {
            return false;
        }

        SelectedAuthorRequest = selectedRequest;
        boundResult = result;
        return true;
    }

    private bool ExactOutputsRemainCurrent(
        CharacterSabotageActionConsiderationRequest request,
        IReadOnlyList<CharacterSabotageActionConsiderationRequest?> composedOutputs)
    {
        if (composedOutputs.Count != CharacterSabotageActionKind.All.Count)
        {
            return false;
        }

        int requestKindIndex = 0;
        for (int index = 0; index < composedOutputs.Count; index++)
        {
            CharacterSabotageActionConsiderationRequest? output = composedOutputs[index];
            if (!ReferenceEquals(authors[index].Request, output))
            {
                return false;
            }

            if (output is null)
            {
                continue;
            }

            CharacterSabotageActionKind kind = CharacterSabotageActionKind.All[index];
            if (!ReferenceEquals(output.Actor, request.Actor)
                || output.ActionKinds.Count != 1
                || !ReferenceEquals(output.ActionKinds[0], kind)
                || requestKindIndex >= request.ActionKinds.Count
                || !ReferenceEquals(request.ActionKinds[requestKindIndex], kind))
            {
                return false;
            }

            requestKindIndex++;
        }

        return requestKindIndex == request.ActionKinds.Count;
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
