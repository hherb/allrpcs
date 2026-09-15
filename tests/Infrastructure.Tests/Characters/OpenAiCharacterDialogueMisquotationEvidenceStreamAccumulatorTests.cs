using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulatorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        Input input = CreateInput();
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator accumulator = new(
            input.Request);
        int? selectedEvidenceIndex = abstain ? null : 0;
        string response = DecisionDocument(input.Actor.Identity.Id.Value, selectedEvidenceIndex);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterDialogueMisquotationEvidenceDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        if (abstain)
        {
            Assert.Null(decision?.SelectedEvidence);
        }
        else
        {
            Assert.Same(input.Evidence, decision?.SelectedEvidence);
        }

        AssertRequestUnchanged(input);
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
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator accumulator = new(
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
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator duplicateStart = new(
            input.Request);
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator outOfOrder = new(
            input.Request);

        Assert.Null(duplicateStart.Apply("{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => duplicateStart.Apply(
            "{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => outOfOrder.Apply(Delta("{}")));
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator metadataBeforeStart = new(
            input.Request);
        Assert.Throws<InvalidDataException>(() => metadataBeforeStart.Apply(
            "{\"type\":\"response.output_item.added\"}"));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsInvalidDuplicateAndOversizedOutputBeforeCompleting()
    {
        Input input = CreateInput();
        string response = DecisionDocument(input.Actor.Identity.Id.Value, null);
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator invalid = new(input.Request);
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator duplicate = new(input.Request);
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator excessive = new(input.Request);

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
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessive.Apply(Delta("x")));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndOversizedEventsAndExposesOnlyTheBoundedAccumulatorSurface()
    {
        Input input = CreateInput();
        OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator accumulator = new(
            input.Request);
        Type accumulatorType = typeof(
            OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator);
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            accumulatorType.GetConstructors());
        System.Reflection.MethodInfo method = Assert.Single(
            accumulatorType.GetMethods().Where(
                candidate => candidate.DeclaringType == accumulatorType));
        System.Reflection.FieldInfo field = Assert.Single(
            accumulatorType.GetFields(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply(new string(
            'x',
            OpenAiCharacterDialogueMisquotationEvidenceStreamAccumulator.MaximumEventLength + 1)));
        Assert.Equal(
            [typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterDialogueMisquotationEvidenceDecision), method.ReturnType);
        Assert.Equal(
            [typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumEventLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(accumulatorType.GetProperties());
        Assert.Empty(accumulatorType.GetEvents());
        Assert.True(accumulatorType.IsSealed);
        AssertRequestUnchanged(input);
    }

    internal static string DecisionDocument(string actorId, int? selectedEvidenceIndex) =>
        System.Text.Json.JsonSerializer.Serialize(new { actorId, selectedEvidenceIndex });

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
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Evidence, Assert.Single(input.Request.Candidates));
        Assert.Same(input.Evidence.Occurrence.Proposal.SourceEvent, input.SourceEvent);
        Assert.Same(input.Evidence.Occurrence.DeliveredEvent, input.DeliveredEvent);
    }

    private static Input CreateInput()
    {
        CharacterDialogueMisquotationEvidenceDecisionJsonDeserializerTests.Input input =
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializerTests.CreateInput();
        return new(
            input.Actor,
            input.SourceEvent,
            input.DeliveredEvent,
            input.Evidence,
            input.Request);
    }

    private sealed record Input(
        Character Actor,
        Domain.Dialogue.DialogueEvent SourceEvent,
        Domain.Dialogue.DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}
