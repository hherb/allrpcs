using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterTargetSabotageDelayConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageDelayDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterTargetSabotageDelayConsiderationResult result =
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
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
        CharacterTargetSabotageDelayDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageDelayConsiderationResult result =
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
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
        CharacterTargetSabotageDelayDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
                new RecordingModel(invalid),
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
                new NullModel(),
                input.Request,
                CancellationToken.None));
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Empty(input.EventLog.Entries);
        Assert.Null(input.WorldTime.CurrentEventId);
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        ControllerInput input = CreateInput();
        RecordingModel model = new(new(input.Actor.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
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

        OperationCanceledException cancelled = await Assert.ThrowsAnyAsync<
            OperationCanceledException>(async () =>
                await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
                    new CancellingModel(cancellation.Token),
                    input.Request,
                    CancellationToken.None));
        InvalidOperationException failed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CharacterTargetSabotageDelayConsiderationController.DecideAsync(
                new FailingModel(expected),
                input.Request,
                CancellationToken.None));

        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.Same(expected, failed);
    }

    [Fact]
    public void ExposesOnlyOneProviderNeutralModelAndStaticDecisionOperation()
    {
        Type modelType = typeof(ICharacterTargetSabotageDelayDecisionModel);
        System.Reflection.MethodInfo modelMethod = Assert.Single(modelType.GetMethods());
        Assert.Equal("DecideAsync", modelMethod.Name);
        Assert.Equal(
            typeof(ValueTask<CharacterTargetSabotageDelayDecision>),
            modelMethod.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageDelayConsiderationRequest),
                typeof(CancellationToken),
            ],
            modelMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());

        Type controllerType = typeof(CharacterTargetSabotageDelayConsiderationController);
        System.Reflection.MethodInfo controllerMethod = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));
        Assert.Equal("DecideAsync", controllerMethod.Name);
        Assert.True(controllerMethod.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterTargetSabotageDelayConsiderationResult>),
            controllerMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterTargetSabotageDelayDecisionModel),
                typeof(CharacterTargetSabotageDelayConsiderationRequest),
                typeof(CancellationToken),
            ],
            controllerMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent worldEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTargetSabotageDelayConsiderationRequest request = new(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new(actor, worldEvents, eventLog, worldTime, candidate, request);
    }

    private sealed class RecordingModel(CharacterTargetSabotageDelayDecision decision)
        : ICharacterTargetSabotageDelayDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class NullModel : ICharacterTargetSabotageDelayDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<CharacterTargetSabotageDelayDecision>(null!);
    }

    private sealed class CancellingModel(CancellationToken cancellationToken)
        : ICharacterTargetSabotageDelayDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken ignoredCancellationToken) =>
            ValueTask.FromCanceled<CharacterTargetSabotageDelayDecision>(cancellationToken);
    }

    private sealed class FailingModel(Exception exception)
        : ICharacterTargetSabotageDelayDecisionModel
    {
        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageDelayDecision>(exception);
    }

    private sealed record ControllerInput(
        Character Actor,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageDelayCandidate Candidate,
        CharacterTargetSabotageDelayConsiderationRequest Request);
}
