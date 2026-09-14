using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityResponseRoundControllerTests
{
    [Fact]
    public async Task ObtainsOneExactResponsePerPerspectiveInStableDeclarationOrder()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        RecordingModel model = new(request => new(
            request.Perspective,
            ReferenceEquals(request.Perspective.SubjectCharacter, declaration.FirstCharacter)
                ? CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation
                : CharacterBeliefGoalIncompatibilityResponseKind.ReconsiderGoal,
            "Return one response for this exact perspective."));
        using CancellationTokenSource cancellation = new();
        CharacterBelief[] firstBeliefs = [.. declaration.FirstCharacter.Beliefs.Entries];
        CharacterGoal[] secondGoals = [.. declaration.SecondCharacter.Goals.Entries];

        CharacterBeliefGoalIncompatibilityResponseRound round =
            await CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                cancellation.Token);

        Assert.Equal(2, model.Requests.Count);
        Assert.Same(declaration.FirstCharacter, model.Requests[0].Perspective.SubjectCharacter);
        Assert.Same(declaration.SecondCharacter, model.Requests[1].Perspective.SubjectCharacter);
        Assert.Same(round.Perspectives[0], model.Requests[0].Perspective);
        Assert.Same(round.Perspectives[1], model.Requests[1].Perspective);
        Assert.Same(model.Responses[0], round.Responses[0]);
        Assert.Same(model.Responses[1], round.Responses[1]);
        Assert.All(model.CancellationTokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Same(declaration, round.Perspectives[0].Declaration);
        Assert.Same(declaration, round.Perspectives[1].Declaration);
        Assert.Equal(firstBeliefs, declaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(secondGoals, declaration.SecondCharacter.Goals.Entries);
    }

    [Fact]
    public async Task InvalidSecondResponseFailsWithoutChangingEitherCharacter()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective replacementSecond = new(
            declaration,
            declaration.SecondCharacter);
        RecordingModel model = new(request => new(
            ReferenceEquals(request.Perspective.SubjectCharacter, declaration.FirstCharacter)
                ? request.Perspective
                : replacementSecond,
            CharacterBeliefGoalIncompatibilityResponseKind.MaintainPosition,
            "Attempt to answer one supplied perspective."));
        CharacterBelief[] firstBeliefs = [.. declaration.FirstCharacter.Beliefs.Entries];
        CharacterGoal[] firstGoals = [.. declaration.FirstCharacter.Goals.Entries];
        CharacterBelief[] secondBeliefs = [.. declaration.SecondCharacter.Beliefs.Entries];
        CharacterGoal[] secondGoals = [.. declaration.SecondCharacter.Goals.Entries];

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                CancellationToken.None));

        Assert.Equal(2, model.Requests.Count);
        Assert.NotSame(model.Requests[1].Perspective, replacementSecond);
        Assert.Equal(model.Requests[1].Perspective, replacementSecond);
        Assert.Equal(firstBeliefs, declaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(firstGoals, declaration.FirstCharacter.Goals.Entries);
        Assert.Equal(secondBeliefs, declaration.SecondCharacter.Beliefs.Entries);
        Assert.Equal(secondGoals, declaration.SecondCharacter.Goals.Entries);
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        RecordingModel model = new(Response);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                null!,
                declaration,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                cancellation.Token));

        Assert.Empty(model.Requests);
    }

    [Fact]
    public async Task CancellationAfterFirstResponseStopsBeforeSecondModelInvocation()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        using CancellationTokenSource cancellation = new();
        RecordingModel model = new(request =>
        {
            cancellation.Cancel();
            return Response(request);
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                cancellation.Token));

        Assert.Single(model.Requests);
        Assert.Same(declaration.FirstCharacter, model.Requests[0].Perspective.SubjectCharacter);
    }

    [Fact]
    public async Task PropagatesModelFailureWithoutStartingTheSecondResponse()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        FailingModel model = new();

        InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                CancellationToken.None));

        Assert.Equal("model failure", failure.Message);
        Assert.Equal(1, model.CallCount);
    }

    [Fact]
    public async Task AppliesOneSharedDeadlineToTheCompleteResponseRound()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        PartialThenWaitingModel model = new();

        await Assert.ThrowsAsync<TimeoutException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                TimeSpan.FromMilliseconds(20),
                CancellationToken.None));

        Assert.Equal(2, model.Requests.Count);
        Assert.Same(declaration.FirstCharacter, model.Requests[0].Perspective.SubjectCharacter);
        Assert.Same(declaration.SecondCharacter, model.Requests[1].Perspective.SubjectCharacter);
        Assert.Equal(model.CancellationTokens[0], model.CancellationTokens[1]);
        Assert.True(model.CancellationTokens[1].IsCancellationRequested);
    }

    [Fact]
    public async Task DeadlineOverloadDistinguishesCallerCancellationAndRejectsInvalidDeadlines()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        WaitingModel waiting = new();
        using CancellationTokenSource cancellation = new();
        Task<CharacterBeliefGoalIncompatibilityResponseRound> pending =
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                waiting,
                declaration,
                TimeSpan.FromSeconds(1),
                cancellation.Token);

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.True(waiting.CancellationToken.IsCancellationRequested);

        RecordingModel model = new(Response);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                TimeSpan.Zero,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                Timeout.InfiniteTimeSpan,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                null!,
                declaration,
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
        Assert.Empty(model.Requests);
    }

    [Fact]
    public async Task DeadlineOverloadReturnsTheExactCompleteRoundWithinItsBound()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        RecordingModel model = new(Response);

        CharacterBeliefGoalIncompatibilityResponseRound round =
            await CharacterBeliefGoalIncompatibilityResponseRoundController.RespondAsync(
                model,
                declaration,
                TimeSpan.FromSeconds(1),
                CancellationToken.None);

        Assert.Equal(2, round.Responses.Count);
        Assert.Equal(model.CancellationTokens[0], model.CancellationTokens[1]);
        Assert.NotEqual(CancellationToken.None, model.CancellationTokens[0]);
    }

    [Fact]
    public void ExposesOnlyTheTwoStaticResponseRoundOperations()
    {
        Type controllerType = typeof(CharacterBeliefGoalIncompatibilityResponseRoundController);
        System.Reflection.MethodInfo[] methods = controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType).ToArray();

        Assert.Equal(2, methods.Length);
        Assert.All(methods, method =>
        {
            Assert.Equal("RespondAsync", method.Name);
            Assert.True(method.IsStatic);
            Assert.Equal(
                typeof(Task<CharacterBeliefGoalIncompatibilityResponseRound>),
                method.ReturnType);
        });
        Assert.Contains(methods, method => method.GetParameters().Select(
            parameter => parameter.ParameterType).SequenceEqual(
            [
                typeof(ICharacterBeliefGoalIncompatibilityResponseModel),
                typeof(CharacterBeliefGoalIncompatibility),
                typeof(CancellationToken),
            ]));
        Assert.Contains(methods, method => method.GetParameters().Select(
            parameter => parameter.ParameterType).SequenceEqual(
            [
                typeof(ICharacterBeliefGoalIncompatibilityResponseModel),
                typeof(CharacterBeliefGoalIncompatibility),
                typeof(TimeSpan),
                typeof(CancellationToken),
            ]));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.Empty(controllerType.GetEvents());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static CharacterBeliefGoalIncompatibilityResponse Response(
        CharacterBeliefGoalIncompatibilityConsiderationRequest request) => new(
            request.Perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation,
            "Seek a response supported by both perspectives.");

    private sealed class RecordingModel(
        Func<CharacterBeliefGoalIncompatibilityConsiderationRequest,
            CharacterBeliefGoalIncompatibilityResponse> respond)
        : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public List<CharacterBeliefGoalIncompatibilityConsiderationRequest> Requests { get; } = [];

        public List<CharacterBeliefGoalIncompatibilityResponse> Responses { get; } = [];

        public List<CancellationToken> CancellationTokens { get; } = [];

        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            CancellationTokens.Add(cancellationToken);
            CharacterBeliefGoalIncompatibilityResponse response = respond(request);
            Responses.Add(response);
            return ValueTask.FromResult(response);
        }
    }

    private sealed class FailingModel : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return ValueTask.FromException<CharacterBeliefGoalIncompatibilityResponse>(
                new InvalidOperationException("model failure"));
        }
    }

    private sealed class PartialThenWaitingModel : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public List<CharacterBeliefGoalIncompatibilityConsiderationRequest> Requests { get; } = [];

        public List<CancellationToken> CancellationTokens { get; } = [];

        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            CancellationTokens.Add(cancellationToken);
            return Requests.Count == 1
                ? ValueTask.FromResult(Response(request))
                : new ValueTask<CharacterBeliefGoalIncompatibilityResponse>(
                    WaitForCancellationAsync(cancellationToken));
        }
    }

    private sealed class WaitingModel : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return new ValueTask<CharacterBeliefGoalIncompatibilityResponse>(
                WaitForCancellationAsync(cancellationToken));
        }
    }

    private static async Task<CharacterBeliefGoalIncompatibilityResponse> WaitForCancellationAsync(
        CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("An infinite delay must complete only through cancellation.");
    }
}
