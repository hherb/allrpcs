using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit sabotage-action decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterSabotageActionConsiderationFlowController : Node
{
    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
    }

    /// <summary>Starts one decision for the exact supplied request only when explicitly called.</summary>
    public bool TryStart(CharacterSabotageActionConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (task.Status != CharacterSabotageActionTaskStatus.Idle)
        {
            return false;
        }

        ICharacterSabotageActionDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
