using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageDelayRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterTargetSabotageDelayRecoveryConsiderationResult result =
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token);

        Assert.Equal(1, model.CallCount);
        Assert.Same(input.Request, model.Request);
        Assert.Equal(cancellation.Token, model.CancellationToken);
        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public async Task RetainsExactAcceptedAbstention()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageDelayRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageDelayRecoveryConsiderationResult result =
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None);

        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public async Task RejectsInvalidOrNullDecisionWithoutChangingRequest()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageDelayRecoveryDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                new RecordingModel(invalid),
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                new NullModel(),
                input.Request,
                CancellationToken.None));
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        ControllerInput input = CreateInput();
        RecordingModel model = new(new(input.Actor.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public async Task PropagatesModelCancellationAndFailureUnchanged()
    {
        ControllerInput input = CreateInput();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        InvalidOperationException expected = new("model failed");

        OperationCanceledException cancelled = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
                await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                    new CancellingModel(cancellation.Token),
                    input.Request,
                    CancellationToken.None));
        InvalidOperationException failed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await CharacterTargetSabotageDelayRecoveryConsiderationController.DecideAsync(
                    new FailingModel(expected),
                    input.Request,
                    CancellationToken.None));

        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.Same(expected, failed);
    }

    [Fact]
    public void ExposesOnlyOneProviderNeutralModelAndStaticDecisionOperation()
    {
        Type modelType = typeof(ICharacterTargetSabotageDelayRecoveryDecisionModel);
        System.Reflection.MethodInfo modelMethod = Assert.Single(modelType.GetMethods());
        Assert.Equal("DecideAsync", modelMethod.Name);
        Assert.Equal(
            typeof(ValueTask<CharacterTargetSabotageDelayRecoveryDecision>),
            modelMethod.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest),
                typeof(CancellationToken),
            ],
            modelMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());

        Type controllerType = typeof(
            CharacterTargetSabotageDelayRecoveryConsiderationController);
        System.Reflection.MethodInfo controllerMethod = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));
        Assert.Equal("DecideAsync", controllerMethod.Name);
        Assert.True(controllerMethod.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterTargetSabotageDelayRecoveryConsiderationResult>),
            controllerMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterTargetSabotageDelayRecoveryDecisionModel),
                typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest),
                typeof(CancellationToken),
            ],
            controllerMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput CreateInput()
    {
        Character enemyAgent = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemyAgent.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemyAgent.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            actor);
        WorldEvent priorEvent = new(
            new WorldEventId("sage-began-round"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent delayEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("sage-delay-recovered"),
            WorldEventTimeEffect.Advance);
        WorldEventLogSnapshot eventLog = new([priorEvent.EventId, delayEvent.EventId]);
        WorldTimeSnapshot worldTime = new(delayEvent.EventId);
        CharacterTargetSabotageDelayResult delayResult = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            eventLog,
            worldTime);
        CharacterTargetSabotageDelayRecoveryCandidate candidate = new(
            delayResult,
            recoveryEvent);
        WorldEventSnapshot worldEvents = new([priorEvent, delayEvent, recoveryEvent]);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request = new(
            actor,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new(actor, delayEvent, worldEvents, eventLog, worldTime, candidate, request);
    }

    private sealed class RecordingModel(CharacterTargetSabotageDelayRecoveryDecision decision)
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class NullModel : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<CharacterTargetSabotageDelayRecoveryDecision>(null!);
    }

    private sealed class CancellingModel(CancellationToken cancellationToken)
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken ignoredCancellationToken) =>
            ValueTask.FromCanceled<CharacterTargetSabotageDelayRecoveryDecision>(
                cancellationToken);
    }

    private sealed class FailingModel(Exception exception)
        : ICharacterTargetSabotageDelayRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
            CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageDelayRecoveryDecision>(exception);
    }

    private sealed record ControllerInput(
        Character Actor,
        WorldEvent DelayEvent,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayRecoveryCandidate Candidate,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest Request);
}
