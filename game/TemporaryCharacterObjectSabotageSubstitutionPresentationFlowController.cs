using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates explicit substitution state transitions with exact scene presentation.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController : Node
{
    private TemporaryCharacterObjectSabotageSubstitutionFlowController flow = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;
    private CharacterObjectSabotageSubstitutionPresentationController presentation = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageSubstitutionSource source =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionSource");
        flow = GetNode<TemporaryCharacterObjectSabotageSubstitutionFlowController>(
            "../TemporaryCharacterObjectSabotageSubstitutionFlowController");
        state = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        presentation = GetNode<CharacterObjectSabotageSubstitutionPresentationController>(
            "../CharacterObjectSabotageSubstitutionPresentationController");
        CanvasItem originalBody = GetNode<CanvasItem>(
            "../InteractionCandidates/WaterCask/Body");
        CanvasItem replacementBody = GetNode<CanvasItem>(
            "../InteractionCandidates/WaterCask/SubstituteBody");

        if (state.Substitutions is not { } substitutions
            || !presentation.TryInitialize(
                source.Candidate.Substitution,
                originalBody,
                replacementBody,
                substitutions))
        {
            throw new InvalidOperationException(
                "The substitution presentation flow requires initialized exact state.");
        }
    }

    /// <summary>Explicitly applies and presents the exact authored substitution once.</summary>
    public bool TryApply()
    {
        if (state.Substitutions is not { } expectedSubstitutions
            || !ReferenceEquals(
                presentation.LastPresentedSubstitutions,
                expectedSubstitutions)
            || !flow.TryApply()
            || state.Substitutions is not { } currentSubstitutions)
        {
            return false;
        }

        return presentation.TryPresent(expectedSubstitutions, currentSubstitutions);
    }

    /// <summary>Explicitly recovers and presents the original through visible exact evidence.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Substitutions is not { } expectedSubstitutions
            || !ReferenceEquals(
                presentation.LastPresentedSubstitutions,
                expectedSubstitutions)
            || !flow.TryRecover(perception)
            || state.Substitutions is not { } currentSubstitutions)
        {
            return false;
        }

        return presentation.TryPresent(expectedSubstitutions, currentSubstitutions);
    }
}
