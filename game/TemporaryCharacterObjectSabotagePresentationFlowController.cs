using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates explicit sabotage state transitions with exact scene presentation.</summary>
public partial class TemporaryCharacterObjectSabotagePresentationFlowController : Node
{
    private TemporaryCharacterObjectSabotageFlowController flow = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private CharacterObjectSabotageConcealmentPresentationController presentation = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageSource source =
            GetNode<TemporaryCharacterObjectSabotageSource>(
                "../TemporaryCharacterObjectSabotageSource");
        flow = GetNode<TemporaryCharacterObjectSabotageFlowController>(
            "../TemporaryCharacterObjectSabotageFlowController");
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        presentation = GetNode<CharacterObjectSabotageConcealmentPresentationController>(
            "../CharacterObjectSabotageConcealmentPresentationController");
        Node2D body = GetNode<Node2D>("../InteractionCandidates/WaterCask");

        if (state.Concealment is not { } concealment
            || !presentation.TryInitialize(
                source.Candidate.Evidence.ObjectId,
                body,
                concealment))
        {
            throw new InvalidOperationException(
                "The temporary sabotage presentation flow requires initialized exact state.");
        }
    }

    /// <summary>Explicitly applies and presents the exact authored concealment once.</summary>
    public bool TryApply()
    {
        if (state.Concealment is not { } expectedConcealment
            || !ReferenceEquals(
                presentation.LastPresentedConcealment,
                expectedConcealment)
            || !flow.TryApply()
            || state.Concealment is not { } currentConcealment)
        {
            return false;
        }

        return presentation.TryPresent(expectedConcealment, currentConcealment);
    }

    /// <summary>Explicitly recovers and presents the target through visible exact evidence.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Concealment is not { } expectedConcealment
            || !ReferenceEquals(
                presentation.LastPresentedConcealment,
                expectedConcealment)
            || !flow.TryRecover(perception)
            || state.Concealment is not { } currentConcealment)
        {
            return false;
        }

        return presentation.TryPresent(expectedConcealment, currentConcealment);
    }
}
