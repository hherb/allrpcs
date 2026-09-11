using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipChangeTests
{
    [Theory]
    [InlineData(-10)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(10)]
    public void RetainsExactInputsAtAcceptedBounds(int delta)
    {
        CharacterId characterId = new("healer");
        CharacterId relatedCharacterId = new("seeker");
        CharacterRelationshipReason reason = Reason();

        CharacterRelationshipChange change = new(
            characterId,
            relatedCharacterId,
            delta,
            reason);

        Assert.Same(characterId, change.CharacterId);
        Assert.Same(relatedCharacterId, change.RelatedCharacterId);
        Assert.Equal(delta, change.Delta);
        Assert.Same(reason, change.Reason);
    }

    [Theory]
    [InlineData(-11)]
    [InlineData(0)]
    [InlineData(11)]
    public void RejectsZeroAndExcessiveChanges(int delta)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRelationshipChange(
            new CharacterId("healer"),
            new CharacterId("seeker"),
            delta,
            Reason()));
    }

    [Fact]
    public void RejectsSelfRelationshipAndNulls()
    {
        CharacterId characterId = new("healer");
        CharacterId relatedCharacterId = new("seeker");
        CharacterRelationshipReason reason = Reason();

        Assert.Throws<ArgumentException>(() => new CharacterRelationshipChange(
            characterId,
            new CharacterId("healer"),
            1,
            reason));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipChange(
            null!, relatedCharacterId, 1, reason));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipChange(
            characterId, null!, 1, reason));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipChange(
            characterId, relatedCharacterId, 1, null!));
    }

    [Fact]
    public void ExposesOnlyImmutableBoundedChangeSurface()
    {
        Type changeType = typeof(CharacterRelationshipChange);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CharacterId", typeof(CharacterId)),
            ("Delta", typeof(int)),
            ("Reason", typeof(CharacterRelationshipReason)),
            ("RelatedCharacterId", typeof(CharacterId)),
        ];
        System.Reflection.PropertyInfo[] properties = changeType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(10, CharacterRelationshipChange.MaximumAbsoluteDeltaPerExchange);
        Assert.True(changeType.IsSealed);
    }

    internal static CharacterRelationshipChange Change(
        int delta = 1,
        CharacterId? characterId = null,
        CharacterId? relatedCharacterId = null,
        CharacterRelationshipReason? reason = null) => new(
            characterId ?? new CharacterId("healer"),
            relatedCharacterId ?? new CharacterId("seeker"),
            delta,
            reason ?? Reason());

    internal static CharacterRelationshipReason Reason(
        string reasonId = "kept-promise",
        string sourceTurnId = "turn-one",
        CharacterRelationshipDimension? dimension = null) => new(
            new RelationshipReasonId(reasonId),
            dimension ?? CharacterRelationshipDimension.Trust,
            new CharacterTurnId(sourceTurnId),
            "The promise was kept.");
}
