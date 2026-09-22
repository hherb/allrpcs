using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterSabotageActionConsiderationControllerTests
{
    [Fact]
    public async Task ForwardsExactRequestAndCancellationOnceAndReturnsValidatedResult()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Delay]);
        CharacterSabotageActionDecision decision = new(
            actor.Identity.Id,
            CharacterSabotageActionKind.Delay);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterSabotageActionConsiderationResult result =
            await CharacterSabotageActionConsiderationController.DecideAsync(
                model,
                request,
                cancellation.Token);

        Assert.Equal(1, model.CallCount);
        Assert.Same(request, model.Request);
        Assert.Equal(cancellation.Token, model.CancellationToken);
        Assert.Same(request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(CharacterSabotageActionKind.Delay, result.SelectedActionKind);
    }

    [Fact]
    public async Task RetainsAbstentionAndRejectsInvalidOrNullOutput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Conceal]);
        CharacterSabotageActionDecision abstention = new(actor.Identity.Id, null);

        CharacterSabotageActionConsiderationResult result =
            await CharacterSabotageActionConsiderationController.DecideAsync(
                new RecordingModel(abstention),
                request,
                CancellationToken.None);

        Assert.Null(result.SelectedActionKind);
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterSabotageActionConsiderationController.DecideAsync(
                new RecordingModel(new(
                    new CharacterId(actor.Identity.Id.Value),
                    CharacterSabotageActionKind.Conceal)),
                request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterSabotageActionConsiderationController.DecideAsync(
                new NullModel(),
                request,
                CancellationToken.None));
        Assert.Same(CharacterSabotageActionKind.Conceal, Assert.Single(request.ActionKinds));
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(actor, []);
        RecordingModel model = new(new(actor.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterSabotageActionConsiderationController.DecideAsync(
                null!,
                request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterSabotageActionConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterSabotageActionConsiderationController.DecideAsync(
                model,
                request,
                cancellation.Token));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public async Task PropagatesModelCancellationAndFailureUnchanged()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(actor, []);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        InvalidOperationException expected = new("model failed");

        OperationCanceledException cancelled = await Assert.ThrowsAnyAsync<
            OperationCanceledException>(async () =>
                await CharacterSabotageActionConsiderationController.DecideAsync(
                    new CancellingModel(cancellation.Token),
                    request,
                    CancellationToken.None));
        InvalidOperationException failed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CharacterSabotageActionConsiderationController.DecideAsync(
                new FailingModel(expected),
                request,
                CancellationToken.None));

        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.Same(expected, failed);
    }

    [Fact]
    public void ExposesOnlyOneProviderNeutralModelAndStaticDecisionOperation()
    {
        Type modelType = typeof(ICharacterSabotageActionDecisionModel);
        System.Reflection.MethodInfo modelMethod = Assert.Single(modelType.GetMethods());
        Assert.Equal("DecideAsync", modelMethod.Name);
        Assert.Equal(typeof(ValueTask<CharacterSabotageActionDecision>), modelMethod.ReturnType);
        Assert.Equal(
            [typeof(CharacterSabotageActionConsiderationRequest), typeof(CancellationToken)],
            modelMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());

        Type controllerType = typeof(CharacterSabotageActionConsiderationController);
        System.Reflection.MethodInfo controllerMethod = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));
        Assert.Equal("DecideAsync", controllerMethod.Name);
        Assert.True(controllerMethod.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterSabotageActionConsiderationResult>),
            controllerMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterSabotageActionDecisionModel),
                typeof(CharacterSabotageActionConsiderationRequest),
                typeof(CancellationToken),
            ],
            controllerMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private sealed class RecordingModel(CharacterSabotageActionDecision decision)
        : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class NullModel : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<CharacterSabotageActionDecision>(null!);
    }

    private sealed class CancellingModel(CancellationToken cancellationToken)
        : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken ignoredCancellationToken) =>
            ValueTask.FromCanceled<CharacterSabotageActionDecision>(cancellationToken);
    }

    private sealed class FailingModel(Exception exception) : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterSabotageActionDecision>(exception);
    }
}
