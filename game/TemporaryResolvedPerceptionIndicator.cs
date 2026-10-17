using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Presents one temporary greybox world object's authored description only when it is exactly
/// identified in an existing perception snapshot.
/// </summary>
public partial class TemporaryResolvedPerceptionIndicator : Node2D
{
    private Label indicator = null!;

    /// <summary>Gets whether this target's temporary resolved-description label is visible.</summary>
    public bool IsIndicatorVisible => GetIndicator().Visible;

    /// <inheritdoc />
    public override void _Ready()
    {
        Label resolvedIndicator = GetIndicator();
        resolvedIndicator.Text = string.Empty;
        resolvedIndicator.Visible = false;
    }

    /// <summary>
    /// Presents the existing world object's exact authored description only when its retained
    /// visibility is the identified state.
    /// </summary>
    public void Present(
        WorldObject? targetWorldObject,
        CharacterPerceptionSnapshot? perception)
    {
        WorldObjectVisibility? visibility = targetWorldObject is null || perception is null
            ? null
            : CharacterPerceptionLookup.FindByObject(targetWorldObject.Identity.Id, perception);
        string? presentedDescription = ReferenceEquals(
                visibility?.VisibilityState,
                VisibilityState.Identified)
            ? targetWorldObject?.Description.Text
            : null;
        Label resolvedIndicator = GetIndicator();
        resolvedIndicator.Text = presentedDescription ?? string.Empty;
        resolvedIndicator.Visible = presentedDescription is not null;
    }

    private Label GetIndicator()
    {
        indicator ??= GetNode<Label>("Indicator");
        return indicator;
    }
}
