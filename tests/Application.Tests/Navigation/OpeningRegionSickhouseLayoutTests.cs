using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionSickhouseLayoutTests
{
    private const float MinimumPathClearancePixels = 24.0f;
    private const float EntranceWidthPixels = 64.0f;
    private const float WallThicknessPixels = 8.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds RoomBounds =
        new(824.0f, 680.0f, 1016.0f, 872.0f);
    private static readonly Vector2 EntranceCenter = new(920.0f, 680.0f);
    private static readonly Vector2 ExteriorApproach = new(920.0f, 640.0f);
    private static readonly Vector2 PatientApproach = new(920.0f, 820.0f);
    private static readonly Vector2[] Positions =
    [
        PatientApproach,
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
    public void CreatesNorthEntranceWithExactPatientApproachAndClearance()
    {
        OpeningRegionFirstPatientSceneBinding binding = Binding();

        OpeningRegionSickhouseLayout layout = Layout(binding);

        Assert.Same(binding, layout.FirstPatientBinding);
        Assert.Equal(RoomBounds, layout.RoomBounds);
        Assert.Equal(EntranceCenter, layout.EntranceCenter);
        Assert.Equal(EntranceWidthPixels, layout.EntranceWidthPixels);
        Assert.Equal(WallThicknessPixels, layout.WallThicknessPixels);
        Assert.Equal(ExteriorApproach, layout.ExteriorApproach);
        Assert.Equal(PatientApproach, layout.PatientApproach);
        Assert.Same(binding.SickhouseAnchor, layout.FirstPatientBinding.SickhouseAnchor);
        Assert.Equal(
            MinimumPathClearancePixels,
            layout.FirstPatientBinding.Projection.MinimumPathClearancePixels);
    }

    [Fact]
    public void RejectsNarrowEntranceAndNonExactPatientApproach()
    {
        OpeningRegionFirstPatientSceneBinding binding = Binding();

        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionSickhouseLayout(
            binding,
            RoomBounds,
            EntranceCenter,
            (MinimumPathClearancePixels * 2.0f) - 1.0f,
            WallThicknessPixels,
            ExteriorApproach,
            PatientApproach));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSickhouseLayout(
            binding,
            RoomBounds,
            EntranceCenter,
            EntranceWidthPixels,
            WallThicknessPixels,
            ExteriorApproach,
            PatientApproach + Vector2.One));
    }

    [Fact]
    public void RejectsInvalidRoomEntranceExteriorAndWallInputs()
    {
        OpeningRegionFirstPatientSceneBinding binding = Binding();

        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionSickhouseLayout(
            binding,
            new NavigationWorldBounds(824.0f, 680.0f, 1601.0f, 872.0f),
            EntranceCenter,
            EntranceWidthPixels,
            WallThicknessPixels,
            ExteriorApproach,
            PatientApproach));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSickhouseLayout(
            binding,
            RoomBounds,
            EntranceCenter + Vector2.UnitY,
            EntranceWidthPixels,
            WallThicknessPixels,
            ExteriorApproach,
            PatientApproach));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSickhouseLayout(
            binding,
            RoomBounds,
            EntranceCenter,
            EntranceWidthPixels,
            WallThicknessPixels,
            new Vector2(920.0f, 700.0f),
            PatientApproach));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionSickhouseLayout(
            binding,
            RoomBounds,
            EntranceCenter,
            EntranceWidthPixels,
            float.NaN,
            ExteriorApproach,
            PatientApproach));
    }

    [Fact]
    public void RoutePolicyAcceptsOnlyBoundedEntranceTravel()
    {
        OpeningRegionSickhouseLayout layout = Layout(Binding());

        Assert.True(OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            [ExteriorApproach, EntranceCenter, PatientApproach]));
        Assert.True(OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            [PatientApproach, EntranceCenter, ExteriorApproach]));
        Assert.True(OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            [
                PatientApproach,
                new Vector2(908.6829f, 704.0f),
                new Vector2(904.0f, 656.0f),
                new Vector2(896.0f, 646.50006f),
                new Vector2(864.0f, 608.5f),
                new Vector2(856.0f, 599.00006f),
                new Vector2(848.0f, 589.5f),
                new Vector2(840.0f, 580.00006f),
                new Vector2(824.0f, 561.00006f),
                new Vector2(800.0f, 532.50006f),
                new Vector2(776.0f, 504.0f),
                new Vector2(776.0f, 480.0f),
                new Vector2(776.0f, 466.0f),
                new Vector2(776.0f, 456.0f),
                new Vector2(776.0f, 450.0f),
            ]));
        Assert.False(OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            [new Vector2(800.0f, 760.0f), new Vector2(900.0f, 760.0f)]));
        Assert.False(OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            [PatientApproach, new Vector2(920.0f, 901.0f)]));
        Assert.False(OpeningRegionSickhouseRoutePolicy.IsAcceptable(
            layout,
            [ExteriorApproach]));
    }

    private static OpeningRegionSickhouseLayout Layout(
        OpeningRegionFirstPatientSceneBinding binding) => new(
        binding,
        RoomBounds,
        EntranceCenter,
        EntranceWidthPixels,
        WallThicknessPixels,
        ExteriorApproach,
        PatientApproach);

    private static OpeningRegionFirstPatientSceneBinding Binding()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();
        OpeningRegionProjection projection = new(
            topology,
            WorldBounds,
            MinimumPathClearancePixels,
            120.0f,
            anchors);
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        return new OpeningRegionFirstPatientSceneBinding(
            projection,
            projection.Anchors[0],
            context,
            new Vector2(920.0f, 760.0f),
            96.0f);
    }
}
