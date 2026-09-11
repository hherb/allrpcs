using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationRecipeTests
{
    [Fact]
    public void CopiesInputsAndRetainsEveryExactAuthoredValue()
    {
        PreparationRecipeId recipeId = new("cooling-tonic");
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationInputRequirement first = Requirement("moonwort-leaf", 250);
        IngredientPreparationInputRequirement second = Requirement("marsh-moss", 50);
        List<IngredientPreparationInputRequirement> supplied = [first, second];
        IngredientDefinition resultDefinition = new(new IngredientId("cooling-tonic"));
        IngredientQuantity resultQuantity = new(1, IngredientQuantityUnit.Count);

        IngredientPreparationRecipe recipe = new(
            recipeId,
            plan,
            supplied,
            resultDefinition,
            resultQuantity);
        supplied.Clear();

        Assert.Same(recipeId, recipe.RecipeId);
        Assert.Same(plan, recipe.Plan);
        Assert.Collection(
            recipe.Inputs,
            input => Assert.Same(first, input),
            input => Assert.Same(second, input));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<IngredientPreparationInputRequirement>)recipe.Inputs).Add(first));
        Assert.Same(resultDefinition, recipe.ResultDefinition);
        Assert.Same(resultQuantity, recipe.ResultQuantity);
    }

    [Fact]
    public void AcceptsTheMaximumDistinctInputCount()
    {
        IngredientPreparationInputRequirement[] inputs = Enumerable
            .Range(0, IngredientPreparationRecipe.MaximumInputCount)
            .Select(index => Requirement($"ingredient-{index}", 1))
            .ToArray();

        IngredientPreparationRecipe recipe = Recipe(inputs);

        Assert.Equal(IngredientPreparationRecipe.MaximumInputCount, recipe.Inputs.Count);
    }

    [Fact]
    public void RejectsNullEmptyNullEntryExcessiveAndDuplicateInputs()
    {
        IngredientPreparationInputRequirement input = Requirement("moonwort-leaf", 1);
        IngredientPreparationInputRequirement duplicate = Requirement("moonwort-leaf", 2);
        IngredientPreparationInputRequirement[] excessive = Enumerable
            .Range(0, IngredientPreparationRecipe.MaximumInputCount + 1)
            .Select(index => Requirement($"ingredient-{index}", 1))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() => Recipe(null!));
        Assert.Throws<ArgumentException>(() => Recipe([]));
        Assert.Throws<ArgumentException>(() => Recipe([null!]));
        Assert.Throws<ArgumentException>(() => Recipe([input, duplicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Recipe(excessive));
    }

    [Fact]
    public void RejectsNullExactAuthoredValues()
    {
        PreparationRecipeId recipeId = new("cooling-tonic");
        IngredientPreparationPlan plan = IngredientPreparationProgressTests.Plan();
        IngredientPreparationInputRequirement[] inputs = [Requirement("moonwort-leaf", 1)];
        IngredientDefinition resultDefinition = new(new IngredientId("cooling-tonic"));
        IngredientQuantity resultQuantity = new(1, IngredientQuantityUnit.Count);

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationRecipe(null!, plan, inputs, resultDefinition, resultQuantity));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationRecipe(recipeId, null!, inputs, resultDefinition, resultQuantity));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationRecipe(recipeId, plan, inputs, null!, resultQuantity));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationRecipe(recipeId, plan, inputs, resultDefinition, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableRecipeSurface()
    {
        Type recipeType = typeof(IngredientPreparationRecipe);
        System.Reflection.PropertyInfo[] properties = recipeType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(recipeType.GetConstructors());

        Assert.Equal(
            ["Inputs", "Plan", "RecipeId", "ResultDefinition", "ResultQuantity"],
            properties.Select(property => property.Name).Order().ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(PreparationRecipeId),
                typeof(IngredientPreparationPlan),
                typeof(IEnumerable<IngredientPreparationInputRequirement>),
                typeof(IngredientDefinition),
                typeof(IngredientQuantity),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(recipeType.GetEvents());
        Assert.Empty(recipeType.GetMethods().Where(
            method => method.DeclaringType == recipeType && !method.IsSpecialName));
        Assert.True(recipeType.IsSealed);
    }

    private static IngredientPreparationRecipe Recipe(
        IEnumerable<IngredientPreparationInputRequirement> inputs) =>
        new(
            new PreparationRecipeId("cooling-tonic"),
            IngredientPreparationProgressTests.Plan(),
            inputs,
            new IngredientDefinition(new IngredientId("cooling-tonic")),
            new IngredientQuantity(1, IngredientQuantityUnit.Count));

    private static IngredientPreparationInputRequirement Requirement(string id, int amount) =>
        new(
            new IngredientDefinition(new IngredientId(id)),
            new IngredientQuantity(amount, IngredientQuantityUnit.Milligram));
}
