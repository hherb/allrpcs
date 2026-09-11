using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class RepresentativeFirstPatientTreatmentContextSourceTests
{
    [Fact]
    public void CreatesAnExactEvidenceConfirmedRepresentativeContext()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Same(context.Scenario.Remedy, context.EvidenceRequirement.Remedy);
        Assert.Same(
            Assert.Single(context.Scenario.Investigation.Plans.Plans).Conclusion,
            context.EvidenceRequirement.Conclusion);
        Assert.Same(
            context.EvidenceRequirement.Conclusion,
            Assert.Single(context.Conclusions.Conclusions));
        Assert.True(FirstPatientTreatmentContextPolicy.IsAuthorized(context));
    }

    [Fact]
    public void CreatesFreshIndependentContextsAndExposesOnlyOneFactoryMethod()
    {
        FirstPatientTreatmentContext first = RepresentativeFirstPatientTreatmentContextSource.Create();
        FirstPatientTreatmentContext second = RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.NotSame(first, second);
        Assert.NotSame(first.Scenario, second.Scenario);
        Assert.NotSame(first.EvidenceRequirement, second.EvidenceRequirement);
        Assert.NotSame(first.Conclusions, second.Conclusions);

        Type sourceType = typeof(RepresentativeFirstPatientTreatmentContextSource);
        System.Reflection.MethodInfo method = Assert.Single(sourceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sourceType));

        Assert.Equal("Create", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(FirstPatientTreatmentContext), method.ReturnType);
        Assert.Empty(method.GetParameters());
        Assert.Empty(sourceType.GetProperties());
        Assert.Empty(sourceType.GetEvents());
    }
}
