using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit influence decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceConsiderationFlowController : Node
{
    private TemporaryCharacterTargetSabotageInfluenceModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceTaskController>(
            "../CharacterTargetSabotageInfluenceTaskController");
    }

    /// <summary>Starts one decision for the exact supplied request only when explicitly called.</summary>
    public bool TryStart(CharacterTargetSabotageInfluenceConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (task.Status != CharacterTargetSabotageInfluenceTaskStatus.Idle)
        {
            return false;
        }

        ICharacterTargetSabotageInfluenceDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
