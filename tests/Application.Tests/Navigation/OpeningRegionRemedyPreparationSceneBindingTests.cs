using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionRemedyPreparationSceneBindingTests
{
    private const float MaximumStationDistancePixels = 72.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly Vector2[] Positions =
    [
        new(920.0f, 820.0f),
        new(880.0f, 620.0f),
        new(880.0f, 400.0f),
        new(650.0f, 400.0f),
        new(808.0f, 260.0f),
        new(700.0f, 60.0f),
        new(470.0f, 70.0f),
        new(160.0f, 70.0f),
        new(620.0f, 620.0f),
        new(720.0f, 820.0f),
        new(120.0f, 620.0f),
    ];

    [Fact]
    public void BindsExactGardenAnchorRecipeAndBoundedStation()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor garden = projection.Anchors[1];
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        Vector2 stationPosition = new(920.0f, 620.0f);

        OpeningRegionRemedyPreparationSceneBinding binding = new(
            projection,
            garden,
            context,
            stationPosition,
            MaximumStationDistancePixels);

        Assert.Same(projection, binding.Projection);
        Assert.Same(garden, binding.GardenAnchor);
        Assert.Same(context, binding.TreatmentContext);
        Assert.Same(context.Scenario.Remedy.Recipe, binding.Recipe);
        Assert.Equal(stationPosition, binding.StationPosition);
        Assert.Equal(MaximumStationDistancePixels, binding.MaximumStationDistancePixels);
    }

    [Fact]
    public void RejectsRebuiltAndNonGardenAnchors()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor garden = projection.Anchors[1];
        OpeningRegionAnchor rebuilt = new(garden.Location, garden.Position);
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<ArgumentException>(() => Binding(projection, rebuilt, context));
        Assert.Throws<ArgumentException>(() => Binding(projection, projection.Anchors[0], context));
    }

    [Fact]
    public void RejectsDistantOutOfBoundsAndInvalidDistanceInputs()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor garden = projection.Anchors[1];
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<ArgumentException>(() => new OpeningRegionRemedyPreparationSceneBinding(
            projection,
            garden,
            context,
            new Vector2(960.0f, 700.0f),
            MaximumStationDistancePixels));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionRemedyPreparationSceneBinding(
            projection,
            garden,
            context,
            new Vector2(1601.0f, 620.0f),
            MaximumStationDistancePixels));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionRemedyPreparationSceneBinding(
            projection,
            garden,
            context,
            new Vector2(920.0f, 620.0f),
            float.PositiveInfinity));
    }

    private static OpeningRegionRemedyPreparationSceneBinding Binding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        FirstPatientTreatmentContext context) => new(
        projection,
        anchor,
        context,
        new Vector2(920.0f, 620.0f),
        MaximumStationDistancePixels);

    private static OpeningRegionProjection Projection()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();
        return new OpeningRegionProjection(topology, WorldBounds, 24.0f, 120.0f, anchors);
    }
}
