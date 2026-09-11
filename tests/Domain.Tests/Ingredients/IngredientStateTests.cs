using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientStateTests
{
    [Fact]
    public void RetainsEveryExactIngredientValue()
    {
        IngredientDefinition definition = new(new IngredientId("moonwort-leaf"));
        IngredientQuantity quantity = new(250, IngredientQuantityUnit.Milligram);
        IngredientFreshness freshness = IngredientFreshness.Fresh;
        IngredientPhysicalCondition condition = IngredientPhysicalCondition.Intact;
        IngredientContamination contamination = IngredientContamination.Uncontaminated;

        IngredientState state = new(definition, quantity, freshness, condition, contamination);

        Assert.Same(definition, state.Definition);
        Assert.Same(quantity, state.Quantity);
        Assert.Same(freshness, state.Freshness);
        Assert.Same(condition, state.PhysicalCondition);
        Assert.Same(contamination, state.Contamination);
    }

    [Fact]
    public void RejectsEveryNullValue()
    {
        IngredientDefinition definition = new(new IngredientId("moonwort-leaf"));
        IngredientQuantity quantity = new(1, IngredientQuantityUnit.Count);

        Assert.Throws<ArgumentNullException>(() => new IngredientState(
            null!, quantity, IngredientFreshness.Fresh, IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated));
        Assert.Throws<ArgumentNullException>(() => new IngredientState(
            definition, null!, IngredientFreshness.Fresh, IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated));
        Assert.Throws<ArgumentNullException>(() => new IngredientState(
            definition, quantity, null!, IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated));
        Assert.Throws<ArgumentNullException>(() => new IngredientState(
            definition, quantity, IngredientFreshness.Fresh, null!,
            IngredientContamination.Uncontaminated));
        Assert.Throws<ArgumentNullException>(() => new IngredientState(
            definition, quantity, IngredientFreshness.Fresh, IngredientPhysicalCondition.Intact,
            null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableStateSurface()
    {
        Type stateType = typeof(IngredientState);
        System.Reflection.PropertyInfo[] properties = stateType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(stateType.GetConstructors());

        Assert.Equal(
            ["Contamination", "Definition", "Freshness", "PhysicalCondition", "Quantity"],
            properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(IngredientDefinition),
                typeof(IngredientQuantity),
                typeof(IngredientFreshness),
                typeof(IngredientPhysicalCondition),
                typeof(IngredientContamination),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(stateType.GetEvents());
        Assert.True(stateType.IsSealed);
    }
}
