using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterPartySeparationTransitionTests
{
    [Fact]
    public void RemovesValueEqualMemberAndRetainsOtherExactOrder()
    {
        CharacterId healer = new("healer");
        CharacterId sage = new("sage");
        CharacterId hermit = new("hermit");
        CharacterPartySnapshot party = new([healer, sage, hermit]);

        CharacterPartySnapshot result = CharacterPartySeparationTransition.Separate(
            new CharacterId("sage"),
            party);

        Assert.Collection(
            result.MemberCharacterIds,
            member => Assert.Same(healer, member),
            member => Assert.Same(hermit, member));
        Assert.Collection(
            party.MemberCharacterIds,
            member => Assert.Same(healer, member),
            member => Assert.Same(sage, member),
            member => Assert.Same(hermit, member));
    }

    [Fact]
    public void RejectsAbsentMemberWithoutMutation()
    {
        CharacterId healer = new("healer");
        CharacterPartySnapshot party = new([healer]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterPartySeparationTransition.Separate(new CharacterId("sage"), party));
        Assert.Same(healer, Assert.Single(party.MemberCharacterIds));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureTransition()
    {
        CharacterPartySnapshot party = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartySeparationTransition.Separate(null!, party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartySeparationTransition.Separate(new CharacterId("healer"), null!));

        Type transitionType = typeof(CharacterPartySeparationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Separate", method.Name);
        Assert.Equal(typeof(CharacterPartySnapshot), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
