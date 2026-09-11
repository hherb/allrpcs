using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientQuantityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(IngredientQuantity.MaximumAmount)]
    public void RetainsBoundedIntegerAmountAndExactUnit(int amount)
    {
        IngredientQuantityUnit unit = IngredientQuantityUnit.Milligram;

        IngredientQuantity quantity = new(amount, unit);

        Assert.Equal(amount, quantity.Amount);
        Assert.Same(unit, quantity.Unit);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(IngredientQuantity.MaximumAmount + 1)]
    [InlineData(int.MaxValue)]
    public void RejectsAmountsOutsideThePositiveBound(int amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IngredientQuantity(amount, IngredientQuantityUnit.Count));
    }

    [Fact]
    public void RejectsNullUnit()
    {
        Assert.Throws<ArgumentNullException>(() => new IngredientQuantity(1, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableQuantitySurface()
    {
        Type quantityType = typeof(IngredientQuantity);
        System.Reflection.PropertyInfo[] properties = quantityType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(quantityType.GetConstructors());

        Assert.Collection(
            properties.OrderBy(property => property.Name, StringComparer.Ordinal),
            property =>
            {
                Assert.Equal("Amount", property.Name);
                Assert.Equal(typeof(int), property.PropertyType);
                Assert.False(property.CanWrite);
            },
            property =>
            {
                Assert.Equal("Unit", property.Name);
                Assert.Equal(typeof(IngredientQuantityUnit), property.PropertyType);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(int), parameter.ParameterType),
            parameter => Assert.Equal(typeof(IngredientQuantityUnit), parameter.ParameterType));
        Assert.Empty(quantityType.GetEvents());
        Assert.True(quantityType.IsSealed);
    }
}
