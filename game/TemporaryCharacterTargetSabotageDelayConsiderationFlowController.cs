using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit target-delay decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageDelayConsiderationFlowController : Node
{
    private TemporaryCharacterTargetSabotageDelayModelSource modelSource = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayModelSource>(
            "../TemporaryCharacterTargetSabotageDelayModelSource");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
    }

    /// <summary>Starts one decision for the exact supplied request only when explicitly called.</summary>
    public bool TryStart(CharacterTargetSabotageDelayConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (task.Status != CharacterTargetSabotageDelayTaskStatus.Idle)
        {
            return false;
        }

        ICharacterTargetSabotageDelayDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
