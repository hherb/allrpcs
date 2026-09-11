using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationConsumptionTransitionTests
{
    [Fact]
    public void ConsumesRequiredAmountsRemovesDepletedStateAndPreservesStableOrder()
    {
        IngredientDefinition leaf = Definition("moonwort-leaf");
        IngredientDefinition berries = Definition("silver-berries");
        IngredientDefinition moss = Definition("marsh-moss");
        IngredientState first = State(leaf, 300, IngredientQuantityUnit.Milligram);
        IngredientState depleted = State(berries, 2, IngredientQuantityUnit.Count);
        IngredientState unrelated = State(moss, 5, IngredientQuantityUnit.Count);
        IngredientStateSnapshot ingredients = new([first, depleted, unrelated]);
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(leaf, 250, IngredientQuantityUnit.Milligram),
            Requirement(berries, 2, IngredientQuantityUnit.Count));

        IngredientStateSnapshot result = IngredientPreparationConsumptionTransition.Consume(
            recipe,
            ingredients);

        Assert.NotSame(ingredients, result);
        Assert.Collection(
            result.States,
            retained =>
            {
                Assert.NotSame(first, retained);
                Assert.Same(leaf, retained.Definition);
                Assert.Equal(50, retained.Quantity.Amount);
                Assert.Same(IngredientQuantityUnit.Milligram, retained.Quantity.Unit);
                Assert.Same(first.Freshness, retained.Freshness);
                Assert.Same(first.PhysicalCondition, retained.PhysicalCondition);
                Assert.Same(first.Contamination, retained.Contamination);
            },
            retained => Assert.Same(unrelated, retained));
        Assert.Collection(
            ingredients.States,
            retained => Assert.Same(first, retained),
            retained => Assert.Same(depleted, retained),
            retained => Assert.Same(unrelated, retained));
    }

    [Fact]
    public void ReplacesEveryPartiallyConsumedStateInItsOriginalPosition()
    {
        IngredientDefinition leaf = Definition("moonwort-leaf");
        IngredientDefinition moss = Definition("marsh-moss");
        IngredientState first = State(leaf, 3, IngredientQuantityUnit.Count);
        IngredientState second = State(moss, 4, IngredientQuantityUnit.Count);
        IngredientStateSnapshot ingredients = new([first, second]);
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(moss, 1, IngredientQuantityUnit.Count),
            Requirement(leaf, 2, IngredientQuantityUnit.Count));

        IngredientStateSnapshot result = IngredientPreparationConsumptionTransition.Consume(
            recipe,
            ingredients);

        Assert.Collection(
            result.States,
            retained =>
            {
                Assert.Same(leaf, retained.Definition);
                Assert.Equal(1, retained.Quantity.Amount);
            },
            retained =>
            {
                Assert.Same(moss, retained.Definition);
                Assert.Equal(3, retained.Quantity.Amount);
            });
    }

    [Fact]
    public void RejectsUnavailableInputsWithoutChangingTheSnapshot()
    {
        IngredientDefinition exact = Definition("moonwort-leaf");
        IngredientState retained = State(exact, 1, IngredientQuantityUnit.Count);
        IngredientStateSnapshot ingredients = new([retained]);
        IngredientPreparationRecipe excessive = Recipe(
            Requirement(exact, 2, IngredientQuantityUnit.Count));
        IngredientPreparationRecipe impostor = Recipe(
            Requirement(Definition("moonwort-leaf"), 1, IngredientQuantityUnit.Count));

        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationConsumptionTransition.Consume(excessive, ingredients));
        Assert.Throws<InvalidOperationException>(() =>
            IngredientPreparationConsumptionTransition.Consume(impostor, ingredients));
        Assert.Same(retained, Assert.Single(ingredients.States));
    }

    [Fact]
    public void RejectsNullsAndExposesOnePureStaticOperation()
    {
        IngredientDefinition exact = Definition("moonwort-leaf");
        IngredientStateSnapshot ingredients = new(
            [State(exact, 1, IngredientQuantityUnit.Count)]);
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(exact, 1, IngredientQuantityUnit.Count));

        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationConsumptionTransition.Consume(null!, ingredients));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationConsumptionTransition.Consume(recipe, null!));

        Type transitionType = typeof(IngredientPreparationConsumptionTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Consume", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IngredientStateSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(IngredientPreparationRecipe), typeof(IngredientStateSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static IngredientDefinition Definition(string id) => new(new IngredientId(id));

    private static IngredientPreparationInputRequirement Requirement(
        IngredientDefinition definition,
        int amount,
        IngredientQuantityUnit unit) => new(definition, new IngredientQuantity(amount, unit));

    private static IngredientPreparationRecipe Recipe(
        params IngredientPreparationInputRequirement[] inputs) =>
        new(
            new PreparationRecipeId("cooling-tonic"),
            IngredientPreparationProgressTests.Plan(),
            inputs,
            new IngredientDefinition(new IngredientId("cooling-tonic")),
            new IngredientQuantity(1, IngredientQuantityUnit.Count));

    private static IngredientState State(
        IngredientDefinition definition,
        int amount,
        IngredientQuantityUnit unit) =>
        new(
            definition,
            new IngredientQuantity(amount, unit),
            IngredientFreshness.Aging,
            IngredientPhysicalCondition.Damaged,
            IngredientContamination.Suspected);
}
