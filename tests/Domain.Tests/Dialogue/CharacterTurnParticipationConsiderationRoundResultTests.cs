using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationConsiderationRoundResultTests
{
    [Fact]
    public void RetainsStableExactRequestsAndResultsForACompleteRound()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id, sage.Identity.Id);
        CharacterTurnParticipationConsiderationRequest hermitRequest =
            new(hermit, heardEvent, false);
        CharacterTurnParticipationConsiderationRequest sageRequest =
            new(sage, heardEvent, true);
        CharacterTurnParticipationConsiderationResult hermitResult =
            Result(hermitRequest, CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationResult sageResult =
            Result(sageRequest, CharacterTurnParticipationKind.Respond);

        CharacterTurnParticipationConsiderationRoundResult round = new(
            [hermitRequest, sageRequest],
            [hermitResult, sageResult]);

        Assert.Collection(
            round.Requests,
            request => Assert.Same(hermitRequest, request),
            request => Assert.Same(sageRequest, request));
        Assert.Collection(
            round.Results,
            result => Assert.Same(hermitResult, result),
            result => Assert.Same(sageResult, result));
        Assert.Same(heardEvent, round.Requests[0].HeardDialogueEvent);
        Assert.Same(heardEvent, round.Requests[1].HeardDialogueEvent);
    }

    [Fact]
    public void AcceptsEmptySingleAndMaximumRounds()
    {
        Character singleCharacter = CreateCharacter("single-character");
        DialogueEvent singleEvent = CreateEvent(singleCharacter.Identity.Id);
        CharacterTurnParticipationConsiderationRequest singleRequest =
            new(singleCharacter, singleEvent, true);
        CharacterTurnParticipationConsiderationResult singleResult =
            Result(singleRequest, CharacterTurnParticipationKind.Respond);
        Character[] maximumCharacters = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => CreateCharacter($"character-{index}"))
            .ToArray();
        DialogueEvent maximumEvent = CreateEvent(
            maximumCharacters.Select(character => character.Identity.Id).ToArray());
        CharacterTurnParticipationConsiderationRequest[] maximumRequests = maximumCharacters
            .Select(character => new CharacterTurnParticipationConsiderationRequest(
                character,
                maximumEvent,
                false))
            .ToArray();
        CharacterTurnParticipationConsiderationResult[] maximumResults = maximumRequests
            .Select(request => Result(request, CharacterTurnParticipationKind.RemainSilent))
            .ToArray();

        CharacterTurnParticipationConsiderationRoundResult empty = new([], []);
        CharacterTurnParticipationConsiderationRoundResult single =
            new([singleRequest], [singleResult]);
        CharacterTurnParticipationConsiderationRoundResult maximum =
            new(maximumRequests, maximumResults);

        Assert.Empty(empty.Requests);
        Assert.Empty(empty.Results);
        Assert.Same(singleRequest, Assert.Single(single.Requests));
        Assert.Same(singleResult, Assert.Single(single.Results));
        Assert.Equal(DialogueAudienceSnapshot.MaximumListenerCount, maximum.Requests.Count);
        Assert.Equal(DialogueAudienceSnapshot.MaximumListenerCount, maximum.Results.Count);
    }

    [Fact]
    public void RequiresExactPositionallyAlignedRequestInstances()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id, sage.Identity.Id);
        CharacterTurnParticipationConsiderationRequest hermitRequest =
            new(hermit, heardEvent, false);
        CharacterTurnParticipationConsiderationRequest sageRequest =
            new(sage, heardEvent, false);
        CharacterTurnParticipationConsiderationRequest equalHermitRequest =
            new(hermit, heardEvent, false);
        CharacterTurnParticipationConsiderationResult hermitResult =
            Result(hermitRequest, CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationResult sageResult =
            Result(sageRequest, CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationResult equalHermitResult =
            Result(equalHermitRequest, CharacterTurnParticipationKind.RemainSilent);

        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(
                [hermitRequest, sageRequest],
                [sageResult, hermitResult]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(
                [hermitRequest],
                [equalHermitResult]));
        Assert.NotSame(hermitRequest, equalHermitResult.Request);
        Assert.Equal(hermitRequest, equalHermitResult.Request);
    }

    [Fact]
    public void RejectsIncompleteOversizedDuplicateCharacterOrMixedEventRounds()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character valueEqualHermit = CreateCharacter("woodland-hermit");
        DialogueEvent firstEvent = CreateEvent(hermit.Identity.Id);
        DialogueEvent secondEvent = CreateEvent(hermit.Identity.Id);
        CharacterTurnParticipationConsiderationRequest firstRequest =
            new(hermit, firstEvent, false);
        CharacterTurnParticipationConsiderationRequest duplicateRequest =
            new(valueEqualHermit, firstEvent, false);
        CharacterTurnParticipationConsiderationRequest mixedEventRequest =
            new(hermit, secondEvent, false);
        CharacterTurnParticipationConsiderationResult firstResult =
            Result(firstRequest, CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationResult duplicateResult =
            Result(duplicateRequest, CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationResult mixedEventResult =
            Result(mixedEventRequest, CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationRequest[] oversizedRequests = Enumerable
            .Repeat(firstRequest, DialogueAudienceSnapshot.MaximumListenerCount + 1)
            .ToArray();
        CharacterTurnParticipationConsiderationResult[] oversizedResults = Enumerable
            .Repeat(firstResult, DialogueAudienceSnapshot.MaximumListenerCount + 1)
            .ToArray();

        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult([firstRequest], []));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(
                [firstRequest],
                [firstResult, firstResult]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(
                [firstRequest, duplicateRequest],
                [firstResult, duplicateResult]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(
                [firstRequest, mixedEventRequest],
                [firstResult, mixedEventResult]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(
                oversizedRequests,
                oversizedResults));
        Assert.Same(firstEvent, firstRequest.HeardDialogueEvent);
        Assert.Same(secondEvent, mixedEventRequest.HeardDialogueEvent);
    }

    [Fact]
    public void RejectsNullsAndReturnsIndependentReadOnlyCollections()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        CharacterTurnParticipationConsiderationRequest request =
            new(character, heardEvent, true);
        CharacterTurnParticipationConsiderationResult result =
            Result(request, CharacterTurnParticipationKind.Respond);
        CharacterTurnParticipationConsiderationRequest[] requests = [request];
        CharacterTurnParticipationConsiderationResult[] results = [result];

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(null!, results));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult(requests, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult([null!], [result]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationRoundResult([request], [null!]));

        CharacterTurnParticipationConsiderationRoundResult round = new(requests, results);
        requests[0] = new(character, heardEvent, true);
        results[0] = Result(requests[0], CharacterTurnParticipationKind.RemainSilent);

        Assert.Same(request, Assert.Single(round.Requests));
        Assert.Same(result, Assert.Single(round.Results));
        Assert.Throws<NotSupportedException>(() =>
            ((System.Collections.IList)round.Requests).Add(request));
        Assert.Throws<NotSupportedException>(() =>
            ((System.Collections.IList)round.Results).Add(result));
    }

    [Fact]
    public void ExposesOnlyTheImmutableRoundSurface()
    {
        Type roundType = typeof(CharacterTurnParticipationConsiderationRoundResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(roundType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = roundType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(IEnumerable<CharacterTurnParticipationConsiderationRequest>),
                typeof(IEnumerable<CharacterTurnParticipationConsiderationResult>),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Requests", typeof(IReadOnlyList<CharacterTurnParticipationConsiderationRequest>)),
                ("Results", typeof(IReadOnlyList<CharacterTurnParticipationConsiderationResult>)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(roundType.GetEvents());
        Assert.True(roundType.IsSealed);
    }

    private static CharacterTurnParticipationConsiderationResult Result(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationKind kind)
    {
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : Assert.Single(request.ScheduledCharacter.Goals.Entries);
        CharacterTurnParticipationDecision decision = new(
            request.ScheduledCharacter.Identity.Id,
            kind,
            motive);

        return new(request, decision);
    }

    private static DialogueEvent CreateEvent(params CharacterId[] listeners)
    {
        CharacterId speakerCharacterId = new("seeker");

        return new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
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
