using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterConditionSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        CharacterConditionSet conditions = new(Array.Empty<CharacterCondition>());

        Assert.Empty(conditions.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrder()
    {
        CharacterCondition first = new("travel-weary", "Appears tired after the journey.");
        CharacterCondition second = new("well-rested", "Appears ready to travel.");

        CharacterConditionSet conditions = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, conditions.Entries);
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        CharacterCondition original = new("travel-weary", "Appears tired after the journey.");
        CharacterCondition replacement = new("well-rested", "Appears ready to travel.");
        CharacterCondition[] source = [original];

        CharacterConditionSet conditions = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, conditions.Entries);
    }

    [Fact]
    public void RejectsDuplicateKeysEvenWhenDescriptionsDiffer()
    {
        CharacterCondition first = new("travel-weary", "Appears tired after the journey.");
        CharacterCondition duplicate = new("travel-weary", "Moves cautiously.");

        Assert.Throws<ArgumentException>(
            () => new CharacterConditionSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterConditionSet(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterConditionSet(new CharacterCondition[] { null! }));
    }

    [Fact]
    public void ExposesNoDiseaseOrRuntimeMechanics()
    {
        string[] publicProperties = typeof(CharacterConditionSet)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(publicProperties, property => Assert.Equal("Entries", property));
    }
}
