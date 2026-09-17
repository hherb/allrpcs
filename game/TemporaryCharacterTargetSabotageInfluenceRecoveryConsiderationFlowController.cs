using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit influence-recovery decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController
    : Node
{
    private TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
    }

    /// <summary>Starts one decision for the exact supplied request only when explicitly called.</summary>
    public bool TryStart(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle)
        {
            return false;
        }

        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
