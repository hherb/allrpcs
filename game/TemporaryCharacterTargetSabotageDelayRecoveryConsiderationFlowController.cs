using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one explicit delay-recovery decision through exact scene dependencies.</summary>
public partial class TemporaryCharacterTargetSabotageDelayRecoveryConsiderationFlowController
    : Node
{
    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
    }

    /// <summary>Starts one decision for the exact supplied request only when explicitly called.</summary>
    public bool TryStart(CharacterTargetSabotageDelayRecoveryConsiderationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle)
        {
            return false;
        }

        ICharacterTargetSabotageDelayRecoveryDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, request);
    }
}
