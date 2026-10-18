using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Tests.Dialogue;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryOpenAiStreamTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> accumulator = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);
        string response = DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.output_item.added\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterTargetSabotageInfluenceRecoveryDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.Candidates[0], decision?.SelectedCandidate);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(64, true)]
    public void CompletesWithBoundedSchemaEchoes(int depth, bool abstain)
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> accumulator = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);
        string response = DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);

        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.created", depth)));
        Assert.Null(accumulator.Apply(OpenAiSchemaEcho.Event("response.in_progress", depth)));
        Assert.Null(accumulator.Apply("{\"type\":\"response.output_item.added\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterTargetSabotageInfluenceRecoveryDecision? decision = accumulator.Apply(
            OpenAiSchemaEcho.Event("response.completed", depth));

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.Candidates[0], decision?.SelectedCandidate);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [MemberData(nameof(OpenAiSchemaEcho.InvalidEvents),
        MemberType = typeof(OpenAiSchemaEcho))]
    public void RejectsUnboundedOrNonStrictSchemaEchoes(string serializedEvent)
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> accumulator = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);

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
    [InlineData("{\"type\":\"response.refusal.done\",\"refusal\":\"secret\"}")]
    [InlineData("{\"type\":\"response.failed\",\"detail\":\"secret\"}")]
    public void RejectsMalformedPartialUnexpectedRefusedAndFailedEvents(string serializedEvent)
    {
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> accumulator = AccumulateCharacterTargetSabotageInfluenceRecovery(
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create().Request);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsDuplicateOutOfOrderInvalidAndOversizedOutput()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        string response = DecisionDocument(input.Actor.Identity.Id.Value, null);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> duplicateStart = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> outOfOrder = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> invalid = AccumulateCharacterTargetSabotageInfluenceRecovery(input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> duplicateDone = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> excessive = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);

        Assert.Null(duplicateStart.Apply("{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => duplicateStart.Apply(
            "{\"type\":\"response.created\"}"));
        Assert.Throws<InvalidDataException>(() => outOfOrder.Apply(Delta("{}")));
        Begin(invalid);
        Assert.Throws<InvalidDataException>(() => invalid.Apply(Done(
            DecisionDocument("unknown-character", null))));
        Begin(duplicateDone);
        Assert.Null(duplicateDone.Apply(Done(response)));
        Assert.Throws<InvalidDataException>(() => duplicateDone.Apply(Done(response)));
        Begin(excessive);
        Assert.Null(excessive.Apply(Delta(new string(
            'x',
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessive.Apply(Delta("x")));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndOversizedEvents()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> accumulator = AccumulateCharacterTargetSabotageInfluenceRecovery(
            input.Request);
        Assert.Throws<ArgumentNullException>(() =>
            AccumulateCharacterTargetSabotageInfluenceRecovery(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply(new string(
            'x',
            OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision>.MaximumEventLength + 1)));

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

    private static void Begin(
        OpenAiStructuredDecisionAccumulator<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest, CharacterTargetSabotageInfluenceRecoveryDecision> accumulator)
    {
        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
    }

    private static void AssertRequestUnchanged(
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Same(input.Candidates[0], Assert.Single(input.Request.Candidates));
        Assert.Same(input.InfluenceResult, input.Candidates[0].InfluenceResult);
        Assert.Same(input.RecoveryChange.Reason, input.Candidates[0].RecoveryChange.Reason);
    }
}
