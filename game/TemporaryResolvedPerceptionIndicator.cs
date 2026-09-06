using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Presents whether one temporary greybox world object is exactly identified in an existing
/// perception snapshot.
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
    /// Presents the existing world object only when its retained visibility is the identified
    /// state.
    /// </summary>
    public void Present(
        WorldObject? targetWorldObject,
        CharacterPerceptionSnapshot? perception)
    {
        WorldObjectVisibility? visibility = targetWorldObject is null || perception is null
            ? null
            : CharacterPerceptionLookup.FindByObject(targetWorldObject.Identity.Id, perception);
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
