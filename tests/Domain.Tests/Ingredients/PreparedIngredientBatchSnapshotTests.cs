using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class PreparedIngredientBatchSnapshotTests
{
    [Fact]
    public void CopiesBatchesInStableOrderAndRetainsExactInstances()
    {
        PreparedIngredientBatch first = Batch(4);
        PreparedIngredientBatch second = Batch(6);
        PreparedIngredientBatch replacement = Batch(8);
        PreparedIngredientBatch[] supplied = [first, second];

        PreparedIngredientBatchSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Batches,
            batch => Assert.Same(first, batch),
            batch => Assert.Same(second, batch));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<PreparedIngredientBatch>)snapshot.Batches).Add(replacement));
    }

    [Fact]
    public void RejectsNullDuplicatePreparationAndExcessiveBatches()
    {
        PreparedIngredientBatch duplicate = Batch(4);
        PreparedIngredientBatch[] maximum = Enumerable
            .Range(0, PreparedIngredientBatchSnapshot.MaximumBatchCount)
            .Select(index => Batch(index + 1))
            .ToArray();

        Assert.Empty(new PreparedIngredientBatchSnapshot([]).Batches);
        Assert.Equal(
            PreparedIngredientBatchSnapshot.MaximumBatchCount,
            new PreparedIngredientBatchSnapshot(maximum).Batches.Count);
        Assert.Throws<ArgumentNullException>(() => new PreparedIngredientBatchSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new PreparedIngredientBatchSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new PreparedIngredientBatchSnapshot(
            [duplicate, new PreparedIngredientBatch(duplicate.Preparation)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PreparedIngredientBatchSnapshot(
            maximum.Append(Batch(PreparedIngredientBatchSnapshot.MaximumBatchCount + 1))));
    }

    [Fact]
    public void FindsAndConsumesOnlyTheExactPreparedBatchInStableOrder()
    {
        PreparedIngredientBatch first = Batch(4);
        PreparedIngredientBatch target = Batch(5);
        PreparedIngredientBatch last = Batch(6);
        PreparedIngredientBatchSnapshot snapshot = new([first, target, last]);
        IngredientDosage partialDose = new(
            target.Preparation,
            new IngredientQuantity(2, IngredientQuantityUnit.Millilitre));

        Assert.True(PreparedIngredientBatchSnapshotConsumptionPolicy.CanConsume(
            partialDose,
            snapshot));
        PreparedIngredientBatchSnapshot partial =
            PreparedIngredientBatchSnapshotConsumptionTransition.Consume(
                partialDose,
                snapshot);

        Assert.Collection(
            partial.Batches,
            batch => Assert.Same(first, batch),
            batch =>
            {
                Assert.Same(target.Preparation, batch.Preparation);
                Assert.Equal(3, batch.RemainingQuantity.Amount);
            },
            batch => Assert.Same(last, batch));

        IngredientDosage finalDose = new(
            target.Preparation,
            new IngredientQuantity(3, IngredientQuantityUnit.Millilitre));
        PreparedIngredientBatchSnapshot consumed =
            PreparedIngredientBatchSnapshotConsumptionTransition.Consume(finalDose, partial);
        Assert.Collection(
            consumed.Batches,
            batch => Assert.Same(first, batch),
            batch => Assert.Same(last, batch));
        Assert.Collection(
            snapshot.Batches,
            batch => Assert.Same(first, batch),
            batch => Assert.Same(target, batch),
            batch => Assert.Same(last, batch));
    }

    [Fact]
    public void RejectsReplacementMissingAndExcessiveDosagesWithoutMutation()
    {
        PreparedIngredientBatch target = Batch(4);
        PreparedIngredientBatchSnapshot snapshot = new([target]);
        IngredientPreparationOutcome replacementPreparation = IngredientDosageTests.Outcome(
            4,
            IngredientQuantityUnit.Millilitre);
        IngredientDosage replacementDose = new(
            replacementPreparation,
            new IngredientQuantity(1, IngredientQuantityUnit.Millilitre));
        IngredientDosage partialDose = new(
            target.Preparation,
            new IngredientQuantity(3, IngredientQuantityUnit.Millilitre));
        PreparedIngredientBatchSnapshot partial =
            PreparedIngredientBatchSnapshotConsumptionTransition.Consume(partialDose, snapshot);
        IngredientDosage excessiveDose = new(
            target.Preparation,
            new IngredientQuantity(2, IngredientQuantityUnit.Millilitre));

        Assert.False(PreparedIngredientBatchSnapshotConsumptionPolicy.CanConsume(
            replacementDose,
            snapshot));
        Assert.False(PreparedIngredientBatchSnapshotConsumptionPolicy.CanConsume(
            excessiveDose,
            partial));
        Assert.Throws<InvalidOperationException>(() =>
            PreparedIngredientBatchSnapshotConsumptionTransition.Consume(
                replacementDose,
                snapshot));
        Assert.Throws<InvalidOperationException>(() =>
            PreparedIngredientBatchSnapshotConsumptionTransition.Consume(
                excessiveDose,
                partial));
        Assert.Same(target, Assert.Single(snapshot.Batches));
        Assert.Equal(1, Assert.Single(partial.Batches).RemainingQuantity.Amount);
    }

    [Fact]
    public void ExposesOnlyTheBoundedReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(PreparedIngredientBatchSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());

        Assert.Equal("Batches", property.Name);
        Assert.Equal(typeof(IReadOnlyList<PreparedIngredientBatch>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Equal(
            typeof(IEnumerable<PreparedIngredientBatch>),
            Assert.Single(snapshotType.GetConstructors()).GetParameters()[0].ParameterType);
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    private static PreparedIngredientBatch Batch(int amount) => new(
        IngredientDosageTests.Outcome(amount, IngredientQuantityUnit.Millilitre));
}
