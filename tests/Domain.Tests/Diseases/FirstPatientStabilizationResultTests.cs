using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientStabilizationResultTests
{
    [Fact]
    public void RetainsTheExactDiseaseTruthAndExperimentJournal()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        IngredientExperimentJournalSnapshot journal = new([]);

        FirstPatientStabilizationResult result = new(diseaseTruth, journal);

        Assert.Same(diseaseTruth, result.DiseaseTruth);
        Assert.Same(journal, result.ExperimentJournal);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableResultSurface()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        IngredientExperimentJournalSnapshot journal = new([]);

        Assert.Throws<ArgumentNullException>(() => new FirstPatientStabilizationResult(null!, journal));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientStabilizationResult(diseaseTruth, null!));

        Type resultType = typeof(FirstPatientStabilizationResult);
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());

        Assert.Equal(
            ["DiseaseTruth", "ExperimentJournal"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(DiseaseTruthSnapshot), typeof(IngredientExperimentJournalSnapshot)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(resultType.GetEvents());
        Assert.Empty(resultType.GetMethods().Where(
            method => method.DeclaringType == resultType && !method.IsSpecialName));
        Assert.True(resultType.IsSealed);
    }
}
