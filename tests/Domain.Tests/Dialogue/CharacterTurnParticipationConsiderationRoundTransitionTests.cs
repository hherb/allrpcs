using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationConsiderationRoundTransitionTests
{
    [Fact]
    public void CreatesStableExactRequestsWithValueEqualDirectAddressMembership()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(
            new CharacterId(hermit.Identity.Id.Value),
            new CharacterId(sage.Identity.Id.Value));
        CharacterId directAddress = new(sage.Identity.Id.Value);

        IReadOnlyList<CharacterTurnParticipationConsiderationRequest> requests =
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [hermit, sage],
                heardEvent,
                [directAddress]);

        Assert.Collection(
            requests,
            request =>
            {
                Assert.Same(hermit, request.ScheduledCharacter);
                Assert.Same(heardEvent, request.HeardDialogueEvent);
                Assert.False(request.IsDirectlyAddressed);
            },
            request =>
            {
                Assert.Same(sage, request.ScheduledCharacter);
                Assert.Same(heardEvent, request.HeardDialogueEvent);
                Assert.True(request.IsDirectlyAddressed);
            });
        Assert.NotSame(sage.Identity.Id, directAddress);
        Assert.Equal(sage.Identity.Id, directAddress);
    }

    [Fact]
    public void AcceptsAnEmptyRoundAndIgnoresAnAbsentDirectAddress()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id);

        IReadOnlyList<CharacterTurnParticipationConsiderationRequest> empty =
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [],
                heardEvent,
                [new CharacterId("absent-character")]);
        IReadOnlyList<CharacterTurnParticipationConsiderationRequest> unaddressed =
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [hermit],
                heardEvent,
                [new CharacterId("absent-character")]);

        Assert.Empty(empty);
        Assert.False(Assert.Single(unaddressed).IsDirectlyAddressed);
    }

    [Fact]
    public void PreflightsEveryScheduledCharacterBeforeCreatingARequest()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character absentSage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id);
        Character[] characters = [hermit, absentSage];

        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                characters,
                heardEvent,
                []));
        Assert.Same(hermit, characters[0]);
        Assert.Same(absentSage, characters[1]);
        Assert.Same(hermit.Identity.Id, heardEvent.Audience.ListenerCharacterIds[0]);
    }

    [Fact]
    public void RejectsNullDuplicateOrOversizedInputs()
    {
        Character character = CreateCharacter("scheduled-character");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        Character[] maximumCharacters = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => CreateCharacter($"character-{index}"))
            .ToArray();
        DialogueEvent maximumEvent = CreateEvent(
            maximumCharacters.Select(candidate => candidate.Identity.Id).ToArray());
        CharacterId[] maximumAddresses = maximumCharacters
            .Select(candidate => candidate.Identity.Id)
            .ToArray();

        Assert.Equal(
            DialogueAudienceSnapshot.MaximumListenerCount,
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                maximumCharacters,
                maximumEvent,
                maximumAddresses).Count);
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                null!,
                heardEvent,
                []));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [],
                null!,
                []));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [],
                heardEvent,
                null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [null!],
                heardEvent,
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [character, character],
                heardEvent,
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [character],
                heardEvent,
                [null!]));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                [character],
                heardEvent,
                [character.Identity.Id, new CharacterId(character.Identity.Id.Value)]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                maximumCharacters.Append(CreateCharacter("extra-character")),
                maximumEvent,
                maximumAddresses));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                maximumCharacters,
                maximumEvent,
                maximumAddresses.Append(new CharacterId("extra-address"))));
    }

    [Fact]
    public void ReturnsAnIndependentReadOnlyCollectionWithoutChangingInputs()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        Character[] characters = [character];
        CharacterId[] addresses = [character.Identity.Id];

        IReadOnlyList<CharacterTurnParticipationConsiderationRequest> requests =
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                characters,
                heardEvent,
                addresses);

        characters[0] = CreateCharacter("replacement");
        addresses[0] = new CharacterId("replacement");
        CharacterTurnParticipationConsiderationRequest request = Assert.Single(requests);
        Assert.Same(character, request.ScheduledCharacter);
        Assert.Same(heardEvent, request.HeardDialogueEvent);
        Assert.True(request.IsDirectlyAddressed);
        Assert.IsAssignableFrom<System.Collections.IList>(requests);
        Assert.Throws<NotSupportedException>(() =>
            ((System.Collections.IList)requests).Add(request));
    }

    [Fact]
    public void ExposesOnlyThePureRoundCreationOperation()
    {
        Type transitionType = typeof(CharacterTurnParticipationConsiderationRoundTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("CreateRequests", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(IReadOnlyList<CharacterTurnParticipationConsiderationRequest>),
            method.ReturnType);
        Assert.Equal(
            [typeof(IEnumerable<Character>), typeof(DialogueEvent), typeof(IEnumerable<CharacterId>)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static DialogueEvent CreateEvent(params CharacterId[] listeners)
    {
        CharacterId speakerCharacterId = new("seeker");

        return new DialogueEvent(
            new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
            new DialogueAudienceSnapshot(speakerCharacterId, listeners));
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
