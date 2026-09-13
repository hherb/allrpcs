using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterPartySeparationPolicyTests
{
    [Fact]
    public void AllowsValueEqualExistingMemberWithoutMutation()
    {
        CharacterId retained = new("healer");
        CharacterPartySnapshot party = new([retained]);

        Assert.True(CharacterPartySeparationPolicy.CanSeparate(
            new CharacterId("healer"),
            party));
        Assert.Same(retained, Assert.Single(party.MemberCharacterIds));
    }

    [Fact]
    public void RejectsAbsentMember()
    {
        Assert.False(CharacterPartySeparationPolicy.CanSeparate(
            new CharacterId("sage"),
            new CharacterPartySnapshot([new CharacterId("healer")])));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureDecision()
    {
        CharacterPartySnapshot party = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartySeparationPolicy.CanSeparate(null!, party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPartySeparationPolicy.CanSeparate(new CharacterId("healer"), null!));

        Type policyType = typeof(CharacterPartySeparationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanSeparate", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
