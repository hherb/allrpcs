using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientStateSnapshotTests
{
    [Fact]
    public void CopiesStatesInStableOrderAndRetainsExactInstances()
    {
        IngredientState first = State("moonwort-leaf");
        IngredientState second = State("marsh-moss");
        IngredientState replacement = State("replacement");
        IngredientState[] supplied = [first, second];

        IngredientStateSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.States,
            state => Assert.Same(first, state),
            state => Assert.Same(second, state));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<IngredientState>)snapshot.States).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndTheMaximumDistinctStateCount()
    {
        IngredientState[] maximum = Enumerable
            .Range(0, IngredientStateSnapshot.MaximumStateCount)
            .Select(index => State($"ingredient-{index}"))
            .ToArray();

        Assert.Empty(new IngredientStateSnapshot([]).States);
        Assert.Equal(
            IngredientStateSnapshot.MaximumStateCount,
            new IngredientStateSnapshot(maximum).States.Count);
    }

    [Fact]
    public void RejectsNullsExcessiveStatesAndDuplicateIngredientIdentities()
    {
        IngredientState[] excessive = Enumerable
            .Range(0, IngredientStateSnapshot.MaximumStateCount + 1)
            .Select(index => State($"ingredient-{index}"))
            .ToArray();

        Assert.Throws<ArgumentNullException>(() => new IngredientStateSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new IngredientStateSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new IngredientStateSnapshot(
            [State("moonwort-leaf"), State("moonwort-leaf")]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IngredientStateSnapshot(excessive));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(IngredientStateSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("States", property.Name);
        Assert.Equal(typeof(IReadOnlyList<IngredientState>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(IEnumerable<IngredientState>), parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static IngredientState State(string ingredientId, int amount = 1) =>
        new(
            new IngredientDefinition(new IngredientId(ingredientId)),
            new IngredientQuantity(amount, IngredientQuantityUnit.Count),
            IngredientFreshness.Fresh,
            IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated);
}
