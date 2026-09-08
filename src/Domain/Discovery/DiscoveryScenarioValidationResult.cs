namespace Allrpcs.Domain.Discovery;

/// <summary>Contains deterministic discovery-scenario validation issues in policy order.</summary>
public sealed class DiscoveryScenarioValidationResult
{
    /// <summary>Creates an immutable validation result from exact issue instances.</summary>
    public DiscoveryScenarioValidationResult(IEnumerable<DiscoveryScenarioValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        List<DiscoveryScenarioValidationIssue> retainedIssues = new();
        foreach (DiscoveryScenarioValidationIssue issue in issues)
        {
            if (issue is null)
            {
                throw new ArgumentException(
                    "Validation issues cannot contain null values.",
                    nameof(issues));
            }

            retainedIssues.Add(issue);
        }

        Issues = retainedIssues.AsReadOnly();
    }

    /// <summary>Gets whether validation found no defects.</summary>
    public bool IsValid => Issues.Count == 0;

    /// <summary>Gets exact issues in deterministic policy order.</summary>
    public IReadOnlyList<DiscoveryScenarioValidationIssue> Issues { get; }
}
