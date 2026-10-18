using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Tests.Dialogue;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageDelayOpenAiStreamTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> accumulator = AccumulateCharacterTargetSabotageDelay(input.Request);
        string response = DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterTargetSabotageDelayDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.Candidate, decision?.SelectedCandidate);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(64, true)]
    public void CompletesWithBoundedSchemaEchoes(int depth, bool abstain)
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> accumulator = AccumulateCharacterTargetSabotageDelay(input.Request);
        string response = DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);

        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.created", depth)));
        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.in_progress", depth)));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterTargetSabotageDelayDecision? decision = accumulator.Apply(
            OpenAiSchemaEcho.Event("response.completed", depth));

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.Candidate, decision?.SelectedCandidate);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [MemberData(nameof(OpenAiSchemaEcho.InvalidEvents),
        MemberType = typeof(OpenAiSchemaEcho))]
    public void RejectsUnboundedOrNonStrictSchemaEchoes(string serializedEvent)
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> accumulator = AccumulateCharacterTargetSabotageDelay(input.Request);

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
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> accumulator = AccumulateCharacterTargetSabotageDelay(
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsDuplicateOutOfOrderInvalidAndOversizedOutput()
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> duplicate = AccumulateCharacterTargetSabotageDelay(input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> invalid = AccumulateCharacterTargetSabotageDelay(input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> excessive = AccumulateCharacterTargetSabotageDelay(input.Request);

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
            CharacterTargetSabotageDelayDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessive.Apply(Delta("x")));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndOversizedEvents()
    {
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision> accumulator = AccumulateCharacterTargetSabotageDelay(input.Request);
        Assert.Throws<ArgumentNullException>(() =>
            AccumulateCharacterTargetSabotageDelay(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply(new string(
            'x', OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageDelayConsiderationRequest, CharacterTargetSabotageDelayDecision>.MaximumEventLength + 1)));

    }

    internal static string DecisionDocument(string actorId, int? selectedCandidateIndex) =>
        System.Text.Json.JsonSerializer.Serialize(new { actorId, selectedCandidateIndex });

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
        CharacterTargetSabotageDelayDecisionJsonDeserializerTests.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.InteractionAccess, input.Request.InteractionAccess);
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }
}
