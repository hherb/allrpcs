using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterEpistemicMemoryCategoryTests
{
    [Fact]
    public void ExposesCanonicalClosedCategoriesInStableOrder()
    {
        Assert.Collection(
            CharacterEpistemicMemoryCategory.All,
            category => Assert.Same(CharacterEpistemicMemoryCategory.ConfirmedFact, category),
            category => Assert.Same(CharacterEpistemicMemoryCategory.AttributedClaim, category),
            category => Assert.Same(CharacterEpistemicMemoryCategory.UncertainBelief, category));

        Assert.Equal("confirmed-fact", CharacterEpistemicMemoryCategory.ConfirmedFact.Value);
        Assert.Equal("attributed-claim", CharacterEpistemicMemoryCategory.AttributedClaim.Value);
        Assert.Equal("uncertain-belief", CharacterEpistemicMemoryCategory.UncertainBelief.Value);
    }

    [Theory]
    [InlineData("confirmed-fact")]
    [InlineData("attributed-claim")]
    [InlineData("uncertain-belief")]
    public void ParsesOnlyCanonicalCategories(string value)
    {
        CharacterEpistemicMemoryCategory category = CharacterEpistemicMemoryCategory.Parse(value);

        Assert.Equal(value, category.Value);
        Assert.Same(category, CharacterEpistemicMemoryCategory.Parse(value));
    }

    [Fact]
    public void RejectsUnknownAndNullCategories()
    {
        Assert.Throws<ArgumentException>(() => CharacterEpistemicMemoryCategory.Parse("fact"));
        Assert.Throws<ArgumentNullException>(() => CharacterEpistemicMemoryCategory.Parse(null!));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterEpistemicMemoryCategory>)CharacterEpistemicMemoryCategory.All)
                .Add(CharacterEpistemicMemoryCategory.ConfirmedFact));
    }
}
