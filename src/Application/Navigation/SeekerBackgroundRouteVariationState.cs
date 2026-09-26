namespace Allrpcs.Application.Navigation;

/// <summary>Retains one exact optional-route catalogue and its one-shot selection state.</summary>
public sealed class SeekerBackgroundRouteVariationState
{
    internal SeekerBackgroundRouteVariationState(
        OpeningRegionProjection openingRegion,
        IReadOnlyList<SeekerBackgroundRouteVariation> variations,
        SeekerBackgroundRouteVariation? selectedVariation,
        SeekerBackgroundRouteVariationState? previousState)
    {
        OpeningRegion = openingRegion;
        Variations = variations;
        SelectedVariation = selectedVariation;
        PreviousState = previousState;
    }

    /// <summary>Gets the exact opening-region projection used by every variation.</summary>
    public OpeningRegionProjection OpeningRegion { get; }

    /// <summary>Gets the exact representative route variations in background order.</summary>
    public IReadOnlyList<SeekerBackgroundRouteVariation> Variations { get; }

    /// <summary>Gets the exact selected optional variation, or null before customization.</summary>
    public SeekerBackgroundRouteVariation? SelectedVariation { get; }

    /// <summary>Gets the exact empty predecessor, or null for initial state.</summary>
    public SeekerBackgroundRouteVariationState? PreviousState { get; }
}
