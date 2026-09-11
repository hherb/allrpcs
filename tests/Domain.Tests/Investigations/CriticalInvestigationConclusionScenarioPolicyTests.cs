using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class CriticalInvestigationConclusionScenarioPolicyTests
{
    [Fact]
    public void AcceptsOnlyWhenEveryCriticalConclusionPlanIsAcceptable()
    {
        CriticalInvestigationConclusionPlanPolicyTests.PlanInput input =
            CriticalInvestigationConclusionPlanPolicyTests.Input();
        InvestigationConclusionEvidencePlan second =
            CriticalInvestigationConclusionPlanSnapshotTests.Plan("second-conclusion");
        CriticalInvestigationConclusionPlanSnapshot plans = new([input.Plan, second]);

        Assert.True(CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(
            plans,
            input.Sources));
    }

    [Fact]
    public void RejectsEmptyOrAnyUnacceptableCriticalPlan()
    {
        CriticalInvestigationConclusionPlanPolicyTests.PlanInput input =
            CriticalInvestigationConclusionPlanPolicyTests.Input();
        InvestigationConclusion conclusion = input.Plan.Conclusion;
        InvestigationConclusionEvidencePlan unacceptable = new(
            conclusion,
            [new InvestigationConclusionEvidenceRoute(
                conclusion,
                [new EvidenceId("laundry-residue")])]);

        Assert.False(CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(
            new CriticalInvestigationConclusionPlanSnapshot([]),
            input.Sources));
        Assert.False(CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(
            new CriticalInvestigationConclusionPlanSnapshot([unacceptable]),
            input.Sources));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        CriticalInvestigationConclusionPlanPolicyTests.PlanInput input =
            CriticalInvestigationConclusionPlanPolicyTests.Input();
        CriticalInvestigationConclusionPlanSnapshot plans = new([input.Plan]);

        Assert.Throws<ArgumentNullException>(() =>
            CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(null!, input.Sources));
        Assert.Throws<ArgumentNullException>(() =>
            CriticalInvestigationConclusionScenarioPolicy.IsAcceptable(plans, null!));

        Type policyType = typeof(CriticalInvestigationConclusionScenarioPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsAcceptable", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(CriticalInvestigationConclusionPlanSnapshot), typeof(InvestigationEvidenceSourceSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
