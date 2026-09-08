using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterIntentionTests
{
    [Fact]
    public void RetainsExactIntentionTypeAndTargetIdentifierInstances()
    {
        CharacterIntentionType type = CharacterIntentionType.ExamineObject;
        ObjectId targetObjectId = new("moonwort-patch");

        CharacterIntention intention = new(type, targetObjectId);

        Assert.Same(type, intention.Type);
        Assert.Same(targetObjectId, intention.TargetObjectId);
    }

    [Fact]
    public void RetainsMissingTargetForDeterministicValidation()
    {
        CharacterIntention intention = new(CharacterIntentionType.CollectObject, null);

        Assert.Same(CharacterIntentionType.CollectObject, intention.Type);
        Assert.Null(intention.TargetObjectId);
    }

    [Fact]
    public void RetainsUnexpectedTargetForDeterministicValidation()
    {
        ObjectId targetObjectId = new("moonwort-patch");

        CharacterIntention intention = new(CharacterIntentionType.Wait, targetObjectId);

        Assert.Same(CharacterIntentionType.Wait, intention.Type);
        Assert.Same(targetObjectId, intention.TargetObjectId);
    }

    [Fact]
    public void EqualityUsesTypeAndTargetIdentifierValues()
    {
        CharacterIntention first = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("moonwort-patch"));
        CharacterIntention same = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("moonwort-patch"));
        CharacterIntention differentType = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("moonwort-patch"));
        CharacterIntention differentTarget = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("yarrow-patch"));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentType);
        Assert.NotEqual(first, differentTarget);
    }

    [Fact]
    public void RejectsNullIntentionType()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterIntention(null!, null));
    }

    [Fact]
    public void ExposesOnlyTheImmutableIntentionPayload()
    {
        Type intentionType = typeof(CharacterIntention);
        (string Name, Type Type)[] expectedProperties =
        [
            ("TargetObjectId", typeof(ObjectId)),
            ("Type", typeof(CharacterIntentionType)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = intentionType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(intentionType.GetEvents());
        Assert.True(intentionType.IsSealed);
    }
}
