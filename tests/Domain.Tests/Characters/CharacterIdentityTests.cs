using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIdentityTests
{
    [Fact]
    public void RetainsStableIdentifierAndAuthoredDisplayName()
    {
        CharacterId id = new("travelling-healer");
        const string DisplayName = "Dr  Séraphine";

        CharacterIdentity identity = new(id, DisplayName);

        Assert.Same(id, identity.Id);
        Assert.Equal(DisplayName, identity.DisplayName);
    }

    [Fact]
    public void EqualityUsesIdentifierAndDisplayNameValues()
    {
        CharacterIdentity first = new(new CharacterId("healer"), "Séraphine");
        CharacterIdentity same = new(new CharacterId("healer"), "Séraphine");
        CharacterIdentity differentId = new(new CharacterId("sage"), "Séraphine");
        CharacterIdentity differentName = new(new CharacterId("healer"), "The Healer");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentId);
        Assert.NotEqual(first, differentName);
    }

    [Fact]
    public void AcceptsTheMaximumDisplayNameLength()
    {
        string displayName = new('n', CharacterIdentity.MaximumDisplayNameLength);

        CharacterIdentity identity = new(new CharacterId("healer"), displayName);

        Assert.Equal(displayName, identity.DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyDisplayNames(string displayName)
    {
        Assert.Throws<ArgumentException>(
            () => new CharacterIdentity(new CharacterId("healer"), displayName));
    }

    [Fact]
    public void RejectsAnOversizedDisplayName()
    {
        string displayName = new('n', CharacterIdentity.MaximumDisplayNameLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterIdentity(new CharacterId("healer"), displayName));
    }

    [Fact]
    public void RejectsNullIdentityValues()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterIdentity(null!, "Healer"));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterIdentity(new CharacterId("healer"), null!));
    }
}
