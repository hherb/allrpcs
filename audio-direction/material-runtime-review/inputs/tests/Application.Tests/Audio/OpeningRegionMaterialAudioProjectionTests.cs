using System.Numerics;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Audio;

public sealed class OpeningRegionMaterialAudioProjectionTests
{
    [Theory]
    [InlineData("sickhouse", "B01")]
    [InlineData("healer-garden", "B01")]
    [InlineData("village-square", "B01")]
    [InlineData("village-edge", "B02")]
    [InlineData("stream", "B02")]
    [InlineData("abandoned-laundry", "B02")]
    [InlineData("woodland-clearing", "B03")]
    [InlineData("old-shrine", "B03")]
    [InlineData("closed-deeper-forest-path", "B03")]
    [InlineData("southern-road", "B04")]
    [InlineData("abandoned-wagon", "B04")]
    public void EveryAuthoredAnchorMapsToItsExactRegionalBed(string id, string clip)
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionEnvironmentalRegionSet regions = RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        Vector2 position = projection.Anchors.Single(anchor => anchor.Location.Identity.Id.Value == id).Position;
        Assert.Equal(clip, OpeningRegionMaterialAudioProjection.Select(projection, regions, position)!.ClipId);
        Assert.Equal(1, OpeningRegionMaterialAudioProjection.Select(projection, regions, position)!.Gain);
    }

    [Fact]
    public void MidpointUsesOrdinalIdAndReturnVisitNeverDependsOnMapHistory()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionEnvironmentalRegionSet regions = RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        Assert.Equal("B02", OpeningRegionMaterialAudioProjection.Select(projection, regions, new(1950, 900))!.ClipId);
        Vector2 village = new(2200, 900);
        MaterialAudioPresentation? first = OpeningRegionMaterialAudioProjection.Select(projection, regions, village);
        _ = OpeningRegionMaterialAudioProjection.Select(projection, regions, new(380, 300));
        Assert.Equal(first, OpeningRegionMaterialAudioProjection.Select(projection, regions, village));
    }

    [Fact]
    public void ForeignTopologyNullAndNonfiniteInputsFailQuietly()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionEnvironmentalRegionSet foreign = RepresentativeOpeningRegionEnvironmentalSource.Create(
            RepresentativeOpeningRegionSource.Create());
        Assert.Null(OpeningRegionMaterialAudioProjection.Select(projection, foreign, Vector2.Zero));
        Assert.Null(OpeningRegionMaterialAudioProjection.Select(null, foreign, Vector2.Zero));
        Assert.Null(OpeningRegionMaterialAudioProjection.Select(projection, null, Vector2.Zero));
        OpeningRegionEnvironmentalRegionSet regions = RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        Assert.Null(OpeningRegionMaterialAudioProjection.Select(projection, regions, new(float.NaN, 0)));
        Assert.Null(OpeningRegionMaterialAudioProjection.Select(projection, regions, new(0, float.PositiveInfinity)));
    }

    [Theory]
    [InlineData("inscription-revealed-sound", true)]
    [InlineData("unknown-sound", false)]
    public void CurrentSnapshotVariationHasExactAttenuationAndUnknownKeysHaveNoFallback(string key, bool known)
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionEnvironmentalRegionSet baseline = RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        EnvironmentalState sound = new(key, "Replacement test state.");
        OpeningRegionEnvironmentalRegionSet varied = new(projection.Topology,
            new EnvironmentalStateSet(baseline.EnvironmentalStates.Entries.Append(sound)),
            baseline.Regions.Select(region => region.Key == "woodland-edge"
                ? new OpeningRegionEnvironmentalRegion(region.Key, region.Locations, region.WeatherState, region.LightingState, sound)
                : region));
        MaterialAudioPresentation? selected = OpeningRegionMaterialAudioProjection.Select(projection, varied, new(380, 300));
        if (known)
        {
            Assert.Equal("B03", selected!.ClipId);
            Assert.Equal(Math.Pow(10, -3.0 / 20), selected.Gain, 10);
            Assert.DoesNotContain("inscription", selected.Caption);
        }
        else { Assert.Null(selected); }
        Assert.Equal(1, OpeningRegionMaterialAudioProjection.Select(projection, baseline, new(380, 300))!.Gain);
    }

    [Fact]
    public void RetargetingIsContinuousAndRepeatedSameTargetDoesNotRestartTheRamp()
    {
        MaterialAudioRamp ramp = new();
        MaterialAudioPresentation village = new(0, "B01", "rain", 1);
        ramp.Retarget(village);
        ramp.Advance(0.375);
        Assert.Equal(0.5, ramp.Gain(0), 10);
        ramp.Retarget(village);
        ramp.Advance(0.375);
        Assert.Equal(1, ramp.Gain(0), 10);
        ramp.Retarget(new(1, "B02", "water", 1));
        ramp.Advance(0.375);
        ramp.Retarget(new(2, "B03", "leaves", 1));
        Assert.Equal(0.5, ramp.Gain(0), 10);
        Assert.Equal(0.5, ramp.Gain(1), 10);
        ramp.Advance(0.375);
        Assert.Equal(0.25, ramp.Gain(0), 10);
        Assert.Equal(0.25, ramp.Gain(1), 10);
        Assert.Equal(0.5, ramp.Gain(2), 10);
        Assert.InRange(Enumerable.Range(0, 4).Sum(ramp.Gain), 0, 1);
        ramp.Retarget(null);
        ramp.Advance(1);
        Assert.Equal(0, Enumerable.Range(0, 4).Sum(ramp.Gain));
    }

    [Fact]
    public void InvalidRampInputsCannotAmplifyOrCorruptWeights()
    {
        MaterialAudioRamp ramp = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => ramp.Retarget(new(4, "bad", "bad", 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ramp.Retarget(new(0, "bad", "bad", 2)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ramp.Retarget(new(0, "bad", "bad", double.NaN)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ramp.Advance(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ramp.Advance(double.PositiveInfinity));
        Assert.Equal(0, Enumerable.Range(0, 4).Sum(ramp.Gain));
    }

    [Fact]
    public void OnlyExactGrindingStepOwnsFeedbackAndUserSilencePreventsLease()
    {
        IngredientPreparationRecipe recipe = RepresentativeFirstPatientScenarioSource.Create().Remedy.Recipe;
        Assert.True(OpeningRegionMaterialAudioProjection.IsGrinding(recipe, recipe.Plan.Steps[0]));
        Assert.False(OpeningRegionMaterialAudioProjection.IsGrinding(recipe, new(IngredientPreparationKind.Grinding)));
        Assert.False(OpeningRegionMaterialAudioProjection.IsGrinding(recipe, recipe.Plan.Steps[1]));
        AudioPreferences preferences = AudioPreferences.Defaults();
        Assert.True(OpeningRegionMaterialAudioProjection.EffectsAudible(preferences));
        foreach (AudioCategory channel in new[] { AudioCategory.Master, AudioCategory.Effects })
        {
            Assert.False(OpeningRegionMaterialAudioProjection.EffectsAudible(preferences.WithChannel(channel, 0, false)));
            Assert.False(OpeningRegionMaterialAudioProjection.EffectsAudible(preferences.WithChannel(channel, 1, true)));
        }
        Assert.True(OpeningRegionMaterialAudioProjection.EffectsAudible(preferences.WithChannel(AudioCategory.Ambience, 0, true)));
    }

    private static OpeningRegionProjection Projection()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        Vector2[] positions = [new(2640, 1450),
            new(2300, 1300),
            new(2200, 900),
            new(1700, 900),
            new(1800, 500),
            new(1500, 220),
            new(1000, 350),
            new(380, 300),
            new(1500, 1400),
            new(1900, 1640),
            new(650, 150)];
        string[] ids = ["sickhouse",
            "healer-garden",
            "village-square",
            "village-edge",
            "stream",
            "abandoned-laundry",
            "woodland-clearing",
            "old-shrine",
            "southern-road",
            "abandoned-wagon",
            "closed-deeper-forest-path"];
        return new(topology, new(0, 0, 3200, 1800), 24, 120,
            ids.Select((id, index) => new OpeningRegionAnchor(
                topology.Locations.Single(location => location.Identity.Id.Value == id), positions[index])));
    }
}
