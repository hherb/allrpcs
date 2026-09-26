using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Navigation;

/// <summary>Authors optional opening routes for the four representative Seeker backgrounds.</summary>
public static class RepresentativeSeekerBackgroundRouteVariationSource
{
    private const string ForesterBackgroundKey = "forester";
    private const string ScribeBackgroundKey = "scribe";
    private const string ApothecarysAssistantBackgroundKey = "apothecarys-assistant";
    private const string TravellerBackgroundKey = "traveller";
    private const string ClosedForestPathLocationId = "closed-deeper-forest-path";
    private const string OldShrineLocationId = "old-shrine";
    private const string HealerGardenLocationId = "healer-garden";
    private const string SouthernRoadLocationId = "southern-road";

    /// <summary>Creates one exact variation per background in representative background order.</summary>
    public static IReadOnlyList<SeekerBackgroundRouteVariation> Create(
        SeekerCustomizationOptions options,
        OpeningRegionProjection projection)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(projection);

        return Array.AsReadOnly(
        [
            Variation(
                options,
                projection,
                ForesterBackgroundKey,
                ClosedForestPathLocationId),
            Variation(options, projection, ScribeBackgroundKey, OldShrineLocationId),
            Variation(
                options,
                projection,
                ApothecarysAssistantBackgroundKey,
                HealerGardenLocationId),
            Variation(options, projection, TravellerBackgroundKey, SouthernRoadLocationId),
        ]);
    }

    private static SeekerBackgroundRouteVariation Variation(
        SeekerCustomizationOptions options,
        OpeningRegionProjection projection,
        string backgroundKey,
        string locationId)
    {
        SeekerBackgroundChoice background = options.BackgroundChoices.SingleOrDefault(
            choice => choice.Key == backgroundKey)
            ?? throw new ArgumentException(
                "Representative route variations require every authored background.",
                nameof(options));
        OpeningRegionAnchor anchor = projection.Anchors.SingleOrDefault(
            candidate => candidate.Location.Identity.Id.Value == locationId)
            ?? throw new ArgumentException(
                "Representative route variations require every destination anchor.",
                nameof(projection));
        return new SeekerBackgroundRouteVariation(background, anchor);
    }
}
