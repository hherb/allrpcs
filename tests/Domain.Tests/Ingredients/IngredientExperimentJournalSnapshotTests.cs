using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientExperimentJournalSnapshotTests
{
    [Fact]
    public void CopiesRecordsInStableOrderAndRetainsExactInstances()
    {
        IngredientAdministrationExperimentRecord first = Record("first-dose");
        IngredientAdministrationExperimentRecord second = Record("second-dose");
        IngredientAdministrationExperimentRecord replacement = Record("replacement");
        IngredientAdministrationExperimentRecord[] supplied = [first, second];

        IngredientExperimentJournalSnapshot journal = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            journal.Records,
            record => Assert.Same(first, record),
            record => Assert.Same(second, record));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<IngredientAdministrationExperimentRecord>)journal.Records).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndTheMaximumDistinctRecordCount()
    {
        IngredientAdministrationExperimentRecord[] maximum = Enumerable
            .Range(0, IngredientExperimentJournalSnapshot.MaximumRecordCount)
            .Select(index => Record($"experiment-{index}"))
            .ToArray();

        Assert.Empty(new IngredientExperimentJournalSnapshot([]).Records);
        Assert.Equal(
            IngredientExperimentJournalSnapshot.MaximumRecordCount,
            new IngredientExperimentJournalSnapshot(maximum).Records.Count);
    }

    [Fact]
    public void RejectsNullsExcessiveRecordsAndDuplicateExperimentIdentities()
    {
        IngredientAdministrationExperimentRecord[] excessive = Enumerable
            .Range(0, IngredientExperimentJournalSnapshot.MaximumRecordCount + 1)
            .Select(index => Record($"experiment-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientExperimentJournalSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new IngredientExperimentJournalSnapshot([null!]));
        Assert.Throws<ArgumentException>(() =>
            new IngredientExperimentJournalSnapshot([Record("first-dose"), Record("first-dose")]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IngredientExperimentJournalSnapshot(excessive));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlyJournalSurface()
    {
        Type journalType = typeof(IngredientExperimentJournalSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(journalType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(journalType.GetConstructors());

        Assert.Equal("Records", property.Name);
        Assert.Equal(
            typeof(IReadOnlyList<IngredientAdministrationExperimentRecord>),
            property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<IngredientAdministrationExperimentRecord>),
                parameter.ParameterType));
        Assert.Empty(journalType.GetEvents());
        Assert.Empty(journalType.GetMethods().Where(
            method => method.DeclaringType == journalType && !method.IsSpecialName));
        Assert.True(journalType.IsSealed);
    }

    internal static IngredientAdministrationExperimentRecord Record(string experimentId) =>
        new(
            new ExperimentId(experimentId),
            IngredientAdministrationExperimentRecordTests.Administration(),
            new CharacterConditionSet([]));
}
