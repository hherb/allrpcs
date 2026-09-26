using System.Numerics;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Discovery;

public sealed class OpeningRegionLayeredInscriptionBindingTests
{
    private static readonly Vector2 InscriptionPosition = new(110.0f, 90.0f);

    [Fact]
    public void RetainsOnlyTheExactNoticedShrineInscriptionFromTheSeekerScenario()
    {
        OpeningRegionOldShrineSceneBinding shrine = ShrineBinding();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject inscription = scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "shrine-inscription");
        WorldObjectVisibility visibility = scenario.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "shrine-inscription");

        OpeningRegionLayeredInscriptionBinding binding = new(
            shrine,
            scenario,
            inscription,
            visibility,
            InscriptionPosition);

        Assert.Same(shrine, binding.ShrineSceneBinding);
        Assert.Same(scenario, binding.DiscoveryScenario);
        Assert.Same(inscription, binding.WorldObject);
        Assert.Same(visibility, binding.InitialVisibility);
        Assert.Equal(InscriptionPosition, binding.InscriptionPosition);
    }

    [Fact]
    public void RejectsRebuiltForeignMissingAndWrongObjectInputs()
    {
        OpeningRegionOldShrineSceneBinding shrine = ShrineBinding();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject inscription = scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "shrine-inscription");
        WorldObjectVisibility visibility = scenario.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "shrine-inscription");
        DiscoveryScenario foreign = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject foreignInscription = foreign.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "shrine-inscription");
        WorldObject rebuilt = new(inscription.Identity, inscription.Description);
        WorldObject wrong = scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "garden-herb");
        DiscoveryScenario missing = With(
            scenario,
            worldObjects: new WorldObjectSnapshot(
                scenario.WorldObjects.Entries.Where(candidate =>
                    !ReferenceEquals(candidate, inscription))));

        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, rebuilt, visibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, foreignInscription, visibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, missing, inscription, visibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, wrong, visibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new WorldObjectSnapshot([inscription, inscription]));
    }

    [Fact]
    public void RejectsRebuiltForeignMissingWrongCharacterAndNonNoticedVisibility()
    {
        OpeningRegionOldShrineSceneBinding shrine = ShrineBinding();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject inscription = scenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "shrine-inscription");
        WorldObjectVisibility visibility = scenario.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "shrine-inscription");
        DiscoveryScenario foreign = RepresentativeDiscoveryScenarioSource.Create();
        WorldObjectVisibility foreignVisibility = foreign.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "shrine-inscription");
        WorldObjectVisibility rebuilt = new(visibility.ObjectId, visibility.VisibilityState);
        WorldObjectVisibility wrong = scenario.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "garden-herb");
        CharacterPerceptionSnapshot withoutVisibility = new(
            scenario.ObserverCharacterId,
            scenario.InitialPerception.Entries.Where(candidate =>
                !ReferenceEquals(candidate, visibility)));
        DiscoveryScenario missing = With(scenario, perception: withoutVisibility);
        CharacterPerceptionSnapshot wrongCharacterPerception = new(
            new CharacterId("travelling-healer"),
            scenario.InitialPerception.Entries);
        DiscoveryScenario wrongCharacter = With(scenario, perception: wrongCharacterPerception);
        WorldObjectVisibility identified = new(visibility.ObjectId, VisibilityState.Identified);

        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, inscription, rebuilt, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, inscription, foreignVisibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, missing, inscription, visibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, inscription, wrong, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, wrongCharacter, inscription, visibility, InscriptionPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionBinding(
            shrine, scenario, inscription, identified, InscriptionPosition));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpeningRegionLayeredInscriptionBinding(
                shrine,
                scenario,
                inscription,
                visibility,
                new Vector2(8.0f, 90.0f)));
        Assert.Throws<ArgumentException>(() => new CharacterPerceptionSnapshot(
            scenario.ObserverCharacterId,
            [visibility, visibility]));
    }

    private static OpeningRegionOldShrineSceneBinding ShrineBinding()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        return new OpeningRegionOldShrineSceneBinding(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
    }

    private static DiscoveryScenario With(
        DiscoveryScenario scenario,
        WorldObjectSnapshot? worldObjects = null,
        CharacterPerceptionSnapshot? perception = null) => new(
            scenario.ObserverCharacterId,
            worldObjects ?? scenario.WorldObjects,
            scenario.Evidence,
            perception ?? scenario.InitialPerception,
            scenario.Conditions,
            scenario.AvailableCapabilities,
            scenario.AvailableInventory,
            scenario.AvailableKnowledge,
            scenario.PassiveNoticingCandidates,
            scenario.ReinterpretationCandidates);
}
