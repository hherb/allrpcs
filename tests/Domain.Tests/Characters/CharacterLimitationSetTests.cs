using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterLimitationSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        CharacterLimitationSet limitations = new(Array.Empty<CharacterLimitation>());

        Assert.Empty(limitations.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrder()
    {
        CharacterLimitation first = new("poor-vision", "Cannot identify distant details.");
        CharacterLimitation second = new("deep-water", "Cannot cross deep water unaided.");

        CharacterLimitationSet limitations = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, limitations.Entries);
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        CharacterLimitation original = new("poor-vision", "Cannot identify distant details.");
        CharacterLimitation replacement = new("deep-water", "Cannot cross deep water unaided.");
        CharacterLimitation[] source = [original];

        CharacterLimitationSet limitations = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, limitations.Entries);
    }

    [Fact]
    public void RejectsDuplicateKeysEvenWhenDescriptionsDiffer()
    {
        CharacterLimitation first = new("poor-vision", "Cannot identify distant details.");
        CharacterLimitation duplicate = new("poor-vision", "Needs spectacles.");

        Assert.Throws<ArgumentException>(
            () => new CharacterLimitationSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterLimitationSet(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterLimitationSet(new CharacterLimitation[] { null! }));
    }
}
