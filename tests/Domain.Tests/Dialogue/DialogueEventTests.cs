using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueEventTests
{
    [Fact]
    public void RetainsExactUtteranceAndAudienceInstances()
    {
        CharacterId speakerCharacterId = new("seeker");
        DialogueUtterance utterance = new(speakerCharacterId, "Examine the flowers first.");
        DialogueAudienceSnapshot audience = new(
            speakerCharacterId,
            [new CharacterId("healer")]);

        DialogueEvent dialogueEvent = new(utterance, audience);

        Assert.Same(utterance, dialogueEvent.Utterance);
        Assert.Same(audience, dialogueEvent.Audience);
    }

    [Fact]
    public void AcceptsDistinctValueEqualSpeakerIdentifiers()
    {
        DialogueUtterance utterance = new(
            new CharacterId("seeker"),
            "Examine the flowers first.");
        DialogueAudienceSnapshot audience = new(
            new CharacterId("seeker"),
            [new CharacterId("healer")]);

        DialogueEvent dialogueEvent = new(utterance, audience);

        Assert.Same(utterance, dialogueEvent.Utterance);
        Assert.Same(audience, dialogueEvent.Audience);
    }

    [Fact]
    public void RejectsMismatchedSpeakerIdentifiers()
    {
        DialogueUtterance utterance = new(
            new CharacterId("seeker"),
            "Examine the flowers first.");
        DialogueAudienceSnapshot audience = new(
            new CharacterId("healer"),
            [new CharacterId("seeker")]);

        Assert.Throws<ArgumentException>(() => new DialogueEvent(utterance, audience));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "Hello.");
        DialogueAudienceSnapshot audience = new(
            new CharacterId("seeker"),
            Array.Empty<CharacterId>());

        Assert.Throws<ArgumentNullException>(() => new DialogueEvent(null!, audience));
        Assert.Throws<ArgumentNullException>(() => new DialogueEvent(utterance, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDialogueEventSurface()
    {
        Type eventType = typeof(DialogueEvent);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Audience", typeof(DialogueAudienceSnapshot)),
            ("Utterance", typeof(DialogueUtterance)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = eventType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(eventType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(DialogueUtterance), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DialogueAudienceSnapshot), parameter.ParameterType));
        Assert.Empty(eventType.GetEvents());
        Assert.True(eventType.IsSealed);
    }
}
