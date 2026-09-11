using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationProgressTests
{
    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(1, false, 1)]
    [InlineData(2, true, null)]
    public void RetainsExactPlanAndExposesOnlyTheNextStep(
        int nextStepIndex,
        bool expectedComplete,
        int? expectedStepIndex)
    {
        IngredientPreparationPlan plan = Plan();

        IngredientPreparationProgress progress = new(plan, nextStepIndex);

        Assert.Same(plan, progress.Plan);
        Assert.Equal(nextStepIndex, progress.NextStepIndex);
        Assert.Equal(expectedComplete, progress.IsComplete);
        if (expectedStepIndex is int index)
        {
            Assert.Same(plan.Steps[index], progress.NextStep);
        }
        else
        {
            Assert.Null(progress.NextStep);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void RejectsAnIndexOutsideThePlanBounds(int nextStepIndex)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IngredientPreparationProgress(Plan(), nextStepIndex));
    }

    [Fact]
    public void RejectsANullPlan()
    {
        Assert.Throws<ArgumentNullException>(() => new IngredientPreparationProgress(null!, 0));
    }

    [Fact]
    public void ExposesOnlyImmutableProgressState()
    {
        Type progressType = typeof(IngredientPreparationProgress);
        (string Name, Type Type)[] expectedProperties =
        [
            ("IsComplete", typeof(bool)),
            ("NextStep", typeof(IngredientPreparationStep)),
            ("NextStepIndex", typeof(int)),
            ("Plan", typeof(IngredientPreparationPlan)),
        ];
        System.Reflection.PropertyInfo[] properties = progressType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(progressType.GetEvents());
        Assert.True(progressType.IsSealed);
    }

    internal static IngredientPreparationPlan Plan() => new(
    [
        new(IngredientPreparationKind.Grinding),
        new(IngredientPreparationKind.Steeping),
    ]);
}
