using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes one exact completed continuation-cycle generic Move result.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator start = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact completed continuation result retained here.</summary>
    public CharacterSabotageActionConsiderationResult? ObservedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        start = GetNode<CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator>(
            "../CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../ContinuedCharacterSabotageActionTaskController");
    }

    /// <summary>Observes once after revalidating the exact completed result and retained graph.</summary>
    public bool TryObserveCompletedResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        CharacterSabotageActionConsiderationRequest? startedRequest = start.StartedRequest;
        if (ObservedResult is not null
            || startedRequest is null
            || !start.IsCurrentCompletedResult(expectedResult)
            || !ReferenceEquals(task.Result, expectedResult)
            || !ReferenceEquals(expectedResult!.Request.Actor, startedRequest.Actor)
            || expectedResult.SelectedActionKind is not { } selectedKind
            || !ReferenceEquals(selectedKind, CharacterSabotageActionKind.Move))
        {
            return false;
        }

        ObservedResult = expectedResult;
        return true;
    }

    /// <summary>Resets only the same exact completed result already retained here.</summary>
    internal bool TryAcknowledgeCompletedResult(
        CharacterSabotageActionConsiderationResult? expectedResult)
    {
        if (expectedResult is null
            || !ReferenceEquals(ObservedResult, expectedResult)
            || !start.IsCurrentCompletedResult(expectedResult)
            || !task.TryReset())
        {
            return false;
        }

        ObservedResult = null;
        return true;
    }
}
