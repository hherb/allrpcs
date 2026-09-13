using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterPartySnapshotTests
{
    [Fact]
    public void PreservesStableExactMembersAndCopiesCollection()
    {
        CharacterId healer = new("healer");
        CharacterId sage = new("sage");
        CharacterId[] source = [healer, sage];

        CharacterPartySnapshot party = new(source);
        source[0] = sage;

        Assert.Collection(
            party.MemberCharacterIds,
            member => Assert.Same(healer, member),
            member => Assert.Same(sage, member));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterId>)party.MemberCharacterIds).Add(healer));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterId[] maximum = Enumerable
            .Range(0, CharacterPartySnapshot.MaximumMemberCount)
            .Select(index => new CharacterId($"character-{index}"))
            .ToArray();

        Assert.Empty(new CharacterPartySnapshot([]).MemberCharacterIds);
        Assert.Equal(
            CharacterPartySnapshot.MaximumMemberCount,
            new CharacterPartySnapshot(maximum).MemberCharacterIds.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterPartySnapshot(maximum.Append(new CharacterId("extra-character"))));
    }

    [Fact]
    public void RejectsValueEqualDuplicatesNullEntriesAndNullCollection()
    {
        Assert.Throws<ArgumentException>(() => new CharacterPartySnapshot(
            [new CharacterId("healer"), new CharacterId("healer")]));
        Assert.Throws<ArgumentException>(() => new CharacterPartySnapshot([null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterPartySnapshot(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutablePartySurface()
    {
        Type partyType = typeof(CharacterPartySnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(partyType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(partyType.GetConstructors());

        Assert.Equal("MemberCharacterIds", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterId>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(IEnumerable<CharacterId>), parameter.ParameterType));
        Assert.Empty(partyType.GetEvents());
        Assert.Empty(partyType.GetMethods().Where(method =>
            method.DeclaringType == partyType && !method.IsSpecialName));
        Assert.True(partyType.IsSealed);
    }
}
