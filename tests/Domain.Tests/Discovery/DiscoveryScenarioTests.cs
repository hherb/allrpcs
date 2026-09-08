using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryScenarioTests
{
    [Fact]
    public void RetainsEveryExactAuthoredInput()
    {
        ScenarioParts parts = CreateParts();

        DiscoveryScenario scenario = CreateScenario(parts);

        Assert.Same(parts.ObserverCharacterId, scenario.ObserverCharacterId);
        Assert.Same(parts.WorldObjects, scenario.WorldObjects);
        Assert.Same(parts.Evidence, scenario.Evidence);
        Assert.Same(parts.InitialPerception, scenario.InitialPerception);
        Assert.Same(parts.Conditions, scenario.Conditions);
        Assert.Same(parts.AvailableCapabilities, scenario.AvailableCapabilities);
        Assert.Same(parts.AvailableInventory, scenario.AvailableInventory);
        Assert.Same(parts.AvailableKnowledge, scenario.AvailableKnowledge);
        Assert.Same(parts.PassiveCandidates, scenario.PassiveNoticingCandidates);
        Assert.Same(parts.ReinterpretationCandidates, scenario.ReinterpretationCandidates);
    }

    [Fact]
    public void RejectsEveryNullInput()
    {
        ScenarioParts parts = CreateParts();

        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { ObserverCharacterId = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { WorldObjects = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(parts with { Evidence = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { InitialPerception = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(parts with { Conditions = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { AvailableCapabilities = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { AvailableInventory = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { AvailableKnowledge = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { PassiveCandidates = null! }));
        Assert.Throws<ArgumentNullException>(() => CreateScenario(
            parts with { ReinterpretationCandidates = null! }));
    }

    private static DiscoveryScenario CreateScenario(ScenarioParts parts) => new(
        parts.ObserverCharacterId,
        parts.WorldObjects,
        parts.Evidence,
        parts.InitialPerception,
        parts.Conditions,
        parts.AvailableCapabilities,
        parts.AvailableInventory,
        parts.AvailableKnowledge,
        parts.PassiveCandidates,
        parts.ReinterpretationCandidates);

    private static ScenarioParts CreateParts()
    {
        CharacterId observer = new("seeker");
        ObjectId target = new("garden-herb");
        return new ScenarioParts(
            observer,
            new WorldObjectSnapshot(
                [
                    new WorldObject(
                        new WorldObjectIdentity(target, "Garden herb"),
                        new WorldObjectDescription("A familiar herb.")),
                ]),
            new DiscoveryEvidenceSnapshot(
                [
                    new DiscoveryEvidence(
                        new EvidenceId("visible-herb"),
                        target,
                        VisibilityState.Noticed,
                        isCritical: true),
                ]),
            new CharacterPerceptionSnapshot(
                observer,
                [new WorldObjectVisibility(target, VisibilityState.Noticed)]),
            new DiscoveryConditionSet(
                new DiscoveryCapabilityRequirementSet([]),
                new DiscoveryToolRequirementSet([])),
            new CharacterCapabilitySet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
    }

    private sealed record ScenarioParts(
        CharacterId ObserverCharacterId,
        WorldObjectSnapshot WorldObjects,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterPerceptionSnapshot InitialPerception,
        DiscoveryConditionSet Conditions,
        CharacterCapabilitySet AvailableCapabilities,
        CharacterInventory AvailableInventory,
        CharacterKnowledge AvailableKnowledge,
        PassiveNoticingCandidateSnapshot PassiveCandidates,
        ObservationReinterpretationCandidateSnapshot ReinterpretationCandidates);
}
