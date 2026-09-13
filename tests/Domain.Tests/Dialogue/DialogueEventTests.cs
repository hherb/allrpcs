using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueEventTests
{
    [Fact]
    public void RetainsExactDeliveryUtteranceAndAudienceInstances()
    {
        CharacterId speakerCharacterId = new("seeker");
        DialogueUtterance utterance = new(speakerCharacterId, "Examine the flowers first.");
        DialogueDelivery delivery = new(utterance, DialogueDeliveryMode.Whispering);
        DialogueAudienceSnapshot audience = new(
            speakerCharacterId,
            [new CharacterId("healer")]);

        DialogueEvent dialogueEvent = new(delivery, audience);

        Assert.Same(delivery, dialogueEvent.Delivery);
        Assert.Same(utterance, dialogueEvent.Utterance);
        Assert.Same(audience, dialogueEvent.Audience);
    }

    [Fact]
    public void AcceptsDistinctValueEqualSpeakerIdentifiers()
    {
        DialogueUtterance utterance = new(
            new CharacterId("seeker"),
            "Examine the flowers first.");
        DialogueDelivery delivery = new(utterance, DialogueDeliveryMode.OrdinarySpeech);
        DialogueAudienceSnapshot audience = new(
            new CharacterId("seeker"),
            [new CharacterId("healer")]);

        DialogueEvent dialogueEvent = new(delivery, audience);

        Assert.Same(delivery, dialogueEvent.Delivery);
        Assert.Same(utterance, dialogueEvent.Utterance);
        Assert.Same(audience, dialogueEvent.Audience);
    }

    [Fact]
    public void RejectsMismatchedSpeakerIdentifiers()
    {
        DialogueUtterance utterance = new(
            new CharacterId("seeker"),
            "Examine the flowers first.");
        DialogueDelivery delivery = new(utterance, DialogueDeliveryMode.Whispering);
        DialogueAudienceSnapshot audience = new(
            new CharacterId("healer"),
            [new CharacterId("seeker")]);

        Assert.Throws<ArgumentException>(() => new DialogueEvent(delivery, audience));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "Hello.");
        DialogueDelivery delivery = new(utterance, DialogueDeliveryMode.OrdinarySpeech);
        DialogueAudienceSnapshot audience = new(
            new CharacterId("seeker"),
            Array.Empty<CharacterId>());

        Assert.Throws<ArgumentNullException>(() => new DialogueEvent(null!, audience));
        Assert.Throws<ArgumentNullException>(() => new DialogueEvent(delivery, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDialogueEventSurface()
    {
        Type eventType = typeof(DialogueEvent);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Audience", typeof(DialogueAudienceSnapshot)),
            ("Delivery", typeof(DialogueDelivery)),
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
            parameter => Assert.Equal(typeof(DialogueDelivery), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DialogueAudienceSnapshot), parameter.ParameterType));
        Assert.Empty(eventType.GetEvents());
        Assert.True(eventType.IsSealed);
    }
}
