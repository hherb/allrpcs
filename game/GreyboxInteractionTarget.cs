using Allrpcs.Application.Interaction;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Adapts one temporary authored scene marker to the pure interaction-focus candidate contract.
/// </summary>
public partial class GreyboxInteractionTarget : Node2D
{
    private CanvasItem focusIndicator = null!;

    /// <summary>Gets or sets the temporary player-facing label for this greybox marker.</summary>
    [Export]
    public string DisplayName { get; set; } = "GREYBOX TARGET";

    /// <summary>Gets or sets whether this marker is visible to interaction focus.</summary>
    [Export]
    public bool IsInteractionVisible { get; set; } = true;

    /// <summary>Gets or sets whether this visible marker is eligible for focus.</summary>
    [Export]
    public bool IsInteractionEnabled { get; set; } = true;

    /// <summary>Gets whether this marker currently presents the focus indicator.</summary>
    public bool IsFocused => GetFocusIndicator().Visible;

    /// <inheritdoc />
    public override void _Ready()
    {
        _ = GetFocusIndicator();
        Visible = IsInteractionVisible;
        SetFocused(false);
    }

    /// <summary>Creates the pure selection candidate for this authored marker.</summary>
    /// <returns>A finite-position candidate reflecting current presentation eligibility.</returns>
    public InteractionFocusCandidate ToFocusCandidate()
    {
        return new InteractionFocusCandidate(
            new NumericsVector2(GlobalPosition.X, GlobalPosition.Y),
            IsInteractionVisible,
            IsInteractionEnabled);
    }

    /// <summary>Shows or clears this marker's temporary focus indicator.</summary>
    /// <param name="isFocused">Whether the pure selector chose this marker.</param>
    public void SetFocused(bool isFocused)
    {
        GetFocusIndicator().Visible = isFocused && IsInteractionVisible && IsInteractionEnabled;
    }

    private CanvasItem GetFocusIndicator()
    {
        focusIndicator ??= GetNode<CanvasItem>("FocusIndicator");
        return focusIndicator;
    }
}
