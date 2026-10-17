using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationPlanTests
{
    [Fact]
    public void CopiesAndRetainsExactStepsInSuppliedOrderIncludingRepeatedKinds()
    {
        IngredientPreparationStep first = new(IngredientPreparationKind.Heating);
        IngredientPreparationStep second = new(IngredientPreparationKind.Cooling);
        IngredientPreparationStep third = new(IngredientPreparationKind.Heating);
        List<IngredientPreparationStep> supplied = [first, second, third];

        IngredientPreparationPlan plan = new(supplied);
        ListIdentityContractTests.AssertIdentity(plan);
        supplied.Clear();

        Assert.Collection(
            plan.Steps,
            step => Assert.Same(first, step),
            step => Assert.Same(second, step),
            step => Assert.Same(third, step));
        Assert.IsAssignableFrom<IReadOnlyList<IngredientPreparationStep>>(plan.Steps);
    }

    [Fact]
    public void AcceptsTheMaximumStepCount()
    {
        IngredientPreparationStep[] steps = Enumerable
            .Range(0, IngredientPreparationPlan.MaximumStepCount)
            .Select(_ => new IngredientPreparationStep(IngredientPreparationKind.Grinding))
            .ToArray();

        IngredientPreparationPlan plan = new(steps);

        Assert.Equal(IngredientPreparationPlan.MaximumStepCount, plan.Steps.Count);
    }

    [Fact]
    public void RejectsNullEmptyNullEntryAndExcessiveSteps()
    {
        Assert.Throws<ArgumentNullException>(() => new IngredientPreparationPlan(null!));
        Assert.Throws<ArgumentException>(() => new IngredientPreparationPlan([]));
        Assert.Throws<ArgumentException>(() => new IngredientPreparationPlan([null!]));

        IngredientPreparationStep[] excessive = Enumerable
            .Range(0, IngredientPreparationPlan.MaximumStepCount + 1)
            .Select(_ => new IngredientPreparationStep(IngredientPreparationKind.Fermentation))
            .ToArray();

        Assert.Throws<ArgumentException>(() => new IngredientPreparationPlan(excessive));
    }

    [Fact]
    public void ExposesOnlyTheImmutablePlanSurface()
    {
        Type planType = typeof(IngredientPreparationPlan);
        System.Reflection.PropertyInfo property = Assert.Single(planType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(planType.GetConstructors());

        Assert.Equal("Steps", property.Name);
        Assert.Equal(typeof(IReadOnlyList<IngredientPreparationStep>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<IngredientPreparationStep>),
                parameter.ParameterType));
        Assert.Empty(planType.GetEvents());
        Assert.True(planType.IsSealed);
    }
}
