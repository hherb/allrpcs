using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryScenarioReachabilityValidationPolicyTests
{
    [Fact]
    public void AcceptsEveryRepresentativeRouteWithoutChangingScenario()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        IReadOnlyList<DiscoveryEvidence> evidence = scenario.Evidence.Entries;
        IReadOnlyList<WorldObjectVisibility> perception = scenario.InitialPerception.Entries;

        DiscoveryScenarioValidationResult result =
            DiscoveryScenarioReachabilityValidationPolicy.Validate(scenario);

        Assert.True(result.IsValid);
        Assert.Same(evidence, scenario.Evidence.Entries);
        Assert.Same(perception, scenario.InitialPerception.Entries);
        Assert.Equal(5, evidence.Count);
    }

    [Fact]
    public void ReportsUnreachableOrdinaryAndCriticalEvidenceInAuthoredOrder()
    {
        CharacterId observer = new("seeker");
        WorldObject firstTarget = CreateWorldObject("first-target");
        WorldObject secondTarget = CreateWorldObject("second-target");
        DiscoveryScenario scenario = CreateScenario(
            observer,
            [firstTarget, secondTarget],
            [
                new DiscoveryEvidence(
                    new EvidenceId("ordinary-evidence"),
                    firstTarget.Identity.Id,
                    VisibilityState.Identified,
                    isCritical: false),
                new DiscoveryEvidence(
                    new EvidenceId("critical-evidence"),
                    secondTarget.Identity.Id,
                    VisibilityState.Understood,
                    isCritical: true),
            ],
            [
                new WorldObjectVisibility(firstTarget.Identity.Id, VisibilityState.Unnoticed),
                new WorldObjectVisibility(secondTarget.Identity.Id, VisibilityState.Noticed),
            ]);

        DiscoveryScenarioValidationResult result =
            DiscoveryScenarioReachabilityValidationPolicy.Validate(scenario);

        Assert.Collection(
            result.Issues,
            issue => AssertIssue(
                issue,
                DiscoveryScenarioValidationIssueCode.UnreachableEvidence,
                "ordinary-evidence"),
            issue => AssertIssue(
                issue,
                DiscoveryScenarioValidationIssueCode.UnreachableCriticalEvidence,
                "critical-evidence"));
    }

    [Fact]
    public void FailsClosedWhenReinterpretationDoesNotStartFromNoticed()
    {
        CharacterId observer = new("seeker");
        WorldObject target = CreateWorldObject("inscription");
        KnowledgeId knowledge = new("read-script");
        DiscoveryScenario scenario = CreateScenario(
            observer,
            [target],
            [
                new DiscoveryEvidence(
                    new EvidenceId("inscription-evidence"),
                    target.Identity.Id,
                    VisibilityState.Identified,
                    isCritical: true),
            ],
            [new WorldObjectVisibility(target.Identity.Id, VisibilityState.Unnoticed)],
            knowledge: new CharacterKnowledge([knowledge]),
            reinterpretationCandidates: new ObservationReinterpretationCandidateSnapshot(
            [
                new ObservationReinterpretationCandidate(observer, target.Identity.Id, knowledge),
            ]));

        DiscoveryScenarioValidationIssue issue = Assert.Single(
            DiscoveryScenarioReachabilityValidationPolicy.Validate(scenario).Issues);

        AssertIssue(
            issue,
            DiscoveryScenarioValidationIssueCode.UnreachableCriticalEvidence,
            "inscription-evidence");
    }

    [Fact]
    public void CompositeReturnsStructuralIssuesBeforeReachabilityIssues()
    {
        CharacterId observer = new("seeker");
        WorldObject target = CreateWorldObject("known-target");
        DiscoveryScenario scenario = CreateScenario(
            observer,
            [target],
            [
                new DiscoveryEvidence(
                    new EvidenceId("unreachable-evidence"),
                    target.Identity.Id,
                    VisibilityState.Identified,
                    isCritical: false),
            ],
            []);

        DiscoveryScenarioValidationResult result = DiscoveryScenarioValidationPolicy.Validate(scenario);

        Assert.Collection(
            result.Issues,
            issue => AssertIssue(
                issue,
                DiscoveryScenarioValidationIssueCode.MissingPerception,
                "known-target"),
            issue => AssertIssue(
                issue,
                DiscoveryScenarioValidationIssueCode.UnreachableEvidence,
                "unreachable-evidence"));
    }

    [Fact]
    public void RejectsNullScenario()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DiscoveryScenarioReachabilityValidationPolicy.Validate(null!));
        Assert.Throws<ArgumentNullException>(() => DiscoveryScenarioValidationPolicy.Validate(null!));
    }

    private static DiscoveryScenario CreateScenario(
        CharacterId observer,
        IEnumerable<WorldObject> worldObjects,
        IEnumerable<DiscoveryEvidence> evidence,
        IEnumerable<WorldObjectVisibility> perception,
        DiscoveryConditionSet? conditions = null,
        CharacterCapabilitySet? capabilities = null,
        CharacterInventory? inventory = null,
        CharacterKnowledge? knowledge = null,
        PassiveNoticingCandidateSnapshot? passiveCandidates = null,
        ObservationReinterpretationCandidateSnapshot? reinterpretationCandidates = null) => new(
            observer,
            new WorldObjectSnapshot(worldObjects),
            new DiscoveryEvidenceSnapshot(evidence),
            new CharacterPerceptionSnapshot(observer, perception),
            conditions ?? new DiscoveryConditionSet(
                new DiscoveryCapabilityRequirementSet([]),
                new DiscoveryToolRequirementSet([])),
            capabilities ?? new CharacterCapabilitySet([]),
            inventory ?? new CharacterInventory([]),
            knowledge ?? new CharacterKnowledge([]),
            passiveCandidates ?? new PassiveNoticingCandidateSnapshot([]),
            reinterpretationCandidates ?? new ObservationReinterpretationCandidateSnapshot([]));

    private static WorldObject CreateWorldObject(string id) => new(
        new WorldObjectIdentity(new ObjectId(id), "Target"),
        new WorldObjectDescription("A test target."));

    private static void AssertIssue(
        DiscoveryScenarioValidationIssue issue,
        DiscoveryScenarioValidationIssueCode code,
        string subject)
    {
        Assert.Equal(code, issue.Code);
        Assert.Equal(subject, issue.SubjectIdentifier);
    }
}
