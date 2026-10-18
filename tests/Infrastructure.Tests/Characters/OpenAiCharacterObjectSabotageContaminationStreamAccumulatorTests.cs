using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Tests.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageContaminationStreamAccumulatorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        Input input = CreateInput();
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator accumulator = new(input.Request);
        string? selectedEvidenceId = abstain ? null : input.Candidate.Evidence.EvidenceId.Value;
        string response = DecisionDocument(input.Character.Identity.Id.Value, selectedEvidenceId);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterObjectSabotageContaminationDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(input.Character.Identity.Id, decision?.CharacterId);
        if (abstain)
        {
            Assert.Null(decision?.SelectedEvidenceId);
        }
        else
        {
            Assert.Same(input.Candidate.Evidence.EvidenceId, decision?.SelectedEvidenceId);
        }

        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(64, true)]
    public void CompletesWithBoundedSchemaEchoes(int depth, bool abstain)
    {
        Input input = CreateInput();
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator accumulator = new(input.Request);
        string? selectedEvidenceId = abstain ? null : input.Candidate.Evidence.EvidenceId.Value;
        string response = DecisionDocument(input.Character.Identity.Id.Value, selectedEvidenceId);

        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.created", depth)));
        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.in_progress", depth)));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterObjectSabotageContaminationDecision? decision = accumulator.Apply(
            OpenAiSchemaEcho.Event("response.completed", depth));

        Assert.Same(input.Character.Identity.Id, decision?.CharacterId);
        if (abstain)
        {
            Assert.Null(decision?.SelectedEvidenceId);
        }
        else
        {
            Assert.Same(input.Candidate.Evidence.EvidenceId, decision?.SelectedEvidenceId);
        }

        AssertRequestUnchanged(input);
    }

    [Theory]
    [MemberData(nameof(OpenAiSchemaEcho.InvalidEvents),
        MemberType = typeof(OpenAiSchemaEcho))]
    public void RejectsUnboundedOrNonStrictSchemaEchoes(string serializedEvent)
    {
        Input input = CreateInput();
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator accumulator = new(input.Request);

        Assert.Throws<InvalidDataException>(() => accumulator.Apply(serializedEvent));
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"unknown.event\"}")]
    [InlineData("{\"type\":\"response.output_text.delta\"}")]
    [InlineData("{\"type\":\"response.output_text.done\",\"text\":5}")]
    [InlineData("{\"type\":\"response.completed\"}")]
    [InlineData("{\"type\":\"response.incomplete\"}")]
    [InlineData("{\"type\":\"response.failed\",\"detail\":\"secret\"}")]
    public void RejectsMalformedPartialUnexpectedAndFailedEvents(string serializedEvent)
    {
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator accumulator = new(
            CreateInput().Request);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsDuplicateAndOutOfOrderLifecycleEvents()
    {
        Input input = CreateInput();
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator duplicateStart = new(input.Request);
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator outOfOrder = new(input.Request);

        Assert.Null(duplicateStart.Apply("{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => duplicateStart.Apply(
            "{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => outOfOrder.Apply(Delta("{}")));
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator metadataBeforeStart = new(
            input.Request);
        Assert.Throws<InvalidDataException>(() => metadataBeforeStart.Apply(
            "{\"type\":\"response.output_item.added\"}"));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsInvalidDuplicateAndOversizedOutputBeforeCompleting()
    {
        Input input = CreateInput();
        string response = DecisionDocument(input.Character.Identity.Id.Value, null);
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator invalid = new(input.Request);
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator duplicate = new(input.Request);
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator excessive = new(input.Request);

        Assert.Null(invalid.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(invalid.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Throws<InvalidDataException>(() => invalid.Apply(Done(
            DecisionDocument("unknown-character", null))));
        Assert.Null(duplicate.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(duplicate.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(duplicate.Apply(Done(response)));
        Assert.Throws<InvalidDataException>(() => duplicate.Apply(Done(response)));
        Assert.Null(excessive.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(excessive.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(excessive.Apply(Delta(new string(
            'x',
            CharacterObjectSabotageContaminationDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessive.Apply(Delta("x")));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndOversizedEventsAndExposesOnlyTheBoundedAccumulatorSurface()
    {
        Input input = CreateInput();
        OpenAiCharacterObjectSabotageContaminationStreamAccumulator accumulator = new(input.Request);
        Type accumulatorType = typeof(OpenAiCharacterObjectSabotageContaminationStreamAccumulator);
        System.Reflection.ConstructorInfo constructor = Assert.Single(accumulatorType.GetConstructors());
        System.Reflection.MethodInfo method = Assert.Single(
            accumulatorType.GetMethods().Where(candidate => candidate.DeclaringType == accumulatorType));
        System.Reflection.FieldInfo field = Assert.Single(accumulatorType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterObjectSabotageContaminationStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply(new string(
            'x',
            OpenAiCharacterObjectSabotageContaminationStreamAccumulator.MaximumEventLength + 1)));
        Assert.Equal([typeof(CharacterObjectSabotageContaminationConsiderationRequest)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterObjectSabotageContaminationDecision), method.ReturnType);
        Assert.Equal([typeof(string)], method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumEventLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(accumulatorType.GetProperties());
        Assert.Empty(accumulatorType.GetEvents());
        Assert.True(accumulatorType.IsSealed);
        AssertRequestUnchanged(input);
    }

    internal static string DecisionDocument(string characterId, string? selectedEvidenceId) =>
        System.Text.Json.JsonSerializer.Serialize(new { characterId, selectedEvidenceId });

    internal static string Delta(string value) => System.Text.Json.JsonSerializer.Serialize(new
    {
        type = "response.output_text.delta",
        delta = value,
    });

    internal static string Done(string value) => System.Text.Json.JsonSerializer.Serialize(new
    {
        type = "response.output_text.done",
        text = value,
    });

    private static void AssertRequestUnchanged(Input input)
    {
        Assert.Same(input.Character, input.Request.Character);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    private static Input CreateInput()
    {
        CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.Input input =
            CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.CreateInput();
        return new(input.Character, input.Candidate, input.Request);
    }

    private sealed record Input(
        Character Character,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
}
