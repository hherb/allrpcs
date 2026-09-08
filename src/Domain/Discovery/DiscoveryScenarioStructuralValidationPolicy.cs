using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>Validates authored discovery references without executing any discovery route.</summary>
public static class DiscoveryScenarioStructuralValidationPolicy
{
    /// <summary>Returns every structural issue in stable category and authored-entry order.</summary>
    public static DiscoveryScenarioValidationResult Validate(DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        List<DiscoveryScenarioValidationIssue> issues = new();
        HashSet<ObjectId> knownObjectIds = scenario.WorldObjects.Entries
            .Select(worldObject => worldObject.Identity.Id)
            .ToHashSet();
        HashSet<ObjectId> perceptionObjectIds = scenario.InitialPerception.Entries
            .Select(visibility => visibility.ObjectId)
            .ToHashSet();

        if (scenario.InitialPerception.CharacterId != scenario.ObserverCharacterId)
        {
            issues.Add(Issue(
                DiscoveryScenarioValidationIssueCode.ObserverPerceptionMismatch,
                scenario.InitialPerception.CharacterId.Value));
        }

        foreach (WorldObject worldObject in scenario.WorldObjects.Entries)
        {
            ObjectId objectId = worldObject.Identity.Id;
            if (!perceptionObjectIds.Contains(objectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.MissingPerception,
                    objectId.Value));
            }
        }

        AddUnknownPerceptionIssues(scenario, knownObjectIds, issues);
        AddEvidenceIssues(scenario, knownObjectIds, issues);
        AddCapabilityIssues(scenario, knownObjectIds, issues);
        AddToolIssues(scenario, knownObjectIds, issues);
        AddInventoryIssues(scenario, knownObjectIds, issues);
        AddPassiveCandidateIssues(scenario, knownObjectIds, issues);
        AddReinterpretationCandidateIssues(scenario, knownObjectIds, issues);

        return new DiscoveryScenarioValidationResult(issues);
    }

    private static void AddUnknownPerceptionIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        foreach (WorldObjectVisibility visibility in scenario.InitialPerception.Entries)
        {
            if (!knownObjectIds.Contains(visibility.ObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownPerceptionTarget,
                    visibility.ObjectId.Value));
            }
        }
    }

    private static void AddEvidenceIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        foreach (DiscoveryEvidence evidence in scenario.Evidence.Entries)
        {
            if (!knownObjectIds.Contains(evidence.ObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownEvidenceTarget,
                    evidence.EvidenceId.Value));
            }
        }
    }

    private static void AddCapabilityIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        HashSet<string> availableCapabilityKeys = scenario.AvailableCapabilities.Entries
            .Select(capability => capability.Key)
            .ToHashSet(StringComparer.Ordinal);
        foreach (DiscoveryCapabilityRequirement requirement in
                 scenario.Conditions.CapabilityRequirements.Entries)
        {
            if (!knownObjectIds.Contains(requirement.ObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownCapabilityTarget,
                    requirement.ObjectId.Value));
            }

            if (!availableCapabilityKeys.Contains(requirement.RequiredCapabilityKey))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnavailableCapability,
                    requirement.RequiredCapabilityKey));
            }
        }
    }

    private static void AddToolIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        HashSet<ObjectId> inventoryObjectIds = scenario.AvailableInventory.ObjectReferences.ToHashSet();
        foreach (DiscoveryToolRequirement requirement in scenario.Conditions.ToolRequirements.Entries)
        {
            if (!knownObjectIds.Contains(requirement.ObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownToolTarget,
                    requirement.ObjectId.Value));
            }

            if (!knownObjectIds.Contains(requirement.RequiredToolObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownToolObject,
                    requirement.RequiredToolObjectId.Value));
            }

            if (!inventoryObjectIds.Contains(requirement.RequiredToolObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnavailableTool,
                    requirement.RequiredToolObjectId.Value));
            }
        }
    }

    private static void AddInventoryIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        foreach (ObjectId inventoryObjectId in scenario.AvailableInventory.ObjectReferences)
        {
            if (!knownObjectIds.Contains(inventoryObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownInventoryObject,
                    inventoryObjectId.Value));
            }
        }
    }

    private static void AddPassiveCandidateIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        foreach (PassiveNoticingCandidate candidate in scenario.PassiveNoticingCandidates.Entries)
        {
            if (candidate.ObserverCharacterId != scenario.ObserverCharacterId)
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.PassiveObserverMismatch,
                    candidate.TargetObjectId.Value));
            }

            if (!knownObjectIds.Contains(candidate.TargetObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownPassiveTarget,
                    candidate.TargetObjectId.Value));
            }
        }
    }

    private static void AddReinterpretationCandidateIssues(
        DiscoveryScenario scenario,
        HashSet<ObjectId> knownObjectIds,
        List<DiscoveryScenarioValidationIssue> issues)
    {
        HashSet<KnowledgeId> availableKnowledge = scenario.AvailableKnowledge.KnowledgeReferences
            .ToHashSet();
        foreach (ObservationReinterpretationCandidate candidate in
                 scenario.ReinterpretationCandidates.Entries)
        {
            if (candidate.ObserverCharacterId != scenario.ObserverCharacterId)
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.ReinterpretationObserverMismatch,
                    candidate.TargetObjectId.Value));
            }

            if (!knownObjectIds.Contains(candidate.TargetObjectId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnknownReinterpretationTarget,
                    candidate.TargetObjectId.Value));
            }

            if (!availableKnowledge.Contains(candidate.TriggerKnowledgeId))
            {
                issues.Add(Issue(
                    DiscoveryScenarioValidationIssueCode.UnavailableTriggerKnowledge,
                    candidate.TriggerKnowledgeId.Value));
            }
        }
    }

    private static DiscoveryScenarioValidationIssue Issue(
        DiscoveryScenarioValidationIssueCode code,
        string subjectIdentifier) => new(code, subjectIdentifier);
}
