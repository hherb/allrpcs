using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Tests.Ingredients;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class StabilizingRemedyDefinitionTests
{
    [Fact]
    public void RetainsTheExactDiseaseAndRecipe()
    {
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();

        StabilizingRemedyDefinition remedy = new(disease, recipe);

        Assert.Same(disease, remedy.Disease);
        Assert.Same(recipe, remedy.Recipe);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableDefinitionSurface()
    {
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        IngredientPreparationRecipe recipe = IngredientPreparationActivityTests.Recipe();

        Assert.Throws<ArgumentNullException>(() => new StabilizingRemedyDefinition(null!, recipe));
        Assert.Throws<ArgumentNullException>(() => new StabilizingRemedyDefinition(disease, null!));

        Type remedyType = typeof(StabilizingRemedyDefinition);
        System.Reflection.PropertyInfo[] properties = remedyType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(remedyType.GetConstructors());

        Assert.Equal(["Disease", "Recipe"], properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(DiseaseDefinition), typeof(IngredientPreparationRecipe)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(remedyType.GetEvents());
        Assert.Empty(remedyType.GetMethods().Where(
            method => method.DeclaringType == remedyType && !method.IsSpecialName));
        Assert.True(remedyType.IsSealed);
    }
}
