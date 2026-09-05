using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Presents whether one temporary greybox target is exactly identified in an existing perception
/// snapshot.
/// </summary>
public partial class TemporaryResolvedPerceptionIndicator : Node2D
{
    private CanvasItem indicator = null!;

    /// <summary>Gets whether this target's temporary identified marker is visible.</summary>
    public bool IsIndicatorVisible => GetIndicator().Visible;

    /// <inheritdoc />
    public override void _Ready()
    {
        GetIndicator().Visible = false;
    }

    /// <summary>
    /// Presents the exact target only when its retained visibility is the identified state.
    /// </summary>
    public void Present(
        ObjectId? targetObjectId,
        CharacterPerceptionSnapshot? perception)
    {
        WorldObjectVisibility? visibility = targetObjectId is null || perception is null
            ? null
            : CharacterPerceptionLookup.FindByObject(targetObjectId, perception);
        GetIndicator().Visible = ReferenceEquals(
            visibility?.VisibilityState,
            VisibilityState.Identified);
    }

    private CanvasItem GetIndicator()
    {
        indicator ??= GetNode<CanvasItem>("Indicator");
        return indicator;
    }
}
