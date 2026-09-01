using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        CharacterBeliefSet beliefs = new(Array.Empty<CharacterBelief>());

        Assert.Empty(beliefs.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrder()
    {
        CharacterBelief first = new("moonwort-is-cooling", "Moonwort cools a fever.");
        CharacterBelief second = new("sunroot-is-warming", "Sunroot warms the body.");

        CharacterBeliefSet beliefs = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, beliefs.Entries);
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        CharacterBelief original = new("moonwort-is-cooling", "Moonwort cools a fever.");
        CharacterBelief replacement = new("sunroot-is-warming", "Sunroot warms the body.");
        CharacterBelief[] source = [original];

        CharacterBeliefSet beliefs = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, beliefs.Entries);
    }

    [Fact]
    public void RejectsDuplicateKeysEvenWhenStatementsDiffer()
    {
        CharacterBelief first = new("moonwort-is-cooling", "Moonwort cools a fever.");
        CharacterBelief duplicate = new("moonwort-is-cooling", "Moonwort cools any illness.");

        Assert.Throws<ArgumentException>(
            () => new CharacterBeliefSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefSet(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterBeliefSet(new CharacterBelief[] { null! }));
    }

    [Fact]
    public void ExposesReadOnlyEntriesWithoutBeliefMechanics()
    {
        CharacterBelief belief = new("moonwort-is-cooling", "Moonwort cools a fever.");
        CharacterBeliefSet beliefs = new(new[] { belief });

        Assert.IsAssignableFrom<IReadOnlyList<CharacterBelief>>(beliefs.Entries);
        Assert.Throws<NotSupportedException>(
            () => ((IList<CharacterBelief>)beliefs.Entries).Add(belief));

        string[] publicProperties = typeof(CharacterBeliefSet)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(publicProperties, property => Assert.Equal("Entries", property));
    }
}
