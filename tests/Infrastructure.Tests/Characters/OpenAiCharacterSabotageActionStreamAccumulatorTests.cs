using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Tests.Dialogue;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterSabotageActionStreamAccumulatorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> accumulator = AccumulateCharacterSabotageAction(input.Request);
        string response = DecisionDocument(
            input.Actor.Identity.Id.Value,
            abstain ? null : input.SelectedKind.Value);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterSabotageActionDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.SelectedKind, decision?.SelectedActionKind);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(64, true)]
    public void CompletesWithBoundedSchemaEchoes(int depth, bool abstain)
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> accumulator = AccumulateCharacterSabotageAction(input.Request);
        string response = DecisionDocument(
            input.Actor.Identity.Id.Value,
            abstain ? null : input.SelectedKind.Value);

        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.created", depth)));
        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.in_progress", depth)));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterSabotageActionDecision? decision = accumulator.Apply(
            OpenAiSchemaEcho.Event("response.completed", depth));

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.SelectedKind, decision?.SelectedActionKind);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [MemberData(nameof(OpenAiSchemaEcho.InvalidEvents),
        MemberType = typeof(OpenAiSchemaEcho))]
    public void RejectsUnboundedOrNonStrictSchemaEchoes(string serializedEvent)
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> accumulator = AccumulateCharacterSabotageAction(input.Request);

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
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> accumulator = AccumulateCharacterSabotageAction(
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput().Request);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsDuplicateOutOfOrderInvalidAndOversizedOutput()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> duplicate = AccumulateCharacterSabotageAction(input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> invalid = AccumulateCharacterSabotageAction(input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> excessive = AccumulateCharacterSabotageAction(input.Request);

        Assert.Null(duplicate.Apply("{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => duplicate.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(invalid.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(invalid.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Throws<InvalidDataException>(() => invalid.Apply(Done(
            DecisionDocument("unknown-character", null))));
        Assert.Null(excessive.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(excessive.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(excessive.Apply(Delta(new string(
            'x',
            CharacterSabotageActionDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessive.Apply(Delta("x")));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndOversizedEvents()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> accumulator = AccumulateCharacterSabotageAction(input.Request);
        Assert.Throws<ArgumentNullException>(() =>
            AccumulateCharacterSabotageAction(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply(new string(
            'x', OpenAiStructuredDecisionAccumulator<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision>.MaximumEventLength + 1)));

    }

    internal static string DecisionDocument(string actorId, string? selectedActionKind) =>
        System.Text.Json.JsonSerializer.Serialize(new { actorId, selectedActionKind });

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

    private static void AssertRequestUnchanged(
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.FirstKind, input.Request.ActionKinds[0]);
        Assert.Same(input.SelectedKind, input.Request.ActionKinds[1]);
    }
}
