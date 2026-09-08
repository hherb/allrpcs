using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnRequestTests
{
    [Fact]
    public void RetainsExactResponderEventAndDialogueActInstances()
    {
        CharacterId responderCharacterId = new("healer");
        DialogueUtterance utterance = CreateUtterance("Examine the flowers first.");
        DialogueEvent dialogueEvent = CreateEvent(utterance, responderCharacterId);
        DialogueAct dialogueAct = new(utterance, DialogueActType.Command);

        CharacterTurnRequest request = new(
            responderCharacterId,
            dialogueEvent,
            dialogueAct);

        Assert.Same(responderCharacterId, request.RespondingCharacterId);
        Assert.Same(dialogueEvent, request.DialogueEvent);
        Assert.Same(dialogueAct, request.DialogueAct);
    }

    [Fact]
    public void AcceptsDistinctValueEqualResponderAndUtteranceValues()
    {
        DialogueUtterance eventUtterance = CreateUtterance("Is moonwort safe?");
        DialogueUtterance actUtterance = CreateUtterance("Is moonwort safe?");
        DialogueEvent dialogueEvent = CreateEvent(
            eventUtterance,
            new CharacterId("healer"));
        DialogueAct dialogueAct = new(actUtterance, DialogueActType.Question);

        CharacterTurnRequest request = new(
            new CharacterId("healer"),
            dialogueEvent,
            dialogueAct);

        Assert.Same(dialogueEvent, request.DialogueEvent);
        Assert.Same(dialogueAct, request.DialogueAct);
    }

    [Fact]
    public void RejectsResponderWhoDidNotHearTheUtterance()
    {
        DialogueUtterance utterance = CreateUtterance("Examine the flowers first.");
        DialogueEvent dialogueEvent = CreateEvent(utterance, new CharacterId("healer"));
        DialogueAct dialogueAct = new(utterance, DialogueActType.Command);

        Assert.Throws<ArgumentException>(() => new CharacterTurnRequest(
            new CharacterId("hermit"),
            dialogueEvent,
            dialogueAct));
        Assert.Throws<ArgumentException>(() => new CharacterTurnRequest(
            new CharacterId("seeker"),
            dialogueEvent,
            dialogueAct));
    }

    [Fact]
    public void RejectsActForDifferentUtterance()
    {
        DialogueUtterance eventUtterance = CreateUtterance("Examine the flowers first.");
        DialogueEvent dialogueEvent = CreateEvent(
            eventUtterance,
            new CharacterId("healer"));
        DialogueAct differentAct = new(
            CreateUtterance("Collect the flowers now."),
            DialogueActType.Command);

        Assert.Throws<ArgumentException>(() => new CharacterTurnRequest(
            new CharacterId("healer"),
            dialogueEvent,
            differentAct));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DialogueUtterance utterance = CreateUtterance("Hello.");
        CharacterId responderCharacterId = new("healer");
        DialogueEvent dialogueEvent = CreateEvent(utterance, responderCharacterId);
        DialogueAct dialogueAct = new(utterance, DialogueActType.Request);

        Assert.Throws<ArgumentNullException>(
            () => new CharacterTurnRequest(null!, dialogueEvent, dialogueAct));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterTurnRequest(responderCharacterId, null!, dialogueAct));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterTurnRequest(responderCharacterId, dialogueEvent, null!));
    }

    [Fact]
    public void ExposesOnlyTheProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterTurnRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("DialogueAct", typeof(DialogueAct)),
            ("DialogueEvent", typeof(DialogueEvent)),
            ("RespondingCharacterId", typeof(CharacterId)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = requestType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.DoesNotContain(actualProperties, property =>
            property.Name.Contains("Provider", StringComparison.Ordinal)
            || property.Name.Contains("Model", StringComparison.Ordinal));
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static DialogueUtterance CreateUtterance(string text)
    {
        return new DialogueUtterance(new CharacterId("seeker"), text);
    }

    private static DialogueEvent CreateEvent(
        DialogueUtterance utterance,
        CharacterId listenerCharacterId)
    {
        return new DialogueEvent(
            utterance,
            new DialogueAudienceSnapshot(
                utterance.SpeakerCharacterId,
                [listenerCharacterId]));
    }
}
