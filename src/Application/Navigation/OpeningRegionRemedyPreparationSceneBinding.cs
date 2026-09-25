using System.Numerics;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact healer-garden anchor to the first remedy's preparation station.</summary>
public sealed class OpeningRegionRemedyPreparationSceneBinding
{
    private const string HealerGardenLocationId = "healer-garden";

    /// <summary>Creates one bounded station without starting or advancing preparation.</summary>
    public OpeningRegionRemedyPreparationSceneBinding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor gardenAnchor,
        FirstPatientTreatmentContext treatmentContext,
        Vector2 stationPosition,
        float maximumStationDistancePixels)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(gardenAnchor);
        ArgumentNullException.ThrowIfNull(treatmentContext);
        if (!float.IsFinite(maximumStationDistancePixels)
            || maximumStationDistancePixels <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumStationDistancePixels),
                maximumStationDistancePixels,
                "The remedy-preparation station distance must be finite and positive.");
        }

        if (!projection.Anchors.Any(anchor => ReferenceEquals(anchor, gardenAnchor))
            || gardenAnchor.Location.Identity.Id.Value != HealerGardenLocationId)
        {
            throw new ArgumentException(
                "The remedy-preparation scene requires the projection's exact healer-garden anchor.",
                nameof(gardenAnchor));
        }

        if (!FirstPatientScenarioStructuralPolicy.IsAcceptable(treatmentContext.Scenario))
        {
            throw new ArgumentException(
                "The remedy-preparation scene requires the exact structurally valid first remedy.",
                nameof(treatmentContext));
        }

        _ = NavigationRequestValidator.Create(
            projection.WorldBounds,
            gardenAnchor.Position,
            stationPosition);
        if (Vector2.Distance(gardenAnchor.Position, stationPosition)
            > maximumStationDistancePixels)
        {
            throw new ArgumentException(
                "The remedy-preparation station must remain inside the bounded garden scene.",
                nameof(stationPosition));
        }

        Projection = projection;
        GardenAnchor = gardenAnchor;
        TreatmentContext = treatmentContext;
        Recipe = treatmentContext.Scenario.Remedy.Recipe;
        StationPosition = stationPosition;
        MaximumStationDistancePixels = maximumStationDistancePixels;
    }

    /// <summary>Gets the exact opening-region projection containing the healer's garden.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact healer-garden anchor.</summary>
    public OpeningRegionAnchor GardenAnchor { get; }

    /// <summary>Gets the exact first-patient treatment context.</summary>
    public FirstPatientTreatmentContext TreatmentContext { get; }

    /// <summary>Gets the exact remedy recipe represented at the station.</summary>
    public IngredientPreparationRecipe Recipe { get; }

    /// <summary>Gets the bounded scene position of the preparation station.</summary>
    public Vector2 StationPosition { get; }

    /// <summary>Gets the largest accepted station-to-garden-anchor distance.</summary>
    public float MaximumStationDistancePixels { get; }
}
