using System.Text.Json;
using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Infrastructure.Investigations;

namespace Allrpcs.Infrastructure.Tests.Investigations;

public sealed class SerializedInvestigationHypothesisInterpreterTests
{
    [Fact]
    public async Task SerializesOnlyBoundedUntrustedInputAndCanonicalizesAValidMatch()
    {
        InvestigationHypothesisRecognitionRequest request = Request();
        InvestigationQuestion question = request.OpenQuestions.Questions[1];
        InvestigationHypothesis hypothesis = request.Hypotheses.Hypotheses[1];
        RecordingTransport transport = new(
            "{\"status\":\"match\",\"questionId\":\"second-question\","
                + "\"hypothesisId\":\"second-hypothesis\"}");
        SerializedInvestigationHypothesisInterpreter interpreter = new(transport);
        using CancellationTokenSource cancellation = new();

        InvestigationHypothesisRecognition? result =
            await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
                interpreter,
                request,
                cancellation.Token);

        InvestigationHypothesisRecognition recognition = Assert.IsType<
            InvestigationHypothesisRecognition>(result);
        Assert.Same(request.SourceUtterance, recognition.SourceUtterance);
        Assert.Same(hypothesis, recognition.Hypothesis);
        Assert.Same(question, recognition.Hypothesis.Question);
        Assert.Equal(cancellation.Token, transport.CancellationToken);
        AssertRequestDocument(transport.RequestDocument!);
        Assert.Same(question, request.OpenQuestions.Questions[1]);
        Assert.Same(hypothesis, request.Hypotheses.Hypotheses[1]);
    }

    [Fact]
    public async Task CarriesExplicitNoMatchAndUnknownIdentitiesThroughTheController()
    {
        InvestigationHypothesisRecognitionRequest request = Request();
        SerializedInvestigationHypothesisInterpreter noMatch = new(
            new RecordingTransport("{\"status\":\"no-match\"}"));
        SerializedInvestigationHypothesisInterpreter unknownQuestion = new(
            new RecordingTransport(
                "{\"status\":\"match\",\"questionId\":\"unknown-question\","
                    + "\"hypothesisId\":\"first-hypothesis\"}"));
        SerializedInvestigationHypothesisInterpreter unknownHypothesis = new(
            new RecordingTransport(
                "{\"status\":\"match\",\"questionId\":\"first-question\","
                    + "\"hypothesisId\":\"unknown-hypothesis\"}"));

        Assert.Null(await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
            noMatch,
            request,
            CancellationToken.None));
        Assert.Null(await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
            unknownQuestion,
            request,
            CancellationToken.None));
        Assert.Null(await InvestigationHypothesisRecognitionController.TryRecognizeAsync(
            unknownHypothesis,
            request,
            CancellationToken.None));
        Assert.Equal(2, request.OpenQuestions.Questions.Count);
        Assert.Equal(3, request.Hypotheses.Hypotheses.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"status\":\"unknown\"}")]
    [InlineData("{\"status\":\"no-match\",\"questionId\":\"first-question\"}")]
    [InlineData("{\"status\":\"no-match\",\"status\":\"no-match\"}")]
    [InlineData("{\"status\":\"match\"}")]
    [InlineData("{\"status\":\"match\",\"questionId\":\"first-question\"}")]
    [InlineData(
        "{\"status\":\"match\",\"questionId\":\"first-question\","
            + "\"hypothesisId\":\"first-hypothesis\",\"extra\":null}")]
    [InlineData(
        "{\"status\":\"match\",\"questionId\":\"first-question\","
            + "\"questionId\":\"first-question\",\"hypothesisId\":\"first-hypothesis\"}")]
    [InlineData("[]")]
    [InlineData("null")]
    public async Task RejectsMalformedExtraDuplicateUnknownShapeAndPartialOutput(
        string responseDocument)
    {
        SerializedInvestigationHypothesisInterpreter interpreter = new(
            new RecordingTransport(responseDocument));

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await interpreter.InterpretAsync(Request(), CancellationToken.None));

        if (responseDocument.Length > 0)
        {
            Assert.DoesNotContain(responseDocument, exception.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task RejectsOversizedOutputWithoutRetainingIt()
    {
        string responseDocument = new(
            'x',
            SerializedInvestigationHypothesisInterpreter.MaximumResponseDocumentLength + 1);
        SerializedInvestigationHypothesisInterpreter interpreter = new(
            new RecordingTransport(responseDocument));

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await interpreter.InterpretAsync(Request(), CancellationToken.None));

        Assert.DoesNotContain(responseDocument, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropagatesCancellationAcrossTheSuppliedTransportSeam()
    {
        CancellingTransport transport = new();
        SerializedInvestigationHypothesisInterpreter interpreter = new(transport);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await interpreter.InterpretAsync(Request(), cancellation.Token));

        Assert.Equal(cancellation.Token, transport.CancellationToken);
    }

    [Fact]
    public void RequiresOnlyOneCallerSuppliedTransportAndPerformsNoConstructionIo()
    {
        RecordingTransport transport = new("{\"status\":\"no-match\"}");

        SerializedInvestigationHypothesisInterpreter interpreter = new(transport);

        Assert.Equal(0, transport.CallCount);
        Type interpreterType = interpreter.GetType();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            interpreterType.GetConstructors());
        Assert.Equal(
            [typeof(IInvestigationHypothesisTransport)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Throws<ArgumentNullException>(() =>
            new SerializedInvestigationHypothesisInterpreter(null!));

        Type transportType = typeof(IInvestigationHypothesisTransport);
        System.Reflection.MethodInfo method = Assert.Single(transportType.GetMethods());
        Assert.Equal("CompleteAsync", method.Name);
        Assert.Equal(typeof(ValueTask<string>), method.ReturnType);
        Assert.Equal(
            [typeof(string), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transportType.GetProperties());
        Assert.Empty(transportType.GetEvents());
    }

    private static void AssertRequestDocument(string serialized)
    {
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal(
            ["sourceUtterance", "openQuestions", "hypotheses"],
            root.EnumerateObject().Select(property => property.Name).ToArray());
        JsonElement source = root.GetProperty("sourceUtterance");
        Assert.Equal(
            ["trust", "text"],
            source.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal("untrusted-in-world-text", source.GetProperty("trust").GetString());
        Assert.Equal(
            "Could cooling the tonic preserve what helps?",
            source.GetProperty("text").GetString());

        JsonElement[] questions = root.GetProperty("openQuestions").EnumerateArray().ToArray();
        Assert.Equal(2, questions.Length);
        Assert.Equal(
            ["questionId", "statement"],
            questions[0].EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal("first-question", questions[0].GetProperty("questionId").GetString());
        Assert.Equal("What is the cause?", questions[0].GetProperty("statement").GetString());
        Assert.Equal("second-question", questions[1].GetProperty("questionId").GetString());

        JsonElement[] hypotheses = root.GetProperty("hypotheses").EnumerateArray().ToArray();
        Assert.Equal(3, hypotheses.Length);
        Assert.Equal(
            ["hypothesisId", "questionId", "statement"],
            hypotheses[0].EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal("first-hypothesis", hypotheses[0].GetProperty("hypothesisId").GetString());
        Assert.Equal("first-question", hypotheses[0].GetProperty("questionId").GetString());
        Assert.Equal("It may be exposure.", hypotheses[0].GetProperty("statement").GetString());
        Assert.Equal("second-hypothesis", hypotheses[1].GetProperty("hypothesisId").GetString());
        Assert.Equal("third-hypothesis", hypotheses[2].GetProperty("hypothesisId").GetString());
        Assert.DoesNotContain("seeker", serialized, StringComparison.Ordinal);
    }

    private static InvestigationHypothesisRecognitionRequest Request()
    {
        InvestigationQuestion firstQuestion = new(
            new InvestigationQuestionId("first-question"),
            "What is the cause?");
        InvestigationQuestion secondQuestion = new(
            new InvestigationQuestionId("second-question"),
            "What treatment helps?");
        InvestigationHypothesis firstHypothesis = new(
            new InvestigationHypothesisId("first-hypothesis"),
            firstQuestion,
            "It may be exposure.");
        InvestigationHypothesis secondHypothesis = new(
            new InvestigationHypothesisId("second-hypothesis"),
            secondQuestion,
            "Cooling may preserve the remedy.");
        InvestigationHypothesis thirdHypothesis = new(
            new InvestigationHypothesisId("third-hypothesis"),
            firstQuestion,
            "It may be contaminated water.");

        return new InvestigationHypothesisRecognitionRequest(
            new DialogueUtterance(
                new CharacterId("seeker"),
                "Could cooling the tonic preserve what helps?"),
            new OpenInvestigationQuestionSnapshot([firstQuestion, secondQuestion]),
            new InvestigationHypothesisSnapshot(
                [firstHypothesis, secondHypothesis, thirdHypothesis]));
    }

    private sealed class RecordingTransport(string responseDocument)
        : IInvestigationHypothesisTransport
    {
        internal int CallCount { get; private set; }

        internal CancellationToken CancellationToken { get; private set; }

        internal string? RequestDocument { get; private set; }

        public ValueTask<string> CompleteAsync(
            string requestDocument,
            CancellationToken cancellationToken)
        {
            CallCount++;
            RequestDocument = requestDocument;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(responseDocument);
        }
    }

    private sealed class CancellingTransport : IInvestigationHypothesisTransport
    {
        internal CancellationToken CancellationToken { get; private set; }

        public ValueTask<string> CompleteAsync(
            string requestDocument,
            CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            return ValueTask.FromCanceled<string>(cancellationToken);
        }
    }
}
