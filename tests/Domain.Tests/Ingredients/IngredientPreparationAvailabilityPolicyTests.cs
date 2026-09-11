using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationAvailabilityPolicyTests
{
    [Fact]
    public void AcceptsExactDefinitionsWithMatchingUnitsAndSufficientAmounts()
    {
        IngredientDefinition leaf = Definition("moonwort-leaf");
        IngredientDefinition moss = Definition("marsh-moss");
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(leaf, 250, IngredientQuantityUnit.Milligram),
            Requirement(moss, 2, IngredientQuantityUnit.Count));
        IngredientState first = State(leaf, 250, IngredientQuantityUnit.Milligram);
        IngredientState second = State(moss, 3, IngredientQuantityUnit.Count);
        IngredientStateSnapshot ingredients = new([first, second]);

        bool available = IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
            recipe,
            ingredients);

        Assert.True(available);
        Assert.Same(first, ingredients.States[0]);
        Assert.Same(second, ingredients.States[1]);
        Assert.Same(leaf, recipe.Inputs[0].Definition);
        Assert.Same(moss, recipe.Inputs[1].Definition);
    }

    [Fact]
    public void RejectsMissingValueEqualDefinitionWrongUnitAndInsufficientAmount()
    {
        IngredientDefinition exact = Definition("moonwort-leaf");
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(exact, 250, IngredientQuantityUnit.Milligram));

        Assert.False(IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
            recipe,
            new IngredientStateSnapshot([])));
        Assert.False(IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
            recipe,
            new IngredientStateSnapshot(
                [State(Definition("moonwort-leaf"), 250, IngredientQuantityUnit.Milligram)])));
        Assert.False(IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
            recipe,
            new IngredientStateSnapshot([State(exact, 250, IngredientQuantityUnit.Millilitre)])));
        Assert.False(IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
            recipe,
            new IngredientStateSnapshot([State(exact, 249, IngredientQuantityUnit.Milligram)])));
    }

    [Theory]
    [InlineData("aging", "damaged", "suspected")]
    [InlineData("spoiled", "ruined", "contaminated")]
    public void DoesNotInferUnauthoredQualityRequirements(
        string freshness,
        string condition,
        string contamination)
    {
        IngredientDefinition exact = Definition("moonwort-leaf");
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(exact, 1, IngredientQuantityUnit.Count));
        IngredientState state = new(
            exact,
            new IngredientQuantity(1, IngredientQuantityUnit.Count),
            IngredientFreshness.Parse(freshness),
            IngredientPhysicalCondition.Parse(condition),
            IngredientContamination.Parse(contamination));

        Assert.True(IngredientPreparationAvailabilityPolicy.HasRequiredInputs(
            recipe,
            new IngredientStateSnapshot([state])));
    }

    [Fact]
    public void RejectsNullsAndExposesOnePureStaticOperation()
    {
        IngredientDefinition exact = Definition("moonwort-leaf");
        IngredientPreparationRecipe recipe = Recipe(
            Requirement(exact, 1, IngredientQuantityUnit.Count));
        IngredientStateSnapshot ingredients = new(
            [State(exact, 1, IngredientQuantityUnit.Count)]);

        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationAvailabilityPolicy.HasRequiredInputs(null!, ingredients));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationAvailabilityPolicy.HasRequiredInputs(recipe, null!));

        Type policyType = typeof(IngredientPreparationAvailabilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("HasRequiredInputs", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(IngredientPreparationRecipe), typeof(IngredientStateSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
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
            IngredientFreshness.Fresh,
            IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated);
}
