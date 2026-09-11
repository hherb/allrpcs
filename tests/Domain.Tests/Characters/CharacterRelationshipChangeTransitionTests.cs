using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipChangeTransitionTests
{
    [Fact]
    public void AppliesOneDimensionAndAppendsExactReasonImmutably()
    {
        CharacterRelationshipDimensionValue trust = new(CharacterRelationshipDimension.Trust, 25);
        CharacterRelationshipDimensionValue respect = new(CharacterRelationshipDimension.Respect, 40);
        CharacterRelationshipReason priorReason = CharacterRelationshipChangeTests.Reason(
            "prior",
            "prior-turn");
        CharacterRelationshipMemory relationship = new(
            new("healer"),
            new("seeker"),
            [trust, respect],
            [priorReason]);
        CharacterRelationshipReason reason = CharacterRelationshipChangeTests.Reason();
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change(
            delta: 10,
            reason: reason);

        CharacterRelationshipMemory result = CharacterRelationshipChangeTransition.Apply(
            relationship,
            change);

        Assert.NotSame(relationship, result);
        Assert.Same(relationship.CharacterId, result.CharacterId);
        Assert.Same(relationship.RelatedCharacterId, result.RelatedCharacterId);
        Assert.Collection(
            result.Dimensions,
            dimension =>
            {
                Assert.NotSame(trust, dimension);
                Assert.Same(CharacterRelationshipDimension.Trust, dimension.Dimension);
                Assert.Equal(35, dimension.Value);
            },
            dimension => Assert.Same(respect, dimension));
        Assert.Collection(
            result.Reasons,
            retained => Assert.Same(priorReason, retained),
            retained => Assert.Same(reason, retained));
        Assert.Collection(
            relationship.Dimensions,
            dimension => Assert.Same(trust, dimension),
            dimension => Assert.Same(respect, dimension));
        Assert.Same(priorReason, Assert.Single(relationship.Reasons));
        Assert.Same(reason, change.Reason);
    }

    [Fact]
    public void RejectsEveryIneligibleChangeWithoutMutatingInputs()
    {
        CharacterRelationshipMemory relationship =
            CharacterRelationshipChangePolicyTests.Relationship(100);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change(1);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterRelationshipChangeTransition.Apply(relationship, change));
        Assert.Equal(100, relationship.Dimensions[0].Value);
        Assert.Empty(relationship.Reasons);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheAtomicTransition()
    {
        CharacterRelationshipMemory relationship =
            CharacterRelationshipChangePolicyTests.Relationship(0);
        CharacterRelationshipChange change = CharacterRelationshipChangeTests.Change();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterRelationshipChangeTransition.Apply(null!, change));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterRelationshipChangeTransition.Apply(relationship, null!));

        Type transitionType = typeof(CharacterRelationshipChangeTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterRelationshipMemory), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
