using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterPartyRejoinTransitionTests
{
    [Fact]
    public void AppendsExactNonMemberWithoutChangingExistingStableOrder()
    {
        CharacterId seeker = new("seeker");
        CharacterId sage = new("sage");
        CharacterId healer = new("healer");
        CharacterPartySnapshot party = new([seeker, sage]);

        CharacterPartySnapshot result = CharacterPartyRejoinTransition.Rejoin(healer, party);

        Assert.Collection(
            result.MemberCharacterIds,
            member => Assert.Same(seeker, member),
            member => Assert.Same(sage, member),
            member => Assert.Same(healer, member));
        Assert.Collection(
            party.MemberCharacterIds,
            member => Assert.Same(seeker, member),
            member => Assert.Same(sage, member));
    }

    [Fact]
    public void RejectsExistingValueEqualMemberAndFullParty()
    {
        CharacterId healer = new("healer");
        CharacterId containedHealer = new("healer");
        CharacterPartySnapshot containing = new([containedHealer]);
        CharacterPartySnapshot full = new(Enumerable
            .Range(0, CharacterPartySnapshot.MaximumMemberCount)
            .Select(index => new CharacterId($"character-{index}")));

        Assert.False(CharacterPartyRejoinPolicy.CanRejoin(healer, containing));
        Assert.False(CharacterPartyRejoinPolicy.CanRejoin(healer, full));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterPartyRejoinTransition.Rejoin(healer, containing));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterPartyRejoinTransition.Rejoin(healer, full));
        Assert.Same(containedHealer, Assert.Single(containing.MemberCharacterIds));
        Assert.Equal(CharacterPartySnapshot.MaximumMemberCount, full.MemberCharacterIds.Count);
    }

    [Fact]
    public void ExactCapacityBoundaryIsEligible()
    {
        CharacterPartySnapshot party = new(Enumerable
            .Range(0, CharacterPartySnapshot.MaximumMemberCount - 1)
            .Select(index => new CharacterId($"character-{index}")));

        Assert.True(CharacterPartyRejoinPolicy.CanRejoin(new CharacterId("healer"), party));
        Assert.Equal(
            CharacterPartySnapshot.MaximumMemberCount,
            CharacterPartyRejoinTransition.Rejoin(
                new CharacterId("healer"),
                party).MemberCharacterIds.Count);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyTheirSingleOperations()
    {
        CharacterId healer = new("healer");
        CharacterPartySnapshot party = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartyRejoinPolicy.CanRejoin(null!, party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartyRejoinPolicy.CanRejoin(healer, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartyRejoinTransition.Rejoin(null!, party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartyRejoinTransition.Rejoin(healer, null!));

        AssertSingleStaticOperation(
            typeof(CharacterPartyRejoinPolicy),
            nameof(CharacterPartyRejoinPolicy.CanRejoin));
        AssertSingleStaticOperation(
            typeof(CharacterPartyRejoinTransition),
            nameof(CharacterPartyRejoinTransition.Rejoin));
    }

    private static void AssertSingleStaticOperation(Type type, string expectedName)
    {
        System.Reflection.MethodInfo method = Assert.Single(
            type.GetMethods().Where(candidate => candidate.DeclaringType == type));
        Assert.Equal(expectedName, method.Name);
        Assert.Empty(type.GetConstructors());
        Assert.Empty(type.GetProperties());
        Assert.Empty(type.GetEvents());
        Assert.True(type.IsAbstract && type.IsSealed);
    }
}
