using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientStabilizationResultTests
{
    [Fact]
    public void RetainsTheExactDiseaseTruthBatchesAndBothExperimentJournals()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        FirstPatientStabilizationResult result = new(
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);

        Assert.Same(diseaseTruth, result.DiseaseTruth);
        Assert.Same(preparedBatches, result.PreparedBatches);
        Assert.Same(administrationExperiments, result.AdministrationExperiments);
        Assert.Same(experiments, result.Experiments);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableResultSurface()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<ArgumentNullException>(() => new FirstPatientStabilizationResult(
            null!, preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientStabilizationResult(
            diseaseTruth, null!, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientStabilizationResult(
            diseaseTruth, preparedBatches, null!, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientStabilizationResult(
            diseaseTruth, preparedBatches, administrationExperiments, null!));

        Type resultType = typeof(FirstPatientStabilizationResult);
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());

        Assert.Equal(
            ["AdministrationExperiments", "DiseaseTruth", "Experiments", "PreparedBatches"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(DiseaseTruthSnapshot),
                typeof(PreparedIngredientBatchSnapshot),
                typeof(IngredientExperimentJournalSnapshot),
                typeof(ExperimentJournalSnapshot),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(resultType.GetEvents());
        Assert.Empty(resultType.GetMethods().Where(
            method => method.DeclaringType == resultType && !method.IsSpecialName));
        Assert.True(resultType.IsSealed);
    }
}
