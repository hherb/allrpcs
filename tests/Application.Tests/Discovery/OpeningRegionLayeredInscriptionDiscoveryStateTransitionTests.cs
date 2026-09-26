using System.Numerics;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Tests.Discovery;

public sealed class OpeningRegionLayeredInscriptionDiscoveryStateTransitionTests
{
    [Fact]
    public void InitializesFromAndAppliesOnlyTheExactResolutionOnce()
    {
        Graph graph = CreateGraph();
        OpeningRegionLayeredInscriptionDiscoveryState initial =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(graph.Inscription);

        Assert.Same(graph.Inscription, initial.InscriptionBinding);
        Assert.Same(graph.Scenario.InitialPerception, initial.Perception);
        Assert.Same(graph.Scenario.ReinterpretationCandidates, initial.Candidates);
        Assert.Null(initial.Resolution);

        OpeningRegionLayeredInscriptionDiscoveryState applied =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
                initial,
                graph.Resolution);

        Assert.NotSame(initial, applied);
        Assert.Same(graph.Inscription, applied.InscriptionBinding);
        Assert.Same(graph.Resolution.Result.Perception, applied.Perception);
        Assert.Same(graph.Resolution.Result.Candidates, applied.Candidates);
        Assert.Same(graph.Resolution, applied.Resolution);
        Assert.Same(graph.Scenario.InitialPerception, initial.Perception);
        Assert.Same(graph.Scenario.ReinterpretationCandidates, initial.Candidates);
        Assert.Null(initial.Resolution);
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
                applied,
                graph.Resolution));
    }

    [Fact]
    public void RejectsAResolutionFromAnotherExactGraph()
    {
        Graph graph = CreateGraph();
        Graph foreign = CreateGraph();
        OpeningRegionLayeredInscriptionDiscoveryState initial =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(graph.Inscription);

        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
                initial,
                foreign.Resolution));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(null!));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(null!, graph.Resolution));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(initial, null!));
    }

    [Fact]
    public void CreatesPresentationOnlyForTheExactAppliedDiscoveryState()
    {
        Graph graph = CreateGraph();
        OpeningRegionLayeredInscriptionDiscoveryState initial =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(graph.Inscription);
        OpeningRegionLayeredInscriptionDiscoveryState applied =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
                initial,
                graph.Resolution);

        OpeningRegionLayeredInscriptionPresentation presentation =
            OpeningRegionLayeredInscriptionPresentationTransition.Create(applied);

        Assert.Same(applied, presentation.DiscoveryState);
        Assert.Same(graph.Resolution, presentation.Resolution);
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionPresentationTransition.Create(initial));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionPresentationTransition.Create(null!));
    }

    [Fact]
    public void ExposesOnlyTheExactPartialInscriptionEvidenceAfterPresentation()
    {
        Graph graph = CreateGraph();
        OpeningRegionLayeredInscriptionDiscoveryState initial =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(graph.Inscription);
        CharacterEvidenceViewSnapshot initialView = DiscoveryEvidenceProjection.ForCharacter(
            graph.Scenario.Evidence,
            initial.Perception);
        OpeningRegionLayeredInscriptionDiscoveryState applied =
            OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
                initial,
                graph.Resolution);
        OpeningRegionLayeredInscriptionPresentation presentation =
            OpeningRegionLayeredInscriptionPresentationTransition.Create(applied);

        OpeningRegionLayeredInscriptionEvidenceExposure exposure =
            OpeningRegionLayeredInscriptionEvidenceExposureTransition.Create(presentation);

        Assert.Same(presentation, exposure.Presentation);
        Assert.Same(
            graph.Scenario.Evidence.Entries.Single(evidence =>
                evidence.EvidenceId.Value == "partial-inscription"),
            exposure.Evidence);
        Assert.Same(graph.Scenario.ObserverCharacterId, exposure.EvidenceView.CharacterId);
        Assert.Single(exposure.EvidenceView.EvidenceIds);
        Assert.Contains(
            exposure.Evidence.EvidenceId,
            exposure.EvidenceView.EvidenceIds);
        Assert.DoesNotContain(
            exposure.Evidence.EvidenceId,
            initialView.EvidenceIds);
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionEvidenceExposureTransition.Create(null!));
    }

    private static Graph CreateGraph()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
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
        Character seeker = CreateCharacter(scenario);
        OpeningRegionLayeredInscriptionResolution resolution =
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(candidate, seeker);
        return new Graph(scenario, inscription, resolution);
    }

    private static Character CreateCharacter(DiscoveryScenario scenario)
    {
        Allrpcs.Domain.Identifiers.CharacterId characterId = scenario.ObserverCharacterId;
        return new Character(
            new CharacterIdentity(characterId, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            scenario.AvailableKnowledge,
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }

    private sealed record Graph(
        DiscoveryScenario Scenario,
        OpeningRegionLayeredInscriptionBinding Inscription,
        OpeningRegionLayeredInscriptionResolution Resolution);
}
