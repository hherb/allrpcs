using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one decision for the exact current recovery-candidate request.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController
    : Node
{
    private CharacterTargetSabotageInfluenceRecoveryCandidateRequestController requests = null!;
    private TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        requests = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
    }

    /// <summary>Starts once for the exact current request while the recovery task is idle.</summary>
    public bool TryStart(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest expectedRequest)
    {
        ArgumentNullException.ThrowIfNull(expectedRequest);
        if (!ReferenceEquals(requests.Request, expectedRequest)
            || task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle)
        {
            return false;
        }

        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model = modelSource.TakeModel();
        return task.TryStart(model, expectedRequest);
    }
}
