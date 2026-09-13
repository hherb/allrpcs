using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs the exact temporary authored concealment through scene-owned state on request.</summary>
public partial class TemporaryCharacterObjectSabotageFlowController : Node
{
    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController state = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");

        if (!state.TryInitialize(source.Concealment, source.Evidence))
        {
            throw new InvalidOperationException(
                "The temporary sabotage flow requires uninitialized scene-owned state.");
        }
    }

    /// <summary>Attempts the exact authored concealment once from current exact scene state.</summary>
    public bool TryApply()
    {
        if (state.Concealment is not { } concealment
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        return state.TryApplyFirstEligible(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            concealment,
            evidence,
            source.Candidates);
    }

    /// <summary>Recovers the authored target only through evidence projected from perception.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Concealment is not { } concealment
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterEvidenceViewSnapshot evidenceView =
            DiscoveryEvidenceProjection.ForCharacter(evidence, perception);
        return state.TryRecover(
            concealment,
            evidence,
            evidenceView,
            source.Candidate);
    }
}
