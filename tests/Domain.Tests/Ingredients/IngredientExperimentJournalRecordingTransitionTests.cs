using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientExperimentJournalRecordingTransitionTests
{
    [Fact]
    public void AppendsTheExactRecordAfterEveryExistingRecord()
    {
        IngredientAdministrationExperimentRecord first =
            IngredientExperimentJournalSnapshotTests.Record("first-dose");
        IngredientAdministrationExperimentRecord second =
            IngredientExperimentJournalSnapshotTests.Record("second-dose");
        IngredientExperimentJournalSnapshot journal = new([first]);

        IngredientExperimentJournalSnapshot result =
            IngredientExperimentJournalRecordingTransition.Record(second, journal);

        Assert.NotSame(journal, result);
        Assert.Collection(
            result.Records,
            record => Assert.Same(first, record),
            record => Assert.Same(second, record));
        Assert.Same(first, Assert.Single(journal.Records));
    }

    [Fact]
    public void RejectsDuplicateCapacityAndNullInputsWithoutChangingTheJournal()
    {
        IngredientAdministrationExperimentRecord retained =
            IngredientExperimentJournalSnapshotTests.Record("first-dose");
        IngredientExperimentJournalSnapshot journal = new([retained]);
        IngredientAdministrationExperimentRecord duplicate =
            IngredientExperimentJournalSnapshotTests.Record("first-dose");
        IngredientExperimentJournalSnapshot full = new(
            Enumerable
                .Range(0, IngredientExperimentJournalSnapshot.MaximumRecordCount)
                .Select(index => IngredientExperimentJournalSnapshotTests.Record(
                    $"experiment-{index}")));

        Assert.Throws<ArgumentException>(() =>
            IngredientExperimentJournalRecordingTransition.Record(duplicate, journal));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IngredientExperimentJournalRecordingTransition.Record(
                IngredientExperimentJournalSnapshotTests.Record("extra-record"),
                full));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientExperimentJournalRecordingTransition.Record(null!, journal));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientExperimentJournalRecordingTransition.Record(retained, null!));
        Assert.Same(retained, Assert.Single(journal.Records));
    }

    [Fact]
    public void ExposesOnePureStaticOperation()
    {
        Type transitionType = typeof(IngredientExperimentJournalRecordingTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Record", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IngredientExperimentJournalSnapshot), method.ReturnType);
        Assert.Equal(
            [
                typeof(IngredientAdministrationExperimentRecord),
                typeof(IngredientExperimentJournalSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
