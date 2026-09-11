using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class PreparedIngredientBatchSnapshotAppendTransitionTests
{
    [Fact]
    public void AppendsOneExactCompletedPreparationInStableOrder()
    {
        PreparedIngredientBatch existing = Batch(4);
        IngredientPreparationOutcome preparation =
            IngredientDosageTests.Outcome(6, IngredientQuantityUnit.Millilitre);
        PreparedIngredientBatchSnapshot snapshot = new([existing]);

        PreparedIngredientBatchSnapshot result =
            PreparedIngredientBatchSnapshotAppendTransition.Append(preparation, snapshot);

        Assert.Collection(
            result.Batches,
            batch => Assert.Same(existing, batch),
            batch =>
            {
                Assert.Same(preparation, batch.Preparation);
                Assert.Same(preparation.Quantity, batch.RemainingQuantity);
            });
        Assert.Same(existing, Assert.Single(snapshot.Batches));
    }

    [Fact]
    public void RejectsDuplicateFullAndNullInputsWithoutChangingTheSnapshot()
    {
        PreparedIngredientBatch existing = Batch(4);
        PreparedIngredientBatchSnapshot snapshot = new([existing]);
        PreparedIngredientBatchSnapshot full = new(
            Enumerable.Range(0, PreparedIngredientBatchSnapshot.MaximumBatchCount)
                .Select(index => Batch(index + 1)));
        IngredientPreparationOutcome additional =
            IngredientDosageTests.Outcome(5, IngredientQuantityUnit.Millilitre);

        Assert.Throws<InvalidOperationException>(() =>
            PreparedIngredientBatchSnapshotAppendTransition.Append(
                existing.Preparation,
                snapshot));
        Assert.Throws<InvalidOperationException>(() =>
            PreparedIngredientBatchSnapshotAppendTransition.Append(additional, full));
        Assert.Throws<ArgumentNullException>(() =>
            PreparedIngredientBatchSnapshotAppendTransition.Append(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() =>
            PreparedIngredientBatchSnapshotAppendTransition.Append(additional, null!));
        Assert.Same(existing, Assert.Single(snapshot.Batches));
        Assert.Equal(PreparedIngredientBatchSnapshot.MaximumBatchCount, full.Batches.Count);
    }

    [Fact]
    public void ExposesOnePureStaticAppendOperation()
    {
        Type transitionType = typeof(PreparedIngredientBatchSnapshotAppendTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Append", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(PreparedIngredientBatchSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(IngredientPreparationOutcome), typeof(PreparedIngredientBatchSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static PreparedIngredientBatch Batch(int amount) => new(
        IngredientDosageTests.Outcome(amount, IngredientQuantityUnit.Millilitre));
}
