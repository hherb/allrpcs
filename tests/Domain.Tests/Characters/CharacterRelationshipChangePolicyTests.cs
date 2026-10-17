using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipChangePolicyTests
{
    [Fact]
    public void AcceptsValueEqualOwnershipAndOneBoundedChange()
    {
        CharacterRelationshipMemory relationship = Relationship(25);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change(
            delta: -10,
            characterId: new CharacterId("healer"),
            relatedCharacterId: new CharacterId("seeker"));

        Assert.True(CharacterRelationshipChangePolicy.CanApply(relationship, change));
        Assert.Equal(25, relationship.Dimensions[0].Value);
        Assert.Empty(relationship.Reasons);
    }

    [Fact]
    public void RejectsMismatchedOwnershipAndMissingDimension()
    {
        CharacterRelationshipMemory relationship = Relationship(0);
        CharacterRelationshipChange wrongOwner = CharacterRelationshipChangeTests.Change(
            characterId: new CharacterId("hermit"));
        CharacterRelationshipChange wrongRelated = CharacterRelationshipChangeTests.Change(
            relatedCharacterId: new CharacterId("sage"));
        CharacterRelationshipChange missingDimension = CharacterRelationshipChangeTests.Change(
            reason: CharacterRelationshipChangeTests.Reason(
                dimension: CharacterRelationshipDimension.Respect));

        Assert.False(CharacterRelationshipChangePolicy.CanApply(relationship, wrongOwner));
        Assert.False(CharacterRelationshipChangePolicy.CanApply(relationship, wrongRelated));
        Assert.False(CharacterRelationshipChangePolicy.CanApply(relationship, missingDimension));
    }

    [Theory]
    [InlineData(100, 1)]
    [InlineData(-100, -1)]
    public void RejectsDimensionOverflowInsteadOfClamping(int currentValue, int delta)
    {
        CharacterRelationshipMemory relationship = Relationship(currentValue);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change(delta);

        Assert.False(CharacterRelationshipChangePolicy.CanApply(relationship, change));
        Assert.Equal(currentValue, relationship.Dimensions[0].Value);
    }

    [Fact]
    public void RejectsASecondChangeFromTheSameExchange()
    {
        CharacterRelationshipReason priorReason = CharacterRelationshipChangeTests.Reason(
            "prior",
            "turn-one");
        CharacterRelationshipMemory relationship = Relationship(0, [priorReason]);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change(
            reason: CharacterRelationshipChangeTests.Reason("replacement", "turn-one"));

        Assert.False(CharacterRelationshipChangePolicy.CanApply(relationship, change));
        Assert.Same(priorReason, Assert.Single(relationship.Reasons));
    }

    [Fact]
    public void RejectsAReusedReasonIdentityFromAnotherExchange()
    {
        CharacterRelationshipReason priorReason = CharacterRelationshipChangeTests.Reason(
            "same-reason",
            "prior-turn");
        CharacterRelationshipMemory relationship = Relationship(0, [priorReason]);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change(
            reason: CharacterRelationshipChangeTests.Reason("same-reason", "new-turn"));

        Assert.False(CharacterRelationshipChangePolicy.CanApply(relationship, change));
    }

    [Fact]
    public void AcceptsByEvictingOldestReasonWhenCapacityIsFull()
    {
        CharacterRelationshipReason[] reasons = Enumerable
            .Range(0, CharacterRelationshipMemory.MaximumReasonCount)
            .Select(index => CharacterRelationshipChangeTests.Reason(
                $"reason-{index}",
                $"turn-{index}"))
            .ToArray();
        CharacterRelationshipMemory relationship = Relationship(0, reasons);

        Assert.True(CharacterRelationshipChangePolicy.CanApply(
            relationship,
            CharacterRelationshipChangeTests.Change(
                reason: CharacterRelationshipChangeTests.Reason("extra", "extra-turn"))));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyPureEligibility()
    {
        CharacterRelationshipMemory relationship = Relationship(0);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterRelationshipChangePolicy.CanApply(null!, change));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterRelationshipChangePolicy.CanApply(relationship, null!));

        Type policyType = typeof(CharacterRelationshipChangePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        Assert.Equal("CanApply", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterRelationshipMemory Relationship(
        int trust,
        IEnumerable<CharacterRelationshipReason>? reasons = null) => new(
            new CharacterId("healer"),
            new CharacterId("seeker"),
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, trust)],
            reasons ?? []);
}
