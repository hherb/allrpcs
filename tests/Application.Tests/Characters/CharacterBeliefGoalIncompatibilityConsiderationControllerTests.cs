using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityConsiderationControllerTests
{
    [Theory]
    [InlineData("maintain-position")]
    [InlineData("reconsider-belief")]
    [InlineData("reconsider-goal")]
    [InlineData("seek-reconciliation")]
    public async Task ObtainsOneExactResponseAndReturnsValidatedResult(string kindValue)
    {
        ControllerInput input = Input();
        CharacterBeliefGoalIncompatibilityResponse response = new(
            input.Request.Perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.Parse(kindValue),
            "Respond to the exact supplied perspective.");
        RecordingModel model = new(response);
        using CancellationTokenSource cancellation = new();

        CharacterBeliefGoalIncompatibilityConsiderationResult result =
            await CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                model,
                input.Request,
                cancellation.Token);

        Assert.Equal(1, model.CallCount);
        Assert.Same(input.Request, model.Request);
        Assert.Equal(cancellation.Token, model.CancellationToken);
        Assert.Same(input.Request, result.Request);
        Assert.Same(response, result.Response);
        Assert.Same(response.Kind, CharacterBeliefGoalIncompatibilityResponseKind.Parse(kindValue));
    }

    [Fact]
    public async Task RejectsOtherAndValueEqualReplacementPerspectivesWithoutChangingCharacterState()
    {
        ControllerInput input = Input();
        CharacterBeliefGoalIncompatibilityPerspective otherPerspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(input.Declaration)[1];
        CharacterBeliefGoalIncompatibilityPerspective replacementPerspective = new(
            input.Declaration,
            input.Request.Perspective.SubjectCharacter);
        CharacterBelief[] beliefs = [.. input.Request.Perspective.SubjectCharacter.Beliefs.Entries];
        CharacterGoal[] goals = [.. input.Request.Perspective.SubjectCharacter.Goals.Entries];

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                new RecordingModel(Response(otherPerspective)),
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                new RecordingModel(Response(replacementPerspective)),
                input.Request,
                CancellationToken.None));

        Assert.NotSame(input.Request.Perspective, replacementPerspective);
        Assert.Equal(input.Request.Perspective, replacementPerspective);
        Assert.Same(input.Declaration, input.Request.Perspective.Declaration);
        Assert.Equal(beliefs, input.Request.Perspective.SubjectCharacter.Beliefs.Entries);
        Assert.Equal(goals, input.Request.Perspective.SubjectCharacter.Goals.Entries);
    }

    [Fact]
    public async Task RejectsNullOutputAndNullInputsBeforeAnyInvalidModelInvocation()
    {
        ControllerInput input = Input();
        RecordingModel model = new(Response(input.Request.Perspective));
        RecordingModel nullOutputModel = new(null!);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                nullOutputModel,
                input.Request,
                CancellationToken.None));

        Assert.Equal(0, model.CallCount);
        Assert.Equal(1, nullOutputModel.CallCount);
    }

    [Fact]
    public async Task RejectsPreCancellationWithoutInvokingTheModel()
    {
        ControllerInput input = Input();
        RecordingModel model = new(Response(input.Request.Perspective));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                model,
                input.Request,
                cancellation.Token));

        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public async Task PropagatesModelCancellationAndFailureWithoutChangingTheRequest()
    {
        ControllerInput input = Input();
        using CancellationTokenSource cancellation = new();
        CharacterBelief[] beliefs = [.. input.Request.Perspective.SubjectCharacter.Beliefs.Entries];
        CharacterGoal[] goals = [.. input.Request.Perspective.SubjectCharacter.Goals.Entries];

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                new CancellingModel(),
                input.Request,
                cancellation.Token));
        InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CharacterBeliefGoalIncompatibilityConsiderationController.RespondAsync(
                new FailingModel(),
                input.Request,
                cancellation.Token));

        Assert.Equal("model failure", failure.Message);
        Assert.Same(input.Declaration, input.Request.Perspective.Declaration);
        Assert.Equal(beliefs, input.Request.Perspective.SubjectCharacter.Beliefs.Entries);
        Assert.Equal(goals, input.Request.Perspective.SubjectCharacter.Goals.Entries);
    }

    [Fact]
    public void ExposesOnlyOneStaticResponseOperation()
    {
        Type controllerType = typeof(CharacterBeliefGoalIncompatibilityConsiderationController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("RespondAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<CharacterBeliefGoalIncompatibilityConsiderationResult>), method.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterBeliefGoalIncompatibilityResponseModel),
                typeof(CharacterBeliefGoalIncompatibilityConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.Empty(controllerType.GetEvents());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput Input()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibilityPerspective perspective =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration)[0];

        return new(declaration, new CharacterBeliefGoalIncompatibilityConsiderationRequest(perspective));
    }

    private static CharacterBeliefGoalIncompatibilityResponse Response(
        CharacterBeliefGoalIncompatibilityPerspective perspective) => new(
            perspective,
            CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation,
            "Seek a jointly supported response.");

    private sealed class RecordingModel(CharacterBeliefGoalIncompatibilityResponse response)
        : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public int CallCount { get; private set; }

        public CharacterBeliefGoalIncompatibilityConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(response);
        }
    }

    private sealed class CancellingModel : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterBeliefGoalIncompatibilityResponse>(
                new OperationCanceledException("model cancellation", cancellationToken));
    }

    private sealed class FailingModel : ICharacterBeliefGoalIncompatibilityResponseModel
    {
        public ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
            CharacterBeliefGoalIncompatibilityConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterBeliefGoalIncompatibilityResponse>(
                new InvalidOperationException("model failure"));
    }

    private sealed record ControllerInput(
        CharacterBeliefGoalIncompatibility Declaration,
        CharacterBeliefGoalIncompatibilityConsiderationRequest Request);
}
