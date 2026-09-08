using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryScenarioStructuralValidationPolicyTests
{
    [Fact]
    public void AcceptsRepresentativeScenarioWithoutChangingIt()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        DiscoveryScenarioValidationResult result =
            DiscoveryScenarioStructuralValidationPolicy.Validate(scenario);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
        Assert.Equal(5, scenario.Evidence.Entries.Count);
        Assert.Equal(6, scenario.WorldObjects.Entries.Count);
    }

    [Fact]
    public void ReportsObserverAndPerceptionDefectsInStableOrder()
    {
        DiscoveryScenario source = RepresentativeDiscoveryScenarioSource.Create();
        ObjectId unknownObjectId = new("unknown-object");
        CharacterPerceptionSnapshot perception = new(
            new CharacterId("healer"),
            source.InitialPerception.Entries
                .Skip(1)
                .Append(new WorldObjectVisibility(unknownObjectId, VisibilityState.Noticed)));
        DiscoveryScenario scenario = Copy(source, initialPerception: perception);

        DiscoveryScenarioValidationResult result =
            DiscoveryScenarioStructuralValidationPolicy.Validate(scenario);

        AssertIssues(
            result,
            (DiscoveryScenarioValidationIssueCode.ObserverPerceptionMismatch, "healer"),
            (DiscoveryScenarioValidationIssueCode.MissingPerception, "garden-herb"),
            (DiscoveryScenarioValidationIssueCode.UnknownPerceptionTarget, "unknown-object"));
    }

    [Fact]
    public void ReportsUnknownEvidenceConditionsAndInventoryInStableOrder()
    {
        DiscoveryScenario source = RepresentativeDiscoveryScenarioSource.Create();
        ObjectId unknownTarget = new("unknown-target");
        ObjectId unknownTool = new("unknown-tool");
        DiscoveryScenario scenario = Copy(
            source,
            evidence: new DiscoveryEvidenceSnapshot(
            [
                new DiscoveryEvidence(
                    new EvidenceId("unknown-evidence"),
                    unknownTarget,
                    VisibilityState.Noticed,
                    isCritical: false),
            ]),
            conditions: new DiscoveryConditionSet(
                new DiscoveryCapabilityRequirementSet(
                [
                    new DiscoveryCapabilityRequirement(unknownTarget, "missing-capability"),
                ]),
                new DiscoveryToolRequirementSet(
                [
                    new DiscoveryToolRequirement(unknownTarget, unknownTool),
                ])),
            availableCapabilities: new CharacterCapabilitySet([]),
            availableInventory: new CharacterInventory([unknownTool]));

        DiscoveryScenarioValidationResult result =
            DiscoveryScenarioStructuralValidationPolicy.Validate(scenario);

        AssertIssues(
            result,
            (DiscoveryScenarioValidationIssueCode.UnknownEvidenceTarget, "unknown-evidence"),
            (DiscoveryScenarioValidationIssueCode.UnknownCapabilityTarget, "unknown-target"),
            (DiscoveryScenarioValidationIssueCode.UnavailableCapability, "missing-capability"),
            (DiscoveryScenarioValidationIssueCode.UnknownToolTarget, "unknown-target"),
            (DiscoveryScenarioValidationIssueCode.UnknownToolObject, "unknown-tool"),
            (DiscoveryScenarioValidationIssueCode.UnknownInventoryObject, "unknown-tool"));
    }

    [Fact]
    public void ReportsUnavailableToolAndInvalidCandidateReferencesInStableOrder()
    {
        DiscoveryScenario source = RepresentativeDiscoveryScenarioSource.Create();
        ObjectId unknownTarget = new("unknown-target");
        KnowledgeId unknownKnowledge = new("unknown-knowledge");
        DiscoveryToolRequirement sourceToolRequirement = Assert.Single(
            source.Conditions.ToolRequirements.Entries);
        DiscoveryScenario scenario = Copy(
            source,
            availableInventory: new CharacterInventory([]),
            passiveCandidates: new PassiveNoticingCandidateSnapshot(
            [
                new PassiveNoticingCandidate(new CharacterId("healer"), unknownTarget),
            ]),
            reinterpretationCandidates: new ObservationReinterpretationCandidateSnapshot(
            [
                new ObservationReinterpretationCandidate(
                    new CharacterId("healer"),
                    unknownTarget,
                    unknownKnowledge),
            ]));

        DiscoveryScenarioValidationResult result =
            DiscoveryScenarioStructuralValidationPolicy.Validate(scenario);

        AssertIssues(
            result,
            (
                DiscoveryScenarioValidationIssueCode.UnavailableTool,
                sourceToolRequirement.RequiredToolObjectId.Value),
            (DiscoveryScenarioValidationIssueCode.PassiveObserverMismatch, "unknown-target"),
            (DiscoveryScenarioValidationIssueCode.UnknownPassiveTarget, "unknown-target"),
            (DiscoveryScenarioValidationIssueCode.ReinterpretationObserverMismatch, "unknown-target"),
            (DiscoveryScenarioValidationIssueCode.UnknownReinterpretationTarget, "unknown-target"),
            (DiscoveryScenarioValidationIssueCode.UnavailableTriggerKnowledge, "unknown-knowledge"));
    }

    [Fact]
    public void RejectsNullScenario()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DiscoveryScenarioStructuralValidationPolicy.Validate(null!));
    }

    private static DiscoveryScenario Copy(
        DiscoveryScenario source,
        DiscoveryEvidenceSnapshot? evidence = null,
        CharacterPerceptionSnapshot? initialPerception = null,
        DiscoveryConditionSet? conditions = null,
        CharacterCapabilitySet? availableCapabilities = null,
        CharacterInventory? availableInventory = null,
        PassiveNoticingCandidateSnapshot? passiveCandidates = null,
        ObservationReinterpretationCandidateSnapshot? reinterpretationCandidates = null) => new(
            source.ObserverCharacterId,
            source.WorldObjects,
            evidence ?? source.Evidence,
            initialPerception ?? source.InitialPerception,
            conditions ?? source.Conditions,
            availableCapabilities ?? source.AvailableCapabilities,
            availableInventory ?? source.AvailableInventory,
            source.AvailableKnowledge,
            passiveCandidates ?? source.PassiveNoticingCandidates,
            reinterpretationCandidates ?? source.ReinterpretationCandidates);

    private static void AssertIssues(
        DiscoveryScenarioValidationResult result,
        params (DiscoveryScenarioValidationIssueCode Code, string Subject)[] expected)
    {
        Assert.False(result.IsValid);
        Assert.Equal(
            expected,
            result.Issues.Select(issue => (issue.Code, issue.SubjectIdentifier)));
    }
}
