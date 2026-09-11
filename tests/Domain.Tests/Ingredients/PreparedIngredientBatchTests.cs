using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class PreparedIngredientBatchTests
{
    [Fact]
    public void StartsWithTheExactCompletedPreparationAndFullQuantity()
    {
        IngredientPreparationOutcome preparation = IngredientDosageTests.Outcome(
            4,
            IngredientQuantityUnit.Millilitre);

        PreparedIngredientBatch batch = new(preparation);

        Assert.Same(preparation, batch.Preparation);
        Assert.Same(preparation.Quantity, batch.RemainingQuantity);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyImmutableBatchState()
    {
        Assert.Throws<ArgumentNullException>(() => new PreparedIngredientBatch(null!));

        Type batchType = typeof(PreparedIngredientBatch);
        Assert.Equal(
            ["Preparation", "RemainingQuantity"],
            batchType.GetProperties().Select(property => property.Name).Order());
        Assert.All(batchType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(IngredientPreparationOutcome)],
            Assert.Single(batchType.GetConstructors())
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray());
        Assert.Empty(batchType.GetEvents());
        Assert.Empty(batchType.GetMethods().Where(
            method => method.DeclaringType == batchType && !method.IsSpecialName));
        Assert.True(batchType.IsSealed);
    }

    [Fact]
    public void ConsumptionPolicyRequiresTheExactPreparationUnitAndAvailableAmount()
    {
        IngredientPreparationOutcome preparation = IngredientDosageTests.Outcome(
            4,
            IngredientQuantityUnit.Millilitre);
        PreparedIngredientBatch batch = new(preparation);
        IngredientDosage firstDose = new(
            preparation,
            new IngredientQuantity(3, IngredientQuantityUnit.Millilitre));
        PreparedIngredientBatch remaining = Assert.IsType<PreparedIngredientBatch>(
            PreparedIngredientBatchConsumptionTransition.Consume(firstDose, batch));
        IngredientDosage finalDose = new(
            preparation,
            new IngredientQuantity(1, IngredientQuantityUnit.Millilitre));
        IngredientDosage excessiveDose = new(
            preparation,
            new IngredientQuantity(2, IngredientQuantityUnit.Millilitre));
        IngredientPreparationOutcome replacementPreparation = IngredientDosageTests.Outcome(
            4,
            IngredientQuantityUnit.Millilitre);
        IngredientDosage replacementDose = new(
            replacementPreparation,
            new IngredientQuantity(1, IngredientQuantityUnit.Millilitre));

        Assert.True(PreparedIngredientBatchConsumptionPolicy.CanConsume(firstDose, batch));
        Assert.True(PreparedIngredientBatchConsumptionPolicy.CanConsume(finalDose, remaining));
        Assert.False(PreparedIngredientBatchConsumptionPolicy.CanConsume(excessiveDose, remaining));
        Assert.False(PreparedIngredientBatchConsumptionPolicy.CanConsume(replacementDose, batch));
        Assert.Throws<ArgumentNullException>(() =>
            PreparedIngredientBatchConsumptionPolicy.CanConsume(null!, batch));
        Assert.Throws<ArgumentNullException>(() =>
            PreparedIngredientBatchConsumptionPolicy.CanConsume(firstDose, null!));
    }

    [Fact]
    public void ConsumptionReturnsExactRemainingStateOrConsumesTheBatchCompletely()
    {
        IngredientPreparationOutcome preparation = IngredientDosageTests.Outcome(
            4,
            IngredientQuantityUnit.Millilitre);
        PreparedIngredientBatch batch = new(preparation);
        IngredientDosage partialDose = new(
            preparation,
            new IngredientQuantity(1, IngredientQuantityUnit.Millilitre));

        PreparedIngredientBatch remaining = Assert.IsType<PreparedIngredientBatch>(
            PreparedIngredientBatchConsumptionTransition.Consume(partialDose, batch));

        Assert.Same(preparation, remaining.Preparation);
        Assert.Equal(3, remaining.RemainingQuantity.Amount);
        Assert.Same(preparation.Quantity.Unit, remaining.RemainingQuantity.Unit);
        Assert.Same(preparation, batch.Preparation);
        Assert.Same(preparation.Quantity, batch.RemainingQuantity);

        IngredientDosage finalDose = new(
            preparation,
            new IngredientQuantity(3, IngredientQuantityUnit.Millilitre));
        Assert.Null(PreparedIngredientBatchConsumptionTransition.Consume(finalDose, remaining));
    }

    [Fact]
    public void ConsumptionRejectsIneligibleDosageWithoutChangingTheBatch()
    {
        IngredientPreparationOutcome preparation = IngredientDosageTests.Outcome(
            4,
            IngredientQuantityUnit.Millilitre);
        PreparedIngredientBatch batch = new(preparation);
        IngredientDosage firstDose = new(
            preparation,
            new IngredientQuantity(3, IngredientQuantityUnit.Millilitre));
        PreparedIngredientBatch remaining = Assert.IsType<PreparedIngredientBatch>(
            PreparedIngredientBatchConsumptionTransition.Consume(firstDose, batch));
        IngredientDosage excessiveDose = new(
            preparation,
            new IngredientQuantity(2, IngredientQuantityUnit.Millilitre));

        Assert.Throws<InvalidOperationException>(() =>
            PreparedIngredientBatchConsumptionTransition.Consume(excessiveDose, remaining));
        Assert.Throws<ArgumentNullException>(() =>
            PreparedIngredientBatchConsumptionTransition.Consume(null!, remaining));
        Assert.Throws<ArgumentNullException>(() =>
            PreparedIngredientBatchConsumptionTransition.Consume(firstDose, null!));
        Assert.Equal(1, remaining.RemainingQuantity.Amount);
        Assert.Same(preparation, remaining.Preparation);
    }
}
