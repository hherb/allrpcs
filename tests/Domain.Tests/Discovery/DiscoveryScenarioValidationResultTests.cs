using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryScenarioValidationResultTests
{
    [Fact]
    public void RetainsExactIssuesAndReportsValidity()
    {
        DiscoveryScenarioValidationIssue issue = new(
            DiscoveryScenarioValidationIssueCode.MissingPerception,
            "garden-herb");
        DiscoveryScenarioValidationIssue[] source = [issue];

        DiscoveryScenarioValidationResult invalidResult = new(source);
        DiscoveryScenarioValidationResult validResult = new([]);
        source[0] = new(
            DiscoveryScenarioValidationIssueCode.UnknownEvidenceTarget,
            "other-evidence");

        Assert.False(invalidResult.IsValid);
        Assert.Same(issue, Assert.Single(invalidResult.Issues));
        Assert.True(validResult.IsValid);
        Assert.Empty(validResult.Issues);
        Assert.Throws<NotSupportedException>(() =>
            Assert.IsAssignableFrom<IList<DiscoveryScenarioValidationIssue>>(invalidResult.Issues)
                .Add(issue));
    }

    [Fact]
    public void RejectsInvalidIssueInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new DiscoveryScenarioValidationResult(null!));
        Assert.Throws<ArgumentException>(() =>
            new DiscoveryScenarioValidationResult([null!]));
        Assert.Throws<ArgumentException>(() => new DiscoveryScenarioValidationIssue(
            DiscoveryScenarioValidationIssueCode.MissingPerception,
            " "));
    }
}
