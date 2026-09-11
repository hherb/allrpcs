using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientTreatmentContextTests
{
    [Fact]
    public void RetainsEveryExactTreatmentInput()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        FirstPatientTreatmentContext retained = new(
            context.Scenario,
            context.EvidenceRequirement,
            context.Conclusions);

        Assert.Same(context.Scenario, retained.Scenario);
        Assert.Same(context.EvidenceRequirement, retained.EvidenceRequirement);
        Assert.Same(context.Conclusions, retained.Conclusions);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableContextSurface()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<ArgumentNullException>(() => new FirstPatientTreatmentContext(
            null!, context.EvidenceRequirement, context.Conclusions));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientTreatmentContext(
            context.Scenario, null!, context.Conclusions));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientTreatmentContext(
            context.Scenario, context.EvidenceRequirement, null!));

        Type contextType = typeof(FirstPatientTreatmentContext);
        System.Reflection.PropertyInfo[] properties = contextType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(contextType.GetConstructors());

        Assert.Equal(
            ["Conclusions", "EvidenceRequirement", "Scenario"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(FirstPatientScenario),
                typeof(StabilizingRemedyEvidenceRequirement),
                typeof(InvestigationConclusionSnapshot),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(contextType.GetEvents());
        Assert.Empty(contextType.GetMethods().Where(
            method => method.DeclaringType == contextType && !method.IsSpecialName));
        Assert.True(contextType.IsSealed);
    }
}
