namespace Allrpcs.Domain.Discovery;

/// <summary>Composes structural and evidence-reachability validation for one scenario.</summary>
public static class DiscoveryScenarioValidationPolicy
{
    /// <summary>Returns all issues in stable structural-then-evidence order.</summary>
    public static DiscoveryScenarioValidationResult Validate(DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        DiscoveryScenarioValidationResult structural =
            DiscoveryScenarioStructuralValidationPolicy.Validate(scenario);
        DiscoveryScenarioValidationResult reachability =
            DiscoveryScenarioReachabilityValidationPolicy.Validate(scenario);

        return new DiscoveryScenarioValidationResult(structural.Issues.Concat(reachability.Issues));
    }
}
