using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Creates and advances immutable optional background-route state.</summary>
public static class SeekerBackgroundRouteVariationStateTransition
{
    /// <summary>Creates one empty state over exact representative options and scene anchors.</summary>
    public static SeekerBackgroundRouteVariationState Initialize(
        SeekerCustomizationOptions options,
        OpeningRegionProjection openingRegion)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(openingRegion);
        IReadOnlyList<SeekerBackgroundRouteVariation> variations =
            RepresentativeSeekerBackgroundRouteVariationSource.Create(options, openingRegion);
        return new SeekerBackgroundRouteVariationState(
            openingRegion,
            variations,
            selectedVariation: null,
            previousState: null);
    }

    /// <summary>Returns one selected state only for exact applied customization provenance.</summary>
    public static SeekerBackgroundRouteVariationState Select(
        SeekerBackgroundRouteVariationState currentState,
        SeekerCustomizationSelectionState customizationState,
        SeekerCustomizedCharacterApplication? appliedCustomization)
    {
        ArgumentNullException.ThrowIfNull(currentState);
        ArgumentNullException.ThrowIfNull(customizationState);
        if (currentState.SelectedVariation is not null)
        {
            throw new InvalidOperationException(
                "A background route variation may be selected only once.");
        }

        SeekerBackgroundRouteVariation? selected =
            SeekerBackgroundRouteVariationProjection.Project(
                customizationState,
                appliedCustomization,
                currentState.OpeningRegion,
                currentState.Variations);
        if (selected is null)
        {
            throw new InvalidOperationException(
                "A background route variation requires exact applied customization.");
        }

        return new SeekerBackgroundRouteVariationState(
            currentState.OpeningRegion,
            currentState.Variations,
            selected,
            currentState);
    }
}
