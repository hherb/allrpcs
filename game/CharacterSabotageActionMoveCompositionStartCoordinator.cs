using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Explicitly starts only one exact fresh admission-guarded Move request.</summary>
public partial class CharacterSabotageActionMoveCompositionStartCoordinator : Node
{
    private CharacterSabotageActionMoveCompositionCoordinator composition = null!;
    private TemporaryCharacterSabotageActionModelSource models = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <summary>Gets the exact fresh request successfully started by this coordinator.</summary>
    public CharacterSabotageActionConsiderationRequest? StartedRequest { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<CharacterSabotageActionMoveCompositionCoordinator>(
            "../CharacterSabotageActionMoveCompositionCoordinator");
        models = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../ScheduledTemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../ScheduledCharacterSabotageActionTaskController");
    }

    /// <summary>Starts once after revalidating the exact fresh request and full retained graph.</summary>
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
                "A preflighted fresh sabotage-action task must accept its exact request.");
        }

        StartedRequest = expectedRequest;
        return true;
    }
}
