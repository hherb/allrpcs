using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationInputRequirementTests
{
    [Fact]
    public void RetainsExactDefinitionAndPositiveQuantity()
    {
        IngredientDefinition definition = new(new IngredientId("moonwort-leaf"));
        IngredientQuantity quantity = new(250, IngredientQuantityUnit.Milligram);

        IngredientPreparationInputRequirement requirement = new(definition, quantity);

        Assert.Same(definition, requirement.Definition);
        Assert.Same(quantity, requirement.Quantity);
    }

    [Fact]
    public void RejectsNullValuesAndExposesOnlyImmutableProperties()
    {
        IngredientDefinition definition = new(new IngredientId("moonwort-leaf"));
        IngredientQuantity quantity = new(1, IngredientQuantityUnit.Count);

        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationInputRequirement(null!, quantity));
        Assert.Throws<ArgumentNullException>(() =>
            new IngredientPreparationInputRequirement(definition, null!));

        Type requirementType = typeof(IngredientPreparationInputRequirement);
        System.Reflection.PropertyInfo[] properties = requirementType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(requirementType.GetConstructors());

        Assert.Collection(
            properties.OrderBy(property => property.Name),
            property =>
            {
                Assert.Equal("Definition", property.Name);
                Assert.Equal(typeof(IngredientDefinition), property.PropertyType);
                Assert.False(property.CanWrite);
            },
            property =>
            {
                Assert.Equal("Quantity", property.Name);
                Assert.Equal(typeof(IngredientQuantity), property.PropertyType);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(IngredientDefinition), parameter.ParameterType),
            parameter => Assert.Equal(typeof(IngredientQuantity), parameter.ParameterType));
        Assert.Empty(requirementType.GetEvents());
        Assert.True(requirementType.IsSealed);
    }
}
