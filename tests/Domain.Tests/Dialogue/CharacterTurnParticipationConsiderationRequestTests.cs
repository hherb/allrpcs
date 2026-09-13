using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationConsiderationRequestTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RetainsExactScheduledCharacterAndHeardEvent(bool isDirectlyAddressed)
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);

        CharacterTurnParticipationConsiderationRequest request = new(
            character,
            heardEvent,
            isDirectlyAddressed);

        Assert.Same(character, request.ScheduledCharacter);
        Assert.Same(heardEvent, request.HeardDialogueEvent);
        Assert.Equal(isDirectlyAddressed, request.IsDirectlyAddressed);
    }

    [Fact]
    public void AcceptsADistinctValueEqualAudienceIdentifier()
    {
        Character character = CreateCharacter("woodland-hermit");
        CharacterId audienceIdentifier = new(character.Identity.Id.Value);
        DialogueEvent heardEvent = CreateEvent(audienceIdentifier);

        CharacterTurnParticipationConsiderationRequest request = new(
            character,
            heardEvent,
            false);

        Assert.Same(character, request.ScheduledCharacter);
        Assert.Same(heardEvent, request.HeardDialogueEvent);
        Assert.NotSame(character.Identity.Id, audienceIdentifier);
        Assert.Equal(character.Identity.Id, audienceIdentifier);
    }

    [Fact]
    public void RejectsACharacterAbsentFromTheEventAudience()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(new CharacterId("woodland-hermit"));

        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRequest(
                character,
                heardEvent,
                false));
        Assert.Same(character.Identity.Id, character.ControllerAssignment.CharacterId);
        Assert.Equal("woodland-hermit", heardEvent.Audience.ListenerCharacterIds[0].Value);
    }

    [Fact]
    public void RejectsNullRequiredValues()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnParticipationConsiderationRequest(null!, heardEvent, true));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnParticipationConsiderationRequest(character, null!, true));
    }

    [Fact]
    public void LeavesTheExactInputsAndNestedInstancesUnchanged()
    {
        Character character = CreateCharacter("village-sage");
        CharacterIdentity identity = character.Identity;
        CharacterId listenerIdentifier = new(character.Identity.Id.Value);
        DialogueEvent heardEvent = CreateEvent(listenerIdentifier);
        DialogueUtterance utterance = heardEvent.Utterance;
        DialogueAudienceSnapshot audience = heardEvent.Audience;

        _ = new CharacterTurnParticipationConsiderationRequest(character, heardEvent, true);

        Assert.Same(identity, character.Identity);
        Assert.Same(listenerIdentifier, Assert.Single(audience.ListenerCharacterIds));
        Assert.Same(utterance, heardEvent.Utterance);
        Assert.Same(audience, heardEvent.Audience);
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralRequestSurface()
    {
        Type requestType = typeof(CharacterTurnParticipationConsiderationRequest);
        System.Reflection.ConstructorInfo constructor = Assert.Single(requestType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = requestType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [typeof(Character), typeof(DialogueEvent), typeof(bool)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("HeardDialogueEvent", typeof(DialogueEvent)),
                ("IsDirectlyAddressed", typeof(bool)),
                ("ScheduledCharacter", typeof(Character)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.DoesNotContain(properties, property =>
            property.Name.Contains("Provider", StringComparison.Ordinal)
            || property.Name.Contains("Model", StringComparison.Ordinal));
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static DialogueEvent CreateEvent(CharacterId listenerCharacterId)
    {
        CharacterId speakerCharacterId = new("seeker");

        return new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerCharacterId, [listenerCharacterId]));
    }

    private static Character CreateCharacter(string id)
    {
        CharacterId characterId = new(id);

        return new Character(
            new CharacterIdentity(characterId, id.ToUpperInvariant()),
            new PublicCharacterBackground("A scheduled participant."),
            new PrivateCharacterBackground("A private scheduled participant."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [new CharacterGoal($"goal-{id}", "Participate for an authored reason.")]));
    }
}
