using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientTreatmentContextPolicyTests
{
    [Fact]
    public void AcceptsTheExactAuthoredEvidenceConfirmedRemedy()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.True(FirstPatientTreatmentContextPolicy.IsAuthorized(context));
    }

    [Fact]
    public void RejectsReplacementRemedyOrConclusionAndMissingConfirmation()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        InvestigationConclusion exactConclusion = context.EvidenceRequirement.Conclusion;
        InvestigationConclusion replacementConclusion = new(
            new InvestigationConclusionId(exactConclusion.ConclusionId.Value),
            exactConclusion.Question,
            exactConclusion.Hypothesis,
            exactConclusion.Statement);
        StabilizingRemedyDefinition replacementRemedy = new(
            context.Scenario.Remedy.Disease,
            context.Scenario.Remedy.Recipe);

        Assert.False(FirstPatientTreatmentContextPolicy.IsAuthorized(new FirstPatientTreatmentContext(
            context.Scenario,
            new StabilizingRemedyEvidenceRequirement(replacementRemedy, exactConclusion),
            context.Conclusions)));
        Assert.False(FirstPatientTreatmentContextPolicy.IsAuthorized(new FirstPatientTreatmentContext(
            context.Scenario,
            new StabilizingRemedyEvidenceRequirement(context.Scenario.Remedy, replacementConclusion),
            new InvestigationConclusionSnapshot([replacementConclusion]))));
        Assert.False(FirstPatientTreatmentContextPolicy.IsAuthorized(new FirstPatientTreatmentContext(
            context.Scenario,
            context.EvidenceRequirement,
            new InvestigationConclusionSnapshot([]))));
    }

    [Fact]
    public void RejectsNullAndExposesOnlyThePurePolicySurface()
    {
        Assert.Throws<ArgumentNullException>(() => FirstPatientTreatmentContextPolicy.IsAuthorized(null!));

        Type policyType = typeof(FirstPatientTreatmentContextPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsAuthorized", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(FirstPatientTreatmentContext)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }
}
