using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class StabilizingRemedyEvidenceRequirementTests
{
    [Fact]
    public void RetainsTheExactRemedyAndConclusion()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        InvestigationConclusion conclusion = Assert.Single(scenario.Investigation.Plans.Plans).Conclusion;

        StabilizingRemedyEvidenceRequirement requirement = new(scenario.Remedy, conclusion);

        Assert.Same(scenario.Remedy, requirement.Remedy);
        Assert.Same(conclusion, requirement.Conclusion);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableRequirementSurface()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        InvestigationConclusion conclusion = Assert.Single(scenario.Investigation.Plans.Plans).Conclusion;

        Assert.Throws<ArgumentNullException>(() =>
            new StabilizingRemedyEvidenceRequirement(null!, conclusion));
        Assert.Throws<ArgumentNullException>(() =>
            new StabilizingRemedyEvidenceRequirement(scenario.Remedy, null!));

        Type requirementType = typeof(StabilizingRemedyEvidenceRequirement);
        System.Reflection.PropertyInfo[] properties = requirementType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(requirementType.GetConstructors());

        Assert.Equal(["Conclusion", "Remedy"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(StabilizingRemedyDefinition), typeof(InvestigationConclusion)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(requirementType.GetEvents());
        Assert.Empty(requirementType.GetMethods().Where(
            method => method.DeclaringType == requirementType && !method.IsSpecialName));
        Assert.True(requirementType.IsSealed);
    }
}
