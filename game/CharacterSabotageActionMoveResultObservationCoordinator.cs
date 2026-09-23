using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes one exact completed fresh generic Move result without acknowledging it.</summary>
public partial class CharacterSabotageActionMoveResultObservationCoordinator : Node
{
    private CharacterSabotageActionMoveCompositionStartCoordinator start = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact completed fresh generic Move result retained here.</summary>
    public CharacterSabotageActionConsiderationResult? ObservedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        start = GetNode<CharacterSabotageActionMoveCompositionStartCoordinator>(
            "../CharacterSabotageActionMoveCompositionStartCoordinator");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../ScheduledCharacterSabotageActionTaskController");
    }

    /// <summary>Observes once after revalidating the exact completed result and full graph.</summary>
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
