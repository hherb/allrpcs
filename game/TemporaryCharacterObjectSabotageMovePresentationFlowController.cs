using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates explicit move state transitions with exact scene presentation.</summary>
public partial class TemporaryCharacterObjectSabotageMovePresentationFlowController : Node
{
    private static readonly Vector2 DestinationScenePosition = new(1900.0f, 1640.0f);
    private TemporaryCharacterObjectSabotageMoveFlowController flow = null!;
    private CharacterObjectSabotageMoveController state = null!;
    private CharacterObjectSabotageMovePresentationController presentation = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterObjectSabotageMoveSource source =
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        flow = GetNode<TemporaryCharacterObjectSabotageMoveFlowController>(
            "../TemporaryCharacterObjectSabotageMoveFlowController");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        presentation = GetNode<CharacterObjectSabotageMovePresentationController>(
            "../CharacterObjectSabotageMovePresentationController");
        Node2D body = GetNode<Node2D>("../InteractionCandidates/WaterCask");

        if (state.Placements is not { } placements
            || !presentation.TryInitialize(
                source.Candidate.Destination.Occurrence.Target,
                body,
                source.SourceLocation,
                body.Position,
                source.Destination,
                DestinationScenePosition,
                placements))
        {
            throw new InvalidOperationException(
                "The temporary move presentation flow requires initialized exact state.");
        }
    }

    /// <summary>Explicitly applies and presents the exact authored move once.</summary>
    public bool TryApply()
    {
        if (state.Placements is not { } expectedPlacements
            || !ReferenceEquals(
                presentation.LastPresentedPlacements,
                expectedPlacements)
            || !flow.TryApply()
            || state.Placements is not { } currentPlacements)
        {
            return false;
        }

        return presentation.TryPresent(expectedPlacements, currentPlacements);
    }

    /// <summary>Explicitly recovers and presents the target through visible exact evidence.</summary>
    public bool TryRecover(CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        if (state.Placements is not { } expectedPlacements
            || !ReferenceEquals(
                presentation.LastPresentedPlacements,
                expectedPlacements)
            || !flow.TryRecover(perception)
            || state.Placements is not { } currentPlacements)
        {
            return false;
        }

        return presentation.TryPresent(expectedPlacements, currentPlacements);
    }
}
