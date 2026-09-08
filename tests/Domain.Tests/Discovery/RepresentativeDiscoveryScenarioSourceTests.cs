using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class RepresentativeDiscoveryScenarioSourceTests
{
    [Fact]
    public void CreatesFiveRepresentativeDiscoveriesInStableOrder()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        Assert.Equal(new CharacterId("seeker"), scenario.ObserverCharacterId);
        Assert.Collection(
            scenario.Evidence.Entries,
            evidence => AssertEvidence(
                evidence,
                "visible-herb",
                "garden-herb",
                VisibilityState.Noticed,
                isCritical: false),
            evidence => AssertEvidence(
                evidence,
                "concealed-herb",
                "woodland-herb",
                VisibilityState.Identified,
                isCritical: true),
            evidence => AssertEvidence(
                evidence,
                "partial-inscription",
                "shrine-inscription",
                VisibilityState.Identified,
                isCritical: true),
            evidence => AssertEvidence(
                evidence,
                "residue",
                "stream-residue",
                VisibilityState.Noticed,
                isCritical: false),
            evidence => AssertEvidence(
                evidence,
                "tool-gated-clue",
                "wagon-clue",
                VisibilityState.Identified,
                isCritical: true));
    }

    [Fact]
    public void RetainsExactObjectsPerceptionAndOnlyRequiredRoutes()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        Assert.Collection(
            scenario.WorldObjects.Entries,
            worldObject => Assert.Equal("garden-herb", worldObject.Identity.Id.Value),
            worldObject => Assert.Equal("woodland-herb", worldObject.Identity.Id.Value),
            worldObject => Assert.Equal("shrine-inscription", worldObject.Identity.Id.Value),
            worldObject => Assert.Equal("stream-residue", worldObject.Identity.Id.Value),
            worldObject => Assert.Equal("wagon-clue", worldObject.Identity.Id.Value),
            worldObject => Assert.Equal("field-lens", worldObject.Identity.Id.Value));
        Assert.Collection(
            scenario.InitialPerception.Entries,
            visibility => AssertVisibility(visibility, "garden-herb", VisibilityState.Noticed),
            visibility => AssertVisibility(visibility, "woodland-herb", VisibilityState.Unnoticed),
            visibility => AssertVisibility(visibility, "shrine-inscription", VisibilityState.Noticed),
            visibility => AssertVisibility(visibility, "stream-residue", VisibilityState.Unnoticed),
            visibility => AssertVisibility(visibility, "wagon-clue", VisibilityState.Unnoticed),
            visibility => AssertVisibility(visibility, "field-lens", VisibilityState.Collected));

        DiscoveryCapabilityRequirement capabilityRequirement = Assert.Single(
            scenario.Conditions.CapabilityRequirements.Entries);
        Assert.Same(
            scenario.WorldObjects.Entries[1].Identity.Id,
            capabilityRequirement.ObjectId);
        CharacterCapability capability = Assert.Single(scenario.AvailableCapabilities.Entries);
        Assert.Equal("identify-healing-plants", capability.Key);
        Assert.Equal(capability.Key, capabilityRequirement.RequiredCapabilityKey);

        DiscoveryToolRequirement toolRequirement = Assert.Single(
            scenario.Conditions.ToolRequirements.Entries);
        Assert.Same(scenario.WorldObjects.Entries[4].Identity.Id, toolRequirement.ObjectId);
        ObjectId toolObjectId = Assert.Single(scenario.AvailableInventory.ObjectReferences);
        Assert.Same(scenario.WorldObjects.Entries[5].Identity.Id, toolObjectId);
        Assert.Same(toolObjectId, toolRequirement.RequiredToolObjectId);

        PassiveNoticingCandidate passiveCandidate = Assert.Single(
            scenario.PassiveNoticingCandidates.Entries);
        Assert.Same(scenario.ObserverCharacterId, passiveCandidate.ObserverCharacterId);
        Assert.Same(scenario.WorldObjects.Entries[3].Identity.Id, passiveCandidate.TargetObjectId);

        ObservationReinterpretationCandidate reinterpretationCandidate = Assert.Single(
            scenario.ReinterpretationCandidates.Entries);
        Assert.Same(scenario.ObserverCharacterId, reinterpretationCandidate.ObserverCharacterId);
        Assert.Same(
            scenario.WorldObjects.Entries[2].Identity.Id,
            reinterpretationCandidate.TargetObjectId);
        KnowledgeId triggerKnowledgeId = Assert.Single(
            scenario.AvailableKnowledge.KnowledgeReferences);
        Assert.Same(triggerKnowledgeId, reinterpretationCandidate.TriggerKnowledgeId);
    }

    [Fact]
    public void RetainsExactInstancesAndImmutableSnapshotsRejectDuplicateIdentifiers()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject firstObject = scenario.WorldObjects.Entries[0];
        DiscoveryEvidence firstEvidence = scenario.Evidence.Entries[0];
        WorldObjectVisibility firstVisibility = scenario.InitialPerception.Entries[0];

        Assert.Same(firstObject.Identity.Id, firstEvidence.ObjectId);
        Assert.Same(firstObject.Identity.Id, firstVisibility.ObjectId);
        Assert.Throws<ArgumentException>(() => new WorldObjectSnapshot([firstObject, firstObject]));
        Assert.Throws<ArgumentException>(() => new DiscoveryEvidenceSnapshot(
            [firstEvidence, firstEvidence]));
        Assert.Throws<ArgumentException>(() => new CharacterPerceptionSnapshot(
            scenario.ObserverCharacterId,
            [firstVisibility, firstVisibility]));
        Assert.Same(firstObject, scenario.WorldObjects.Entries[0]);
        Assert.Same(firstEvidence, scenario.Evidence.Entries[0]);
        Assert.Same(firstVisibility, scenario.InitialPerception.Entries[0]);
    }

    private static void AssertEvidence(
        DiscoveryEvidence evidence,
        string evidenceId,
        string objectId,
        VisibilityState visibilityState,
        bool isCritical)
    {
        Assert.Equal(evidenceId, evidence.EvidenceId.Value);
        Assert.Equal(objectId, evidence.ObjectId.Value);
        Assert.Same(visibilityState, evidence.MinimumVisibilityState);
        Assert.Equal(isCritical, evidence.IsCritical);
    }

    private static void AssertVisibility(
        WorldObjectVisibility visibility,
        string objectId,
        VisibilityState visibilityState)
    {
        Assert.Equal(objectId, visibility.ObjectId.Value);
        Assert.Same(visibilityState, visibility.VisibilityState);
    }
}
