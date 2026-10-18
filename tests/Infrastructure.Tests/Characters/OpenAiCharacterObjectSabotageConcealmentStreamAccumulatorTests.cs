using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Tests.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageConcealmentStreamAccumulatorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        CharacterObjectSabotageConcealmentConsiderationRequest request =
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator accumulator = new(request);
        string? selectedEvidenceId = abstain
            ? null
            : Assert.Single(request.Candidates).Evidence.EvidenceId.Value;
        string response = DecisionDocument(request.Character.Identity.Id.Value, selectedEvidenceId);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterObjectSabotageConcealmentDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(request.Character.Identity.Id, decision?.CharacterId);
        if (abstain)
        {
            Assert.Null(decision?.SelectedEvidenceId);
        }
        else
        {
            Assert.Same(
                Assert.Single(request.Candidates).Evidence.EvidenceId,
                decision?.SelectedEvidenceId);
        }
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(64, true)]
    public void CompletesWithBoundedSchemaEchoes(int depth, bool abstain)
    {
        CharacterObjectSabotageConcealmentConsiderationRequest request =
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator accumulator = new(request);
        string? selectedEvidenceId = abstain
            ? null
            : Assert.Single(request.Candidates).Evidence.EvidenceId.Value;
        string response = DecisionDocument(request.Character.Identity.Id.Value, selectedEvidenceId);

        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.created", depth)));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterObjectSabotageConcealmentDecision? decision = accumulator.Apply(
            OpenAiSchemaEcho.Event("response.completed", depth));

        Assert.Same(request.Character.Identity.Id, decision?.CharacterId);
        if (abstain)
        {
            Assert.Null(decision?.SelectedEvidenceId);
        }
        else
        {
            Assert.Same(
                Assert.Single(request.Candidates).Evidence.EvidenceId,
                decision?.SelectedEvidenceId);
        }
    }

    [Theory]
    [MemberData(nameof(OpenAiSchemaEcho.InvalidEvents),
        MemberType = typeof(OpenAiSchemaEcho))]
    public void RejectsUnboundedOrNonStrictSchemaEchoes(string serializedEvent)
    {
        CharacterObjectSabotageConcealmentConsiderationRequest request =
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator accumulator = new(request);

        Assert.Throws<InvalidDataException>(() => accumulator.Apply(serializedEvent));
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void IgnoresSupportedMetadataAndDoesNotRetainProviderTextOrIdentifiers()
    {
        CharacterObjectSabotageConcealmentConsiderationRequest request =
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator accumulator = new(request);
        string response = DecisionDocument(request.Character.Identity.Id.Value, null);
        string[] metadataEvents =
        [
            "response.created",
            "response.in_progress",
            "response.output_item.added",
            "response.output_item.done",
            "response.content_part.added",
            "response.content_part.done",
            "response.reasoning_summary_part.added",
            "response.reasoning_summary_part.done",
            "response.reasoning_summary_text.delta",
            "response.reasoning_summary_text.done",
        ];

        foreach (string eventType in metadataEvents)
        {
            Assert.Null(accumulator.Apply(
                $"{{\"type\":\"{eventType}\",\"secret\":\"provider-value\"}}"));
        }

        Assert.Null(accumulator.Apply(Delta("non-authoritative transport rendering")));
        Assert.Null(accumulator.Apply(Done(response)));
        Assert.NotNull(accumulator.Apply(
            "{\"type\":\"response.completed\",\"response\":{\"id\":\"secret-id\"}}"));
        Assert.DoesNotContain("provider-value", accumulator.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("secret-id", accumulator.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(response, accumulator.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"unknown.event\"}")]
    [InlineData("{\"type\":\"response.output_text.delta\"}")]
    [InlineData("{\"type\":\"response.output_text.delta\",\"delta\":5}")]
    [InlineData("{\"type\":\"response.output_text.done\"}")]
    [InlineData("{\"type\":\"response.output_text.done\",\"text\":5}")]
    [InlineData("{\"type\":\"response.completed\"}")]
    [InlineData("{\"type\":\"response.incomplete\"}")]
    [InlineData("{\"type\":\"response.refusal.done\",\"refusal\":\"secret\"}")]
    [InlineData("{\"type\":\"response.failed\",\"detail\":\"secret\"}")]
    [InlineData("{\"type\":\"error\",\"detail\":\"secret\"}")]
    public void RejectsMalformedUnexpectedIncompleteAndProviderFailureEvents(string serializedEvent)
    {
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator accumulator = new(
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest());

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsInvalidDecisionDuplicateDoneAndEventsAfterTerminalState()
    {
        CharacterObjectSabotageConcealmentConsiderationRequest request =
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
        string response = DecisionDocument(request.Character.Identity.Id.Value, null);
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator invalid = new(request);
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator duplicate = new(request);
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator completed = new(request);

        Assert.Throws<InvalidDataException>(() => invalid.Apply(Done(
            DecisionDocument("unknown-character", null))));
        Assert.Throws<InvalidOperationException>(() => invalid.Apply(
            "{\"type\":\"response.created\"}"));

        Assert.Null(duplicate.Apply(Done(response)));
        Assert.Throws<InvalidDataException>(() => duplicate.Apply(Done(response)));

        Assert.Null(completed.Apply(Done(response)));
        Assert.NotNull(completed.Apply("{\"type\":\"response.completed\"}"));
        Assert.Throws<InvalidOperationException>(() => completed.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsBoundViolationsAndNulls()
    {
        CharacterObjectSabotageConcealmentConsiderationRequest request =
            OpenAiCharacterObjectSabotageConcealmentRequestSerializerTests.CreateRequest();
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator excessiveEvent = new(request);
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator excessiveDeltas = new(request);
        OpenAiCharacterObjectSabotageConcealmentStreamAccumulator excessiveDone = new(request);

        Assert.Throws<InvalidDataException>(() => excessiveEvent.Apply(new string(
            'x',
            OpenAiCharacterObjectSabotageConcealmentStreamAccumulator.MaximumEventLength + 1)));
        Assert.Null(excessiveDeltas.Apply(Delta(new string(
            'x',
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessiveDeltas.Apply(Delta("x")));
        Assert.Throws<InvalidDataException>(() => excessiveDone.Apply(Done(new string(
            'x',
            CharacterObjectSabotageConcealmentDecisionJsonDeserializer.MaximumDocumentLength + 1))));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterObjectSabotageConcealmentStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => excessiveEvent.Apply(null!));
    }

    [Fact]
    public void ExposesOnlyTheBoundedAccumulatorSurface()
    {
        Type accumulatorType = typeof(OpenAiCharacterObjectSabotageConcealmentStreamAccumulator);
        System.Reflection.ConstructorInfo constructor = Assert.Single(accumulatorType.GetConstructors());
        System.Reflection.MethodInfo method = Assert.Single(
            accumulatorType.GetMethods().Where(candidate => candidate.DeclaringType == accumulatorType));
        System.Reflection.FieldInfo field = Assert.Single(accumulatorType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(
            [typeof(CharacterObjectSabotageConcealmentConsiderationRequest)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterObjectSabotageConcealmentDecision), method.ReturnType);
        Assert.Equal([typeof(string)], method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumEventLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(accumulatorType.GetProperties());
        Assert.Empty(accumulatorType.GetEvents());
        Assert.True(accumulatorType.IsSealed);
    }

    internal static string DecisionDocument(string characterId, string? selectedEvidenceId) =>
        System.Text.Json.JsonSerializer.Serialize(new { characterId, selectedEvidenceId });

    internal static string Delta(string value) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "response.output_text.delta",
            delta = value,
        });

    internal static string Done(string value) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            type = "response.output_text.done",
            text = value,
        });
}
