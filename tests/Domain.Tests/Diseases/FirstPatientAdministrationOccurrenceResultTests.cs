using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientAdministrationOccurrenceResultTests
{
    [Fact]
    public void RetainsTheExactWorldEventAndFirstPatientSnapshots()
    {
        DiseaseTruthSnapshot diseaseTruth =
            StabilizingRemedyAdministrationPolicyTests.Input().Scenario.DiseaseTruth;
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        FirstPatientAdministrationOccurrenceResult result = new(
            eventLog,
            worldTime,
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);

        Assert.Same(eventLog, result.EventLog);
        Assert.Same(worldTime, result.WorldTime);
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
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        PreparedIngredientBatchSnapshot preparedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<ArgumentNullException>(() => new FirstPatientAdministrationOccurrenceResult(
            null!, worldTime, diseaseTruth, preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientAdministrationOccurrenceResult(
            eventLog, null!, diseaseTruth, preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientAdministrationOccurrenceResult(
            eventLog, worldTime, null!, preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientAdministrationOccurrenceResult(
            eventLog, worldTime, diseaseTruth, null!, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientAdministrationOccurrenceResult(
            eventLog, worldTime, diseaseTruth, preparedBatches, null!, experiments));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientAdministrationOccurrenceResult(
            eventLog, worldTime, diseaseTruth, preparedBatches, administrationExperiments, null!));

        Type resultType = typeof(FirstPatientAdministrationOccurrenceResult);
        System.Reflection.PropertyInfo[] properties = resultType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());

        Assert.Equal(
            [
                "AdministrationExperiments",
                "DiseaseTruth",
                "EventLog",
                "Experiments",
                "PreparedBatches",
                "WorldTime",
            ],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
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
