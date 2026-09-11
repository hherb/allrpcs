using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationCompletionPolicyTests
{
    [Fact]
    public void AcceptsOnlyAnActivityWithEveryAuthoredStepCompleted()
    {
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();
        IngredientPreparationActivity incomplete = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, recipe.Plan.Steps.Count - 1));
        IngredientPreparationActivity complete = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, recipe.Plan.Steps.Count));

        Assert.False(IngredientPreparationCompletionPolicy.CanComplete(incomplete));
        Assert.True(IngredientPreparationCompletionPolicy.CanComplete(complete));
    }

    [Fact]
    public void RejectsNullAndExposesOnePureStaticOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IngredientPreparationCompletionPolicy.CanComplete(null!));

        Type policyType = typeof(IngredientPreparationCompletionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanComplete", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(IngredientPreparationActivity), parameter.ParameterType));
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
