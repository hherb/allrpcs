using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs the exact temporary authored move through scene-owned state on request.</summary>
public partial class TemporaryCharacterObjectSabotageMoveFlowController : Node
{
    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");

        if (!state.TryInitialize(source.Placements, source.Evidence))
        {
            throw new InvalidOperationException(
                "The temporary move flow requires uninitialized scene-owned state.");
        }
    }

    /// <summary>Attempts the exact authored move once from current exact scene state.</summary>
    public bool TryApply()
    {
        if (state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        return state.TryApply(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            source.Candidate);
    }

    /// <summary>Recovers the authored target only through evidence projected from perception.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterEvidenceViewSnapshot evidenceView =
            DiscoveryEvidenceProjection.ForCharacter(evidence, perception);
        return state.TryRecover(
            placements,
            evidence,
            evidenceView,
            source.RecoveryCandidate);
    }
}
