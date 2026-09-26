using System.Numerics;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Tests.Discovery;

public sealed class OpeningRegionLayeredInscriptionResolutionTransitionTests
{
    [Fact]
    public void ResolvesExactAccessibleCandidateWithoutChangingInputs()
    {
        OpeningRegionLayeredInscriptionCandidateBinding binding = CandidateBinding();
        DiscoveryScenario scenario = binding.Access.InscriptionBinding.DiscoveryScenario;
        Character seeker = Character(
            scenario.ObserverCharacterId,
            scenario.AvailableKnowledge);

        OpeningRegionLayeredInscriptionResolution resolution =
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(binding, seeker);

        Assert.Same(binding, resolution.CandidateBinding);
        Assert.Same(seeker, resolution.Seeker);
        Assert.Same(
            scenario.InitialPerception.CharacterId,
            resolution.Result.Perception.CharacterId);
        Assert.Empty(resolution.Result.Candidates.Entries);
        WorldObjectVisibility resolved = resolution.Result.Perception.Entries.Single(entry =>
            ReferenceEquals(entry.ObjectId, binding.Candidate.TargetObjectId));
        Assert.Same(VisibilityState.Identified, resolved.VisibilityState);
        Assert.NotSame(binding.Access.InscriptionBinding.InitialVisibility, resolved);
        Assert.Same(
            VisibilityState.Noticed,
            binding.Access.InscriptionBinding.InitialVisibility.VisibilityState);
        Assert.Same(
            binding.Candidate,
            Assert.Single(scenario.ReinterpretationCandidates.Entries));
    }

    [Fact]
    public void RejectsForeignOrRebuiltSeekerAuthority()
    {
        OpeningRegionLayeredInscriptionCandidateBinding binding = CandidateBinding();
        DiscoveryScenario scenario = binding.Access.InscriptionBinding.DiscoveryScenario;
        Character rebuiltIdentifier = Character(
            new CharacterId(scenario.ObserverCharacterId.Value),
            scenario.AvailableKnowledge);
        Character rebuiltKnowledge = Character(
            scenario.ObserverCharacterId,
            new CharacterKnowledge(scenario.AvailableKnowledge.KnowledgeReferences));
        Character missingKnowledge = Character(
            scenario.ObserverCharacterId,
            new CharacterKnowledge([]));
        DiscoveryScenario foreignScenario = RepresentativeDiscoveryScenarioSource.Create();
        Character foreign = Character(
            foreignScenario.ObserverCharacterId,
            foreignScenario.AvailableKnowledge);

        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(
                binding,
                rebuiltIdentifier));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(
                binding,
                rebuiltKnowledge));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(
                binding,
                missingKnowledge));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(binding, foreign));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionLayeredInscriptionResolutionTransition.Resolve(binding, null!));
    }

    private static OpeningRegionLayeredInscriptionCandidateBinding CandidateBinding()
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
        return new OpeningRegionLayeredInscriptionCandidateBinding(
            access,
            scenario.ReinterpretationCandidates.Entries.Single(),
            scenario.AvailableKnowledge.KnowledgeReferences.Single());
    }

    private static Character Character(
        CharacterId characterId,
        CharacterKnowledge knowledge) => new(
            new CharacterIdentity(characterId, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            knowledge,
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
}
