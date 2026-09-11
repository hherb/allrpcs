using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class CriticalInvestigationConclusionScenarioTests
{
    [Fact]
    public void RetainsTheExactPlanAndSourceSnapshots()
    {
        CriticalInvestigationConclusionPlanPolicyTests.PlanInput input =
            CriticalInvestigationConclusionPlanPolicyTests.Input();
        CriticalInvestigationConclusionPlanSnapshot plans = new([input.Plan]);

        CriticalInvestigationConclusionScenario scenario = new(plans, input.Sources);

        Assert.Same(plans, scenario.Plans);
        Assert.Same(input.Sources, scenario.Sources);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableScenarioSurface()
    {
        CriticalInvestigationConclusionPlanPolicyTests.PlanInput input =
            CriticalInvestigationConclusionPlanPolicyTests.Input();
        CriticalInvestigationConclusionPlanSnapshot plans = new([input.Plan]);

        Assert.Throws<ArgumentNullException>(() =>
            new CriticalInvestigationConclusionScenario(null!, input.Sources));
        Assert.Throws<ArgumentNullException>(() =>
            new CriticalInvestigationConclusionScenario(plans, null!));

        Type scenarioType = typeof(CriticalInvestigationConclusionScenario);
        System.Reflection.PropertyInfo[] properties = scenarioType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(scenarioType.GetConstructors());

        Assert.Equal(["Plans", "Sources"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(CriticalInvestigationConclusionPlanSnapshot), typeof(InvestigationEvidenceSourceSnapshot)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(scenarioType.GetEvents());
        Assert.Empty(scenarioType.GetMethods().Where(
            method => method.DeclaringType == scenarioType && !method.IsSpecialName));
        Assert.True(scenarioType.IsSealed);
    }
}
