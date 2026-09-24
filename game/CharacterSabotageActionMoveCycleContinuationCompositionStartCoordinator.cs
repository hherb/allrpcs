using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Explicitly starts only one exact continuation-guarded Move request.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationCompositionStartCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationCompositionCoordinator composition = null!;
    private TemporaryCharacterSabotageActionModelSource models = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact fresh request successfully started by this coordinator.</summary>
    public CharacterSabotageActionConsiderationRequest? StartedRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<CharacterSabotageActionMoveCycleContinuationCompositionCoordinator>(
            "../CharacterSabotageActionMoveCycleContinuationCompositionCoordinator");
        models = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../ContinuedTemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../ContinuedCharacterSabotageActionTaskController");
    }

    /// <summary>Starts once after revalidating the exact request and retained graph.</summary>
    public bool TryStart(CharacterSabotageActionConsiderationRequest? expectedRequest)
    {
        if (StartedRequest is not null
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || !composition.IsCurrentRequest(expectedRequest))
        {
            return false;
        }

        ICharacterSabotageActionDecisionModel model = models.TakeModel();
        if (!task.TryStart(model, expectedRequest!))
        {
            throw new InvalidOperationException(
                "A preflighted continuation task must accept its exact request.");
        }

        StartedRequest = expectedRequest;
        return true;
    }

    /// <summary>Checks one exact completed result against the retained start graph.</summary>
    internal bool IsCurrentCompletedResult(
        CharacterSabotageActionConsiderationResult? expectedResult) =>
        StartedRequest is not null
        && expectedResult is not null
        && task.Status == CharacterSabotageActionTaskStatus.Completed
        && ReferenceEquals(task.Result, expectedResult)
        && ReferenceEquals(expectedResult.Request, StartedRequest)
        && composition.IsCurrentRequest(StartedRequest);
}
