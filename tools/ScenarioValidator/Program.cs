using Allrpcs.Domain.Discovery;

DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
DiscoveryScenarioValidationResult result = DiscoveryScenarioValidationPolicy.Validate(scenario);

if (result.IsValid)
{
    Console.WriteLine(
        $"Representative discovery scenario valid: {scenario.Evidence.Entries.Count} evidence definitions.");
    return 0;
}

foreach (DiscoveryScenarioValidationIssue issue in result.Issues)
{
    Console.Error.WriteLine($"{issue.Code}: {issue.SubjectIdentifier}");
}

return 1;
