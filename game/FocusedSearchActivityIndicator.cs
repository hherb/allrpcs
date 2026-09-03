using Allrpcs.Application.Discovery;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Presents whether the Seeker has focused-search activity through one temporary scene indicator.
/// </summary>
public partial class FocusedSearchActivityIndicator : Node2D
{
    private Line2D indicator = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        indicator = GetNode<Line2D>("Indicator");
        indicator.Visible = false;
    }

    /// <summary>
    /// Presents the existing per-character activity decision by changing only indicator visibility.
    /// </summary>
    public void Present(
        CharacterId searcherCharacterId,
        FocusedSearchActivitySnapshot activities)
    {
        indicator.Visible = FocusedSearchActivityIndicatorPolicy.ShouldShow(
            searcherCharacterId,
            activities);
    }
}
