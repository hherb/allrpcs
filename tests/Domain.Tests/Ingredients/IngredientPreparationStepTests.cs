using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientPreparationStepTests
{
    [Fact]
    public void RetainsTheExactPreparationKind()
    {
        IngredientPreparationKind kind = IngredientPreparationKind.Steeping;

        IngredientPreparationStep step = new(kind);

        Assert.Same(kind, step.Kind);
    }

    [Fact]
    public void RejectsNullKind()
    {
        Assert.Throws<ArgumentNullException>(() => new IngredientPreparationStep(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableStepSurface()
    {
        Type stepType = typeof(IngredientPreparationStep);
        System.Reflection.PropertyInfo property = Assert.Single(stepType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(stepType.GetConstructors());

        Assert.Equal("Kind", property.Name);
        Assert.Equal(typeof(IngredientPreparationKind), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(IngredientPreparationKind), parameter.ParameterType));
        Assert.Empty(stepType.GetEvents());
        Assert.True(stepType.IsSealed);
    }
}
