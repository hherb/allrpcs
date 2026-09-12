using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnParticipationConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneValidatedResultPerExactRequestInStableOrder()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id, sage.Identity.Id);
        RecordingModel model = new(request => Decision(
            request,
            request.IsDirectlyAddressed
                ? CharacterTurnParticipationKind.Respond
                : CharacterTurnParticipationKind.RemainSilent));
        using CancellationTokenSource cancellation = new();

        CharacterTurnParticipationConsiderationRoundResult round =
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model,
                [hermit, sage],
                heardEvent,
                [new CharacterId(sage.Identity.Id.Value)],
                cancellation.Token);

        Assert.Collection(
            model.Requests,
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
        Assert.All(model.CancellationTokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(2, round.Requests.Count);
        Assert.Equal(2, round.Results.Count);
        for (int index = 0; index < round.Requests.Count; index++)
        {
            Assert.Same(model.Requests[index], round.Requests[index]);
            Assert.Same(round.Requests[index], round.Results[index].Request);
            Assert.Same(model.Decisions[index], round.Results[index].Decision);
        }
    }

    [Fact]
    public async Task AcceptsAnEmptyRoundWithoutInvokingTheModel()
    {
        Character listener = CreateCharacter("listener");
        RecordingModel model = new(request => Decision(
            request,
            CharacterTurnParticipationKind.RemainSilent));

        CharacterTurnParticipationConsiderationRoundResult round =
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model,
                [],
                CreateEvent(listener.Identity.Id),
                [],
                CancellationToken.None);

        Assert.Empty(model.Requests);
        Assert.Empty(round.Requests);
        Assert.Empty(round.Results);
    }

    [Fact]
    public async Task PreflightsTheCompleteRequestRoundBeforeInvokingTheModel()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character absentSage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id);
        RecordingModel model = new(request => Decision(
            request,
            CharacterTurnParticipationKind.RemainSilent));

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model,
                [hermit, absentSage],
                heardEvent,
                [],
                CancellationToken.None));

        Assert.Empty(model.Requests);
        Assert.Same(hermit.Identity.Id, heardEvent.Audience.ListenerCharacterIds[0]);
    }

    [Fact]
    public async Task RejectsInvalidOrMissingModelDecisionsWithoutReturningAPartialRound()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        RecordingModel invalid = new(request => new CharacterTurnParticipationDecision(
            new CharacterId(request.ScheduledCharacter.Identity.Id.Value),
            CharacterTurnParticipationKind.Respond,
            null));
        RecordingModel missing = new(_ => null!);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                invalid,
                [character],
                heardEvent,
                [character.Identity.Id],
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                missing,
                [character],
                heardEvent,
                [character.Identity.Id],
                CancellationToken.None));

        Assert.Single(invalid.Requests);
        Assert.Single(missing.Requests);
    }

    [Fact]
    public async Task PropagatesTheExactCallerCancellationToken()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        CancellingModel model = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model,
                [character],
                heardEvent,
                [character.Identity.Id],
                cancellation.Token));

        Assert.Equal(cancellation.Token, model.CancellationToken);
    }

    [Fact]
    public async Task RejectsNullsAndExposesOnlyTheBoundedAsyncOperation()
    {
        Character character = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(character.Identity.Id);
        RecordingModel model = new(request => Decision(
            request,
            CharacterTurnParticipationKind.Respond));

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                null!, [character], heardEvent, [], CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model, null!, heardEvent, [], CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model, [character], null!, [], CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTurnParticipationConsiderationController.DecideRoundAsync(
                model, [character], heardEvent, null!, CancellationToken.None));

        Type controllerType = typeof(CharacterTurnParticipationConsiderationController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("DecideRoundAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterTurnParticipationConsiderationRoundResult>),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterTurnParticipationModel),
                typeof(IEnumerable<Character>),
                typeof(DialogueEvent),
                typeof(IEnumerable<CharacterId>),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetEvents());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static CharacterTurnParticipationDecision Decision(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationKind kind)
    {
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : Assert.Single(request.ScheduledCharacter.Goals.Entries);
        return new(request.ScheduledCharacter.Identity.Id, kind, motive);
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

    private sealed class RecordingModel(
        Func<CharacterTurnParticipationConsiderationRequest, CharacterTurnParticipationDecision>
            decide)
        : ICharacterTurnParticipationModel
    {
        internal List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

        internal List<CharacterTurnParticipationDecision> Decisions { get; } = [];

        internal List<CancellationToken> CancellationTokens { get; } = [];

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            CancellationTokens.Add(cancellationToken);
            CharacterTurnParticipationDecision decision = decide(request);
            Decisions.Add(decision);
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class CancellingModel : ICharacterTurnParticipationModel
    {
        internal CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return ValueTask.FromCanceled<CharacterTurnParticipationDecision>(cancellationToken);
        }
    }
}
