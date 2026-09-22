using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one decision for the exact current delay-recovery request.</summary>
public partial class CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController
    : Node
{
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController requests = null!;
    private TemporaryCharacterTargetSabotageDelayRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        requests = GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        modelSource = GetNode<TemporaryCharacterTargetSabotageDelayRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageDelayRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
    }

    /// <summary>Starts once for the exact current request while the recovery task is idle.</summary>
    public bool TryStart(CharacterTargetSabotageDelayRecoveryConsiderationRequest expectedRequest)
    {
        ArgumentNullException.ThrowIfNull(expectedRequest);
        if (!ReferenceEquals(requests.Request, expectedRequest)
            || task.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle)
        {
            return false;
        }

        ICharacterTargetSabotageDelayRecoveryDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, expectedRequest);
    }
}
