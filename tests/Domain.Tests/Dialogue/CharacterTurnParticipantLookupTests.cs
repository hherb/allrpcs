using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipantLookupTests
{
    [Fact]
    public void FindsTheExactValueMatchingLlmParticipantForAnAwaitingTurn()
    {
        Character healer = CreateCharacter("travelling-healer");
        Character sage = CreateCharacter("village-sage");
        CharacterTurnActivity activity = CreateActivity(
            new CharacterId(sage.Identity.Id.Value),
            CharacterTurnActivityState.AwaitingResponse);

        Character? selected = CharacterTurnParticipantLookup.Find([healer, sage], activity);

        Assert.Same(sage, selected);
        Assert.NotSame(sage.Identity.Id, activity.Request.RespondingCharacterId);
        Assert.Equal(sage.Identity.Id, activity.Request.RespondingCharacterId);
    }

    [Fact]
    public void ReturnsNullForAbsentOrNonAwaitingTurnsAfterCompletePreflight()
    {
        Character healer = CreateCharacter("travelling-healer");
        Character sage = CreateCharacter("village-sage");

        Assert.Null(CharacterTurnParticipantLookup.Find(
            [healer, sage],
            CreateActivity(new CharacterId("absent"), CharacterTurnActivityState.AwaitingResponse)));
        Assert.Null(CharacterTurnParticipantLookup.Find(
            [healer, sage],
            CreateActivity(healer.Identity.Id, CharacterTurnActivityState.Cancelled)));
        Assert.Null(CharacterTurnParticipantLookup.Find(
            [healer, sage],
            CreateActivity(healer.Identity.Id, CharacterTurnActivityState.Resolved)));
    }

    [Fact]
    public void RejectsNullDuplicateOversizedAndIneligibleParticipants()
    {
        Character participant = CreateCharacter("participant");
        Character[] maximum = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => CreateCharacter($"participant-{index}"))
            .ToArray();
        CharacterTurnActivity activity = CreateActivity(
            participant.Identity.Id,
            CharacterTurnActivityState.AwaitingResponse);

        Assert.Throws<ArgumentNullException>(() => CharacterTurnParticipantLookup.Find(null!, activity));
        Assert.Throws<ArgumentNullException>(() => CharacterTurnParticipantLookup.Find([], null!));
        Assert.Throws<ArgumentException>(() => CharacterTurnParticipantLookup.Find([null!], activity));
        Assert.Throws<ArgumentException>(() => CharacterTurnParticipantLookup.Find(
            [participant, CreateCharacter(participant.Identity.Id.Value)],
            activity));
        Assert.Throws<ArgumentException>(() => CharacterTurnParticipantLookup.Find(
            [AsHuman(participant)],
            activity));
        Assert.Throws<ArgumentOutOfRangeException>(() => CharacterTurnParticipantLookup.Find(
            maximum.Append(CreateCharacter("extra-participant")),
            activity));
        Assert.Same(
            maximum[0],
            CharacterTurnParticipantLookup.Find(
                maximum,
                CreateActivity(maximum[0].Identity.Id, CharacterTurnActivityState.AwaitingResponse)));
    }

    [Fact]
    public void PreflightsTheCompleteRosterBeforeReturningAMatch()
    {
        Character selected = CreateCharacter("selected");
        Character invalidLaterParticipant = AsHuman(CreateCharacter("invalid-later"));
        CharacterTurnActivity activity = CreateActivity(
            selected.Identity.Id,
            CharacterTurnActivityState.AwaitingResponse);

        Assert.Throws<ArgumentException>(() => CharacterTurnParticipantLookup.Find(
            [selected, invalidLaterParticipant],
            activity));
    }

    [Fact]
    public void ExposesOnlyThePureFindOperation()
    {
        Type type = typeof(CharacterTurnParticipantLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            type.GetMethods().Where(candidate => candidate.DeclaringType == type));

        Assert.Equal("Find", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Character), method.ReturnType);
        Assert.Equal(
            [typeof(IEnumerable<Character>), typeof(CharacterTurnActivity)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(type.GetConstructors());
        Assert.Empty(type.GetProperties());
        Assert.Empty(type.GetEvents());
        Assert.True(type.IsAbstract && type.IsSealed);
    }

    private static CharacterTurnActivity CreateActivity(
        CharacterId respondingCharacterId,
        CharacterTurnActivityState state)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "What do you think?");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [respondingCharacterId]));
        CharacterTurnRequest request = new(
            respondingCharacterId,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
        return new CharacterTurnActivity(request, null, state);
    }

    private static Character CreateCharacter(string idValue)
    {
        CharacterId id = new(idValue);
        return new Character(
            new CharacterIdentity(id, idValue.ToUpperInvariant()),
            new PublicCharacterBackground("A scheduled participant."),
            new PrivateCharacterBackground("A private scheduled participant."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal("participate", "Participate when appropriate.")]));
    }

    private static Character AsHuman(Character character)
    {
        return new Character(
            character.Identity,
            character.PublicBackground,
            character.PrivateBackground,
            character.Capabilities,
            character.Limitations,
            new CharacterControllerAssignment(character.Identity.Id, ControllerType.Human),
            character.Conditions,
            character.Inventory,
            character.Knowledge,
            character.Beliefs,
            character.Relationships,
            character.Goals);
    }
}
