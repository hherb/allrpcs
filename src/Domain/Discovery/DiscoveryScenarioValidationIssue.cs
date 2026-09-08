namespace Allrpcs.Domain.Discovery;

/// <summary>Describes one deterministic scenario defect and its exact authored subject.</summary>
public sealed record DiscoveryScenarioValidationIssue
{
    /// <summary>Creates one validation issue without rewriting its subject identifier.</summary>
    public DiscoveryScenarioValidationIssue(
        DiscoveryScenarioValidationIssueCode code,
        string subjectIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectIdentifier);

        Code = code;
        SubjectIdentifier = subjectIdentifier;
    }

    /// <summary>Gets the stable kind of defect.</summary>
    public DiscoveryScenarioValidationIssueCode Code { get; }

    /// <summary>Gets the exact identifier of the affected authored subject.</summary>
    public string SubjectIdentifier { get; }
}
