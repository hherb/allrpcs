using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientDefinitionTests
{
    [Fact]
    public void RetainsExactIngredientIdentity()
    {
        IngredientId ingredientId = new("moonwort-leaf");

        IngredientDefinition definition = new(ingredientId);

        Assert.Same(ingredientId, definition.IngredientId);
    }

    [Fact]
    public void RejectsNullIdentity()
    {
        Assert.Throws<ArgumentNullException>(() => new IngredientDefinition(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDefinitionSurface()
    {
        Type definitionType = typeof(IngredientDefinition);
        System.Reflection.PropertyInfo property = Assert.Single(definitionType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(definitionType.GetConstructors());

        Assert.Equal("IngredientId", property.Name);
        Assert.Equal(typeof(IngredientId), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(IngredientId), parameter.ParameterType));
        Assert.Empty(definitionType.GetEvents());
        Assert.True(definitionType.IsSealed);
    }
}
