using System.Numerics;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.EnvironmentalStates;

namespace Allrpcs.Application.Tests.EnvironmentalStates;

public sealed class OpeningRegionLayeredInscriptionEnvironmentalVariationTransitionTests
{
    [Fact]
    public void ReplacesOnlyWoodlandLightAndSoundAfterExactPresentation()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionEnvironmentalRegionSet baseline =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        OpeningRegionLayeredInscriptionPresentation presentation = Presentation(projection);
        EnvironmentalState revealedLighting = new(
            "inscription-revealed-light",
            "The revealed inscription catches a restrained greybox highlight.");
        EnvironmentalState revealedSound = new(
            "inscription-revealed-sound",
            "The woodland rain thins around the revealed inscription moment.");

        OpeningRegionLayeredInscriptionEnvironmentalVariation variation =
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                baseline,
                presentation,
                revealedLighting,
                revealedSound);

        Assert.Same(baseline, variation.BaselineRegions);
        Assert.Same(presentation, variation.Presentation);
        Assert.Same(revealedLighting, variation.RevealedLightingState);
        Assert.Same(revealedSound, variation.RevealedSoundState);
        Assert.Same(projection.Topology, variation.VariedRegions.Topology);
        Assert.Equal(baseline.Regions.Count, variation.VariedRegions.Regions.Count);
        OpeningRegionEnvironmentalRegion baselineWoodland = baseline.Regions.Single(region =>
            region.Key == "woodland-edge");
        OpeningRegionEnvironmentalRegion variedWoodland =
            variation.VariedRegions.Regions.Single(region => region.Key == "woodland-edge");
        Assert.NotSame(baselineWoodland, variedWoodland);
        Assert.Equal(baselineWoodland.Locations, variedWoodland.Locations);
        Assert.Same(baselineWoodland.WeatherState, variedWoodland.WeatherState);
        Assert.Same(revealedLighting, variedWoodland.LightingState);
        Assert.Same(revealedSound, variedWoodland.SoundState);
        Assert.All(
            baseline.Regions.Where(region => region.Key != "woodland-edge"),
            region => Assert.Same(
                region,
                variation.VariedRegions.Regions.Single(candidate =>
                    candidate.Key == region.Key)));
    }

    [Fact]
    public void RejectsForeignTopologyAndCollidingVariationKeys()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionEnvironmentalRegionSet baseline =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        OpeningRegionLayeredInscriptionPresentation presentation = Presentation(projection);
        OpeningRegionLayeredInscriptionPresentation foreignPresentation = Presentation(
            OpeningRegionOldShrineSceneBindingTests.Projection());
        EnvironmentalState lighting = new("revealed-light", "A restrained revealed light.");
        EnvironmentalState sound = new("revealed-sound", "A restrained revealed sound.");
        OpeningRegionEnvironmentalRegionSet missingWoodland = new(
            baseline.Topology,
            baseline.EnvironmentalStates,
            [new OpeningRegionEnvironmentalRegion(
                "other-region",
                baseline.Topology.Locations,
                baseline.EnvironmentalStates.Entries[0],
                baseline.EnvironmentalStates.Entries[1],
                baseline.EnvironmentalStates.Entries[2])]);

        Assert.Throws<ArgumentException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                baseline,
                foreignPresentation,
                lighting,
                sound));
        Assert.Throws<ArgumentException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                missingWoodland,
                presentation,
                lighting,
                sound));
        Assert.Throws<ArgumentException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                baseline,
                presentation,
                new EnvironmentalState("steady-rain", "A duplicate baseline key."),
                sound));
        Assert.Throws<ArgumentException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                baseline,
                presentation,
                lighting,
                new EnvironmentalState(lighting.Key, "A duplicate variation key.")));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionEnvironmentalRegionSet baseline =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projection.Topology);
        OpeningRegionLayeredInscriptionPresentation presentation = Presentation(projection);
        EnvironmentalState lighting = new("revealed-light", "A restrained revealed light.");
        EnvironmentalState sound = new("revealed-sound", "A restrained revealed sound.");

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                null!,
                presentation,
                lighting,
                sound));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                baseline,
                null!,
                lighting,
                sound));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                baseline,
                presentation,
                null!,
                sound));
    }

    private static OpeningRegionLayeredInscriptionPresentation Presentation(
        OpeningRegionProjection projection)
    {
        OpeningRegionOldShrineSceneBinding shrine = new(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionLayeredInscriptionBinding inscription = new(
            shrine,
            scenario,
            scenario.WorldObjects.Entries.Single(candidate =>
                candidate.Identity.Id.Value == "shrine-inscription"),
            scenario.InitialPerception.Entries.Single(candidate =>
                candidate.ObjectId.Value == "shrine-inscription"),
            new Vector2(110.0f, 90.0f));
        OpeningRegionLayeredInscriptionAccess access = new(
            inscription,
            scenario.ObserverCharacterId,
            new Vector2(110.0f, 128.0f));
        OpeningRegionLayeredInscriptionCandidateBinding candidate = new(
            access,
            scenario.ReinterpretationCandidates.Entries.Single(),
            scenario.AvailableKnowledge.KnowledgeReferences.Single());
        Character seeker = Character(scenario);
        OpeningRegionLayeredInscriptionResolution resolution =
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(candidate, seeker);
        OpeningRegionLayeredInscriptionDiscoveryState initial =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(inscription);
        OpeningRegionLayeredInscriptionDiscoveryState applied =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(initial, resolution);
        return OpeningRegionLayeredInscriptionPresentationTransition.Create(applied);
    }

    private static Character Character(DiscoveryScenario scenario)
    {
        Allrpcs.Domain.Identifiers.CharacterId id = scenario.ObserverCharacterId;
        return new Character(
            new CharacterIdentity(id, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            scenario.AvailableKnowledge,
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }
}
