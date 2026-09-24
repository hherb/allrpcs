using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes one exact completed fifth-cycle generic Move result.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator
        start = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact completed fifth-cycle result retained here.</summary>
    public CharacterSabotageActionConsiderationResult? ObservedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        start = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../SuccessorCharacterSabotageActionTaskController");
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
}
