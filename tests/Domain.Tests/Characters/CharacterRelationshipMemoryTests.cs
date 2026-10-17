using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipMemoryTests
{
    [Fact]
    public void DimensionVocabularyIsClosedCanonicalAndStable()
    {
        Assert.Collection(
            CharacterRelationshipDimension.All,
            dimension => Assert.Same(CharacterRelationshipDimension.Trust, dimension),
            dimension => Assert.Same(CharacterRelationshipDimension.Affection, dimension),
            dimension => Assert.Same(CharacterRelationshipDimension.Respect, dimension),
            dimension => Assert.Same(CharacterRelationshipDimension.Fear, dimension));
        Assert.Same(CharacterRelationshipDimension.Trust, CharacterRelationshipDimension.Parse("trust"));
        Assert.Same(
            CharacterRelationshipDimension.Affection,
            CharacterRelationshipDimension.Parse("affection"));
        Assert.Throws<ArgumentException>(() => CharacterRelationshipDimension.Parse("loyalty"));
        Assert.Throws<ArgumentNullException>(() => CharacterRelationshipDimension.Parse(null!));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterRelationshipDimension>)CharacterRelationshipDimension.All)
                .Add(CharacterRelationshipDimension.Trust));
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public void DimensionValueAcceptsExactBounds(int value)
    {
        CharacterRelationshipDimensionValue dimension = new(
            CharacterRelationshipDimension.Trust,
            value);

        Assert.Same(CharacterRelationshipDimension.Trust, dimension.Dimension);
        Assert.Equal(value, dimension.Value);
    }

    [Theory]
    [InlineData(-101)]
    [InlineData(101)]
    public void DimensionValueRejectsOutOfRangeValues(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, value));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipDimensionValue(null!, 0));
    }

    [Fact]
    public void ReasonRetainsExactInputsAndBoundsText()
    {
        RelationshipReasonId reasonId = new("kept-promise");
        CharacterTurnId sourceTurnId = new("healer-turn-2");
        string maximum = new('r', CharacterRelationshipReason.MaximumSummaryLength);

        CharacterRelationshipReason reason = new(
            reasonId,
            CharacterRelationshipDimension.Trust,
            sourceTurnId,
            maximum);

        Assert.Same(reasonId, reason.ReasonId);
        Assert.Same(CharacterRelationshipDimension.Trust, reason.Dimension);
        Assert.Same(sourceTurnId, reason.SourceTurnId);
        Assert.Equal(maximum, reason.Summary);
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipReason(
            reasonId, CharacterRelationshipDimension.Trust, sourceTurnId, " "));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipReason(
            reasonId,
            CharacterRelationshipDimension.Trust,
            sourceTurnId,
            new string('r', CharacterRelationshipReason.MaximumSummaryLength + 1)));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipReason(
            null!, CharacterRelationshipDimension.Trust, sourceTurnId, "Reason."));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipReason(
            reasonId, null!, sourceTurnId, "Reason."));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipReason(
            reasonId, CharacterRelationshipDimension.Trust, null!, "Reason."));
    }

    [Fact]
    public void RelationshipRetainsExactInputsAndCopiesOrderedCollections()
    {
        CharacterId characterId = new("healer");
        CharacterId relatedCharacterId = new("seeker");
        CharacterRelationshipDimensionValue trust = new(CharacterRelationshipDimension.Trust, 25);
        CharacterRelationshipDimensionValue respect = new(CharacterRelationshipDimension.Respect, 40);
        CharacterRelationshipReason reason = Reason("careful-listening", CharacterRelationshipDimension.Trust);
        CharacterRelationshipDimensionValue[] dimensions = [trust, respect];
        CharacterRelationshipReason[] reasons = [reason];

        CharacterRelationshipMemory relationship = new(
            characterId,
            relatedCharacterId,
            dimensions,
            reasons);
        ListIdentityContractTests.AssertIdentity(relationship);
        dimensions[0] = respect;
        reasons[0] = Reason("replacement", CharacterRelationshipDimension.Respect);

        Assert.Same(characterId, relationship.CharacterId);
        Assert.Same(relatedCharacterId, relationship.RelatedCharacterId);
        Assert.Collection(
            relationship.Dimensions,
            dimension => Assert.Same(trust, dimension),
            dimension => Assert.Same(respect, dimension));
        Assert.Collection(relationship.Reasons, retained => Assert.Same(reason, retained));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterRelationshipDimensionValue>)relationship.Dimensions).Add(trust));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterRelationshipReason>)relationship.Reasons).Add(reason));
    }

    [Fact]
    public void RelationshipRejectsInvalidShape()
    {
        CharacterId healerId = new("healer");
        CharacterId sameHealerId = new("healer");
        CharacterId seekerId = new("seeker");
        CharacterRelationshipDimensionValue trust = new(CharacterRelationshipDimension.Trust, 0);
        CharacterRelationshipDimensionValue duplicateTrust = new(CharacterRelationshipDimension.Trust, 1);
        CharacterRelationshipReason trustReason = Reason("trust-reason", CharacterRelationshipDimension.Trust);
        CharacterRelationshipReason duplicateReason = Reason("trust-reason", CharacterRelationshipDimension.Trust);
        CharacterRelationshipReason respectReason = Reason(
            "respect-reason",
            CharacterRelationshipDimension.Respect);

        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, sameHealerId, [], []));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [], []));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [trust, duplicateTrust], []));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [null!], []));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [trust], [trustReason, duplicateReason]));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [trust], [respectReason]));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [trust], [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipMemory(
            null!, seekerId, [], []));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipMemory(
            healerId, null!, [], []));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipMemory(
            healerId, seekerId, [], null!));
    }

    [Fact]
    public void RelationshipAcceptsExactReasonBoundAndRejectsExcess()
    {
        CharacterRelationshipDimensionValue trust = new(CharacterRelationshipDimension.Trust, 0);
        CharacterRelationshipReason[] maximum = Enumerable
            .Range(0, CharacterRelationshipMemory.MaximumReasonCount)
            .Select(index => Reason($"reason-{index}", CharacterRelationshipDimension.Trust))
            .ToArray();

        Assert.Equal(
            CharacterRelationshipMemory.MaximumReasonCount,
            new CharacterRelationshipMemory(
                new CharacterId("healer"),
                new CharacterId("seeker"),
                [trust],
                maximum).Reasons.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRelationshipMemory(
            new CharacterId("healer"),
            new CharacterId("seeker"),
            [trust],
            maximum.Append(Reason("extra", CharacterRelationshipDimension.Trust))));
    }

    internal static CharacterRelationshipMemory Relationship(
        CharacterId characterId,
        string relatedCharacterId) =>
        new(
            characterId,
            new CharacterId(relatedCharacterId),
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 0)],
            []);

    private static CharacterRelationshipReason Reason(
        string reasonId,
        CharacterRelationshipDimension dimension) =>
        new(
            new RelationshipReasonId(reasonId),
            dimension,
            new CharacterTurnId("healer-turn-1"),
            "A bounded local reason.");
}
