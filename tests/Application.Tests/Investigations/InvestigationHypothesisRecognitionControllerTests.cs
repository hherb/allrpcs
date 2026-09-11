using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class InvestigationHypothesisRecognitionControllerTests
{
    [Fact]
    public async Task CanonicalizesOneInterpreterProposalWithoutPromotingIt()
    {
        InvestigationHypothesisRecognitionRequest request = Request();
        InvestigationQuestion question = Assert.Single(request.OpenQuestions.Questions);
        InvestigationHypothesis hypothesis = Assert.Single(request.Hypotheses.Hypotheses);
        RecordingInterpreter interpreter = new(new InvestigationHypothesisProposal(
            new InvestigationQuestionId(question.QuestionId.Value),
            new InvestigationHypothesisId(hypothesis.HypothesisId.Value)));
        using CancellationTokenSource cancellation = new();

        InvestigationHypothesisRecognition? result =
            await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
                interpreter,
                request,
                cancellation.Token);

        InvestigationHypothesisRecognition recognition = Assert.IsType<
            InvestigationHypothesisRecognition>(result);
        Assert.Same(request, interpreter.Request);
        Assert.Equal(cancellation.Token, interpreter.CancellationToken);
        Assert.Same(request.SourceUtterance, recognition.SourceUtterance);
        Assert.Same(hypothesis, recognition.Hypothesis);
        Assert.Same(question, recognition.Hypothesis.Question);
        Assert.Same(question, Assert.Single(request.OpenQuestions.Questions));
        Assert.Same(hypothesis, Assert.Single(request.Hypotheses.Hypotheses));
    }

    [Fact]
    public async Task RejectsMissingAndUnauthoredProposalsWithoutCreatingState()
    {
        InvestigationHypothesisRecognitionRequest request = Request();
        RecordingInterpreter noMatch = new(null);
        RecordingInterpreter unknown = new(new InvestigationHypothesisProposal(
            new InvestigationQuestionId("unknown-question"),
            new InvestigationHypothesisId("unknown-hypothesis")));

        Assert.Null(await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
            noMatch,
            request,
            CancellationToken.None));
        Assert.Null(await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
            unknown,
            request,
            CancellationToken.None));
        Assert.Single(request.OpenQuestions.Questions);
        Assert.Single(request.Hypotheses.Hypotheses);
    }

    [Fact]
    public async Task PropagatesCancellationToTheInterpreter()
    {
        InvestigationHypothesisRecognitionRequest request = Request();
        CancellingInterpreter interpreter = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
                interpreter,
                request,
                cancellation.Token));

        Assert.Equal(cancellation.Token, interpreter.CancellationToken);
    }

    [Fact]
    public async Task RejectsNullsAndExposesOnlyTheBoundedAsyncOperation()
    {
        InvestigationHypothesisRecognitionRequest request = Request();
        RecordingInterpreter interpreter = new(null);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
                null!, request, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
                interpreter, null!, CancellationToken.None));

        Type controllerType = typeof(InvestigationHypothesisRecognitionController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("TryRecognizeAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<InvestigationHypothesisRecognition>), method.ReturnType);
        Assert.Equal(
            [
                typeof(IInvestigationHypothesisInterpreter),
                typeof(InvestigationHypothesisRecognitionRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetEvents());
    }

    [Fact]
    public void InterpreterExposesOnlyOneProviderNeutralOperation()
    {
        Type interpreterType = typeof(IInvestigationHypothesisInterpreter);
        System.Reflection.MethodInfo method = Assert.Single(interpreterType.GetMethods());

        Assert.Equal("InterpretAsync", method.Name);
        Assert.Equal(typeof(ValueTask<InvestigationHypothesisProposal>), method.ReturnType);
        Assert.Equal(
            [typeof(InvestigationHypothesisRecognitionRequest), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(interpreterType.GetProperties());
        Assert.Empty(interpreterType.GetEvents());
    }

    private static InvestigationHypothesisRecognitionRequest Request()
    {
        InvestigationQuestion question = InvestigationHypothesisRecognitionRequestTests.Question();
        InvestigationHypothesis hypothesis =
            InvestigationHypothesisRecognitionRequestTests.Hypothesis(question);
        return new InvestigationHypothesisRecognitionRequest(
            InvestigationHypothesisRecognitionRequestTests.Utterance(),
            new OpenInvestigationQuestionSnapshot([question]),
            new InvestigationHypothesisSnapshot([hypothesis]));
    }

    private sealed class RecordingInterpreter(InvestigationHypothesisProposal? proposal)
        : IInvestigationHypothesisInterpreter
    {
        internal InvestigationHypothesisRecognitionRequest? Request { get; private set; }

        internal CancellationToken CancellationToken { get; private set; }

        public ValueTask<InvestigationHypothesisProposal?> InterpretAsync(
            InvestigationHypothesisRecognitionRequest request,
            CancellationToken cancellationToken)
        {
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(proposal);
        }
    }

    private sealed class CancellingInterpreter : IInvestigationHypothesisInterpreter
    {
        internal CancellationToken CancellationToken { get; private set; }

        public ValueTask<InvestigationHypothesisProposal?> InterpretAsync(
            InvestigationHypothesisRecognitionRequest request,
            CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return ValueTask.FromCanceled<InvestigationHypothesisProposal?>(cancellationToken);
        }
    }
}
