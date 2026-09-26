using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact additive route-access token after background selection.</summary>
public partial class SeekerBackgroundRouteAccessSceneSource : Node
{
    /// <summary>Gets the exact selected route access, or null before customization.</summary>
    public SeekerBackgroundRouteAccess? CurrentAccess { get; private set; }

    /// <summary>Reports whether exact current route access can be installed once.</summary>
    public bool CanInstallAccess(
        SeekerBackgroundRouteVariationState? currentRouteState,
        SeekerBackgroundRouteAccess? access) =>
        CurrentAccess is null
        && currentRouteState?.SelectedVariation is not null
        && access is not null
        && ReferenceEquals(access.RouteState, currentRouteState)
        && ReferenceEquals(
            access.SelectedVariation,
            currentRouteState.SelectedVariation)
        && ReferenceEquals(
            access.DestinationAnchor,
            currentRouteState.SelectedVariation.DestinationAnchor);

    /// <summary>Installs one exact access token through compare-and-swap.</summary>
    public bool TryInstallAccess(
        SeekerBackgroundRouteVariationState? currentRouteState,
        SeekerBackgroundRouteAccess? access)
    {
        if (!CanInstallAccess(currentRouteState, access))
        {
            return false;
        }

        CurrentAccess = access;
        return true;
    }
}
