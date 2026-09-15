using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs the exact temporary authored substitution through scene-owned state on request.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionFlowController : Node
{
    private TemporaryCharacterObjectSabotageSubstitutionSource source = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        state = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");

        if (!state.TryInitialize(source.Substitutions, source.Evidence))
        {
            throw new InvalidOperationException(
                "The temporary substitution flow requires uninitialized scene-owned state.");
        }
    }

    /// <summary>Attempts the exact authored substitution once from current exact scene state.</summary>
    public bool TryApply()
    {
        if (state.Substitutions is not { } substitutions
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        return state.TryApply(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            substitutions,
            evidence,
            source.Candidate);
    }

    /// <summary>Recovers the authored substitution only through evidence projected from perception.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Substitutions is not { } substitutions
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterEvidenceViewSnapshot evidenceView =
            DiscoveryEvidenceProjection.ForCharacter(evidence, perception);
        return state.TryRecover(
            substitutions,
            evidence,
            evidenceView,
            source.Candidate);
    }
}
