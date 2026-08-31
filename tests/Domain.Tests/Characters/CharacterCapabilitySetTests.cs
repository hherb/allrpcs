using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterCapabilitySetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        CharacterCapabilitySet capabilities = new(Array.Empty<CharacterCapability>());

        Assert.Empty(capabilities.Entries);
    }

    [Fact]
    public void PreservesAuthoredEntryOrder()
    {
        CharacterCapability first = new("herbalism", "Recognizes common plants.");
        CharacterCapability second = new("field-medicine", "Can assess wounds.");

        CharacterCapabilitySet capabilities = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, capabilities.Entries);
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        CharacterCapability original = new("herbalism", "Recognizes common plants.");
        CharacterCapability replacement = new("field-medicine", "Can assess wounds.");
        CharacterCapability[] source = [original];

        CharacterCapabilitySet capabilities = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, capabilities.Entries);
    }

    [Fact]
    public void RejectsDuplicateKeysEvenWhenDescriptionsDiffer()
    {
        CharacterCapability first = new("herbalism", "Recognizes common plants.");
        CharacterCapability duplicate = new("herbalism", "Recognizes rare plants.");

        Assert.Throws<ArgumentException>(
            () => new CharacterCapabilitySet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterCapabilitySet(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterCapabilitySet(new CharacterCapability[] { null! }));
    }
}
