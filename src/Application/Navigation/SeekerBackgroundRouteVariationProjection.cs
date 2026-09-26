using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Selects an optional route only from exact applied customization provenance.</summary>
public static class SeekerBackgroundRouteVariationProjection
{
    /// <summary>Returns the exact selected variation, or null before exact application.</summary>
    public static SeekerBackgroundRouteVariation? Project(
        SeekerCustomizationSelectionState customizationState,
        SeekerCustomizedCharacterApplication? appliedCustomization,
        OpeningRegionProjection openingRegion,
        IEnumerable<SeekerBackgroundRouteVariation> variations)
    {
        ArgumentNullException.ThrowIfNull(customizationState);
        ArgumentNullException.ThrowIfNull(openingRegion);
        ArgumentNullException.ThrowIfNull(variations);

        SeekerCustomizationProfile? profile = customizationState.SelectedProfile;
        if (profile is null
            || appliedCustomization is null
            || !ReferenceEquals(customizationState, appliedCustomization.SelectedState))
        {
            return null;
        }

        List<SeekerBackgroundRouteVariation> retainedVariations =
            ValidateCatalogue(customizationState.Options, openingRegion, variations);
        return retainedVariations.Single(
            variation => ReferenceEquals(variation.Background, profile.Background));
    }

    private static List<SeekerBackgroundRouteVariation> ValidateCatalogue(
        SeekerCustomizationOptions options,
        OpeningRegionProjection openingRegion,
        IEnumerable<SeekerBackgroundRouteVariation> variations)
    {
        List<SeekerBackgroundRouteVariation> retainedVariations = [];
        HashSet<SeekerBackgroundChoice> backgrounds = new(ReferenceEqualityComparer.Instance);

        foreach (SeekerBackgroundRouteVariation? variation in variations)
        {
            if (variation is null
                || !options.BackgroundChoices.Any(
                    choice => ReferenceEquals(choice, variation.Background))
                || !openingRegion.Anchors.Any(
                    anchor => ReferenceEquals(anchor, variation.DestinationAnchor))
                || !backgrounds.Add(variation.Background))
            {
                throw new ArgumentException(
                    "Route variations must uniquely bind exact choices and projection anchors.",
                    nameof(variations));
            }

            retainedVariations.Add(variation);
        }

        if (retainedVariations.Count != options.BackgroundChoices.Count)
        {
            throw new ArgumentException(
                "A route-variation catalogue requires one entry per exact background choice.",
                nameof(variations));
        }

        return retainedVariations;
    }
}
