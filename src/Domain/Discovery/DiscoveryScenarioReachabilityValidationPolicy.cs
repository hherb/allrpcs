using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>Validates evidence reachability through the scenario's bounded discovery routes.</summary>
public static class DiscoveryScenarioReachabilityValidationPolicy
{
    private static readonly IReadOnlyDictionary<VisibilityState, int> VisibilityRanks =
        VisibilityState.All
            .Select((state, rank) => (state, rank))
            .ToDictionary(entry => entry.state, entry => entry.rank);

    /// <summary>Reports unreachable evidence in stable authored order without changing inputs.</summary>
    public static DiscoveryScenarioValidationResult Validate(DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        List<DiscoveryScenarioValidationIssue> issues = new();
        foreach (DiscoveryEvidence evidence in scenario.Evidence.Entries)
        {
            if (!CanReachRequiredVisibility(scenario, evidence))
            {
                issues.Add(new DiscoveryScenarioValidationIssue(
                    evidence.IsCritical
                        ? DiscoveryScenarioValidationIssueCode.UnreachableCriticalEvidence
                        : DiscoveryScenarioValidationIssueCode.UnreachableEvidence,
                    evidence.EvidenceId.Value));
            }
        }

        return new DiscoveryScenarioValidationResult(issues);
    }

    private static bool CanReachRequiredVisibility(
        DiscoveryScenario scenario,
        DiscoveryEvidence evidence)
    {
        WorldObjectVisibility? initialVisibility = scenario.InitialPerception.Entries
            .FirstOrDefault(visibility => visibility.ObjectId == evidence.ObjectId);
        if (initialVisibility is null)
        {
            return false;
        }

        int maximumReachableRank = Rank(initialVisibility.VisibilityState);
        if (DiscoveryConditionPolicy.IsSatisfied(
                evidence.ObjectId,
                scenario.Conditions,
                scenario.AvailableCapabilities,
                scenario.AvailableInventory))
        {
            maximumReachableRank = Math.Max(maximumReachableRank, Rank(VisibilityState.Identified));
        }

        if (HasPassiveNoticingRoute(scenario, evidence.ObjectId))
        {
            maximumReachableRank = Math.Max(maximumReachableRank, Rank(VisibilityState.Noticed));
        }

        if (HasReinterpretationRoute(scenario, evidence.ObjectId, initialVisibility.VisibilityState))
        {
            maximumReachableRank = Math.Max(maximumReachableRank, Rank(VisibilityState.Identified));
        }

        return maximumReachableRank >= Rank(evidence.MinimumVisibilityState);
    }

    private static bool HasPassiveNoticingRoute(DiscoveryScenario scenario, ObjectId objectId) =>
        scenario.PassiveNoticingCandidates.Entries.Any(candidate =>
            candidate.ObserverCharacterId == scenario.ObserverCharacterId &&
            candidate.TargetObjectId == objectId);

    private static bool HasReinterpretationRoute(
        DiscoveryScenario scenario,
        ObjectId objectId,
        VisibilityState initialVisibilityState) =>
        initialVisibilityState == VisibilityState.Noticed &&
        scenario.ReinterpretationCandidates.Entries.Any(candidate =>
            candidate.ObserverCharacterId == scenario.ObserverCharacterId &&
            candidate.TargetObjectId == objectId &&
            scenario.AvailableKnowledge.KnowledgeReferences.Contains(candidate.TriggerKnowledgeId));

    private static int Rank(VisibilityState state) => VisibilityRanks[state];
}
