using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.Tests.Diseases;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class FirstPatientSaveStateTests
{
    [Fact]
    public void RetainsEveryExactFirstPatientSnapshot()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        FirstPatientSaveState state = new(
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);

        Assert.Same(diseaseTruth, state.DiseaseTruth);
        Assert.Same(preparedBatches, state.PreparedBatches);
        Assert.Same(administrationExperiments, state.AdministrationExperiments);
        Assert.Same(experiments, state.Experiments);
    }

    [Fact]
    public void EqualityUsesTheRetainedSnapshotInstances()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);
        FirstPatientSaveState first = new(
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);
        FirstPatientSaveState same = new(
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);
        FirstPatientSaveState distinctEmptySnapshots = new(
            diseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, distinctEmptySnapshots);
    }

    [Fact]
    public void RejectsEveryNullSnapshot()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<ArgumentNullException>(() => new FirstPatientSaveState(
            null!, preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientSaveState(
            diseaseTruth, null!, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientSaveState(
            diseaseTruth, preparedBatches, null!, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientSaveState(
            diseaseTruth, preparedBatches, administrationExperiments, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableFirstPatientStateSurface()
    {
        Type stateType = typeof(FirstPatientSaveState);
        (string Name, Type Type)[] expectedProperties =
        [
            ("AdministrationExperiments", typeof(IngredientExperimentJournalSnapshot)),
            ("DiseaseTruth", typeof(DiseaseTruthSnapshot)),
            ("Experiments", typeof(ExperimentJournalSnapshot)),
            ("PreparedBatches", typeof(PreparedIngredientBatchSnapshot)),
        ];

        System.Reflection.PropertyInfo[] properties = stateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedProperties, properties.Select(property =>
            (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(stateType.GetEvents());
        Assert.True(stateType.IsSealed);
    }
}
