using System.Numerics;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Discovery;

public sealed class OpeningRegionLayeredInscriptionCandidateBindingTests
{
    [Fact]
    public void RetainsTheExactCandidateAndTriggerForExactShrineAccess()
    {
        OpeningRegionLayeredInscriptionAccess access = Access();
        DiscoveryScenario scenario = access.InscriptionBinding.DiscoveryScenario;
        ObservationReinterpretationCandidate candidate = Assert.Single(
            scenario.ReinterpretationCandidates.Entries);
        KnowledgeId trigger = Assert.Single(scenario.AvailableKnowledge.KnowledgeReferences);

        OpeningRegionLayeredInscriptionCandidateBinding binding = new(
            access,
            candidate,
            trigger);

        Assert.Same(access, binding.Access);
        Assert.Same(candidate, binding.Candidate);
        Assert.Same(trigger, binding.TriggerKnowledgeId);
    }

    [Fact]
    public void RejectsRebuiltForeignMissingAndMismatchedCandidatesOrTriggers()
    {
        OpeningRegionLayeredInscriptionAccess access = Access();
        DiscoveryScenario scenario = access.InscriptionBinding.DiscoveryScenario;
        ObservationReinterpretationCandidate candidate = Assert.Single(
            scenario.ReinterpretationCandidates.Entries);
        KnowledgeId trigger = Assert.Single(scenario.AvailableKnowledge.KnowledgeReferences);
        ObservationReinterpretationCandidate rebuilt = new(
            candidate.ObserverCharacterId,
            candidate.TargetObjectId,
            candidate.TriggerKnowledgeId);
        DiscoveryScenario foreignScenario = RepresentativeDiscoveryScenarioSource.Create();
        ObservationReinterpretationCandidate foreignCandidate = Assert.Single(
            foreignScenario.ReinterpretationCandidates.Entries);
        KnowledgeId foreignTrigger = Assert.Single(
            foreignScenario.AvailableKnowledge.KnowledgeReferences);
        ObservationReinterpretationCandidate wrongTarget = new(
            candidate.ObserverCharacterId,
            scenario.WorldObjects.Entries[0].Identity.Id,
            trigger);
        DiscoveryScenario missingScenario = With(
            scenario,
            candidates: new ObservationReinterpretationCandidateSnapshot([]));
        OpeningRegionLayeredInscriptionAccess missingAccess = Access(missingScenario);

        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionLayeredInscriptionCandidateBinding(access, rebuilt, trigger));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionLayeredInscriptionCandidateBinding(access, foreignCandidate, trigger));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionLayeredInscriptionCandidateBinding(missingAccess, candidate, trigger));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionLayeredInscriptionCandidateBinding(access, wrongTarget, trigger));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionLayeredInscriptionCandidateBinding(
                access,
                candidate,
                new KnowledgeId(trigger.Value)));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionLayeredInscriptionCandidateBinding(access, candidate, foreignTrigger));
        Assert.Throws<ArgumentException>(() => new ObservationReinterpretationCandidateSnapshot(
            [candidate, candidate]));
    }

    private static OpeningRegionLayeredInscriptionAccess Access(
        DiscoveryScenario? suppliedScenario = null)
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionOldShrineSceneBinding shrine = new(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
        DiscoveryScenario scenario = suppliedScenario ?? RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionLayeredInscriptionBinding inscription = new(
            shrine,
            scenario,
            scenario.WorldObjects.Entries.Single(candidate =>
                candidate.Identity.Id.Value == "shrine-inscription"),
            scenario.InitialPerception.Entries.Single(candidate =>
                candidate.ObjectId.Value == "shrine-inscription"),
            new Vector2(110.0f, 90.0f));
        return new OpeningRegionLayeredInscriptionAccess(
            inscription,
            scenario.ObserverCharacterId,
            new Vector2(110.0f, 128.0f));
    }

    private static DiscoveryScenario With(
        DiscoveryScenario scenario,
        ObservationReinterpretationCandidateSnapshot candidates) => new(
            scenario.ObserverCharacterId,
            scenario.WorldObjects,
            scenario.Evidence,
            scenario.InitialPerception,
            scenario.Conditions,
            scenario.AvailableCapabilities,
            scenario.AvailableInventory,
            scenario.AvailableKnowledge,
            scenario.PassiveNoticingCandidates,
            candidates);
}
