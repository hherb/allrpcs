using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionFirstPatientSceneBindingTests
{
    private const float MaximumPatientDistancePixels = 96.0f;
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
    public void BindsExactSickhouseAnchorAndDeepSleepingPatient()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor sickhouse = projection.Anchors[0];
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        Vector2 patientPosition = new(920.0f, 760.0f);

        OpeningRegionFirstPatientSceneBinding binding = new(
            projection,
            sickhouse,
            context,
            patientPosition,
            MaximumPatientDistancePixels);

        Assert.Same(projection, binding.Projection);
        Assert.Same(sickhouse, binding.SickhouseAnchor);
        Assert.Same(context, binding.TreatmentContext);
        Assert.Equal(patientPosition, binding.PatientPosition);
        Assert.Equal(MaximumPatientDistancePixels, binding.MaximumPatientDistancePixels);
        Assert.Contains(
            context.Scenario.DiseaseTruth.Infections,
            infection => ReferenceEquals(infection.CharacterId, context.Scenario.PatientId)
                && ReferenceEquals(infection.Stage, DiseaseStage.DeepSleep));
    }

    [Fact]
    public void RejectsRebuiltAndNonSickhouseAnchors()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor sickhouse = projection.Anchors[0];
        OpeningRegionAnchor rebuilt = new(sickhouse.Location, sickhouse.Position);
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<ArgumentException>(() => Binding(projection, rebuilt, context));
        Assert.Throws<ArgumentException>(() => Binding(projection, projection.Anchors[1], context));
    }

    [Fact]
    public void RejectsDistantOutOfBoundsAndInvalidDistanceInputs()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor sickhouse = projection.Anchors[0];
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<ArgumentException>(() => new OpeningRegionFirstPatientSceneBinding(
            projection,
            sickhouse,
            context,
            new Vector2(920.0f, 700.0f),
            MaximumPatientDistancePixels));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionFirstPatientSceneBinding(
            projection,
            sickhouse,
            context,
            new Vector2(920.0f, 901.0f),
            MaximumPatientDistancePixels));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionFirstPatientSceneBinding(
            projection,
            sickhouse,
            context,
            new Vector2(920.0f, 760.0f),
            0.0f));
    }

    private static OpeningRegionFirstPatientSceneBinding Binding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        FirstPatientTreatmentContext context) => new(
            projection,
            anchor,
            context,
            new Vector2(920.0f, 760.0f),
            MaximumPatientDistancePixels);

    private static OpeningRegionProjection Projection()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();
        return new OpeningRegionProjection(topology, WorldBounds, 24.0f, 120.0f, anchors);
    }
}
