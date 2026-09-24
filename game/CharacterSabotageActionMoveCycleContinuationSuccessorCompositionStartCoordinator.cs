using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Explicitly starts only one exact fifth-cycle generic Move request.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator
        composition = null!;
    private TemporaryCharacterSabotageActionModelSource models = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact fifth-cycle request successfully started by this coordinator.</summary>
    public CharacterSabotageActionConsiderationRequest? StartedRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator");
        models = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../SuccessorTemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../SuccessorCharacterSabotageActionTaskController");
    }

    /// <summary>Starts once after revalidating the exact request and complete retained graph.</summary>
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
                "A preflighted fifth-cycle sabotage-action task must accept its exact request.");
        }

        StartedRequest = expectedRequest;
        return true;
    }

    /// <summary>Checks one exact completed result against the retained fifth-cycle graph.</summary>
    internal bool IsCurrentCompletedResult(
        CharacterSabotageActionConsiderationResult? expectedResult) =>
        StartedRequest is not null
        && expectedResult is not null
        && task.Status == CharacterSabotageActionTaskStatus.Completed
        && ReferenceEquals(task.Result, expectedResult)
        && ReferenceEquals(expectedResult.Request, StartedRequest)
        && composition.IsCurrentRequest(StartedRequest);
}
