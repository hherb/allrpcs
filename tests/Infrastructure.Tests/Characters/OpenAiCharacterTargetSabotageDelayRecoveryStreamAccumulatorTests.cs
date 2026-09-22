using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulatorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletesAnExactAcceptedSelectionOrAbstention(bool abstain)
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator accumulator = new(
            input.Request);
        string response = DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.output_item.added\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));
        CharacterTargetSabotageDelayRecoveryDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(input.Actor.Identity.Id, decision?.ActorId);
        Assert.Same(abstain ? null : input.Candidates[0], decision?.SelectedCandidate);
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
    [InlineData("{\"type\":\"response.refusal.done\",\"refusal\":\"secret\"}")]
    [InlineData("{\"type\":\"response.failed\",\"detail\":\"secret\"}")]
    public void RejectsMalformedPartialUnexpectedRefusedAndFailedEvents(string serializedEvent)
    {
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator accumulator = new(
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create().Request);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsDuplicateOutOfOrderInvalidAndOversizedOutput()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        string response = DecisionDocument(input.Actor.Identity.Id.Value, null);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator duplicateStart = new(
            input.Request);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator outOfOrder = new(
            input.Request);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator invalid = new(input.Request);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator duplicateDone = new(
            input.Request);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator metadataAfterDone = new(
            input.Request);
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator excessive = new(
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
        Begin(metadataAfterDone);
        Assert.Null(metadataAfterDone.Apply(Done(response)));
        Assert.Throws<InvalidDataException>(() => metadataAfterDone.Apply(
            "{\"type\":\"response.output_item.done\"}"));
        Begin(excessive);
        Assert.Null(excessive.Apply(Delta(new string(
            'x',
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessive.Apply(Delta("x")));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndOversizedEventsAndExposesOnlyTheBoundedAccumulatorSurface()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator accumulator = new(
            input.Request);
        Type accumulatorType = typeof(
            OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator);
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            accumulatorType.GetConstructors());
        System.Reflection.MethodInfo method = Assert.Single(
            accumulatorType.GetMethods().Where(candidate => candidate.DeclaringType == accumulatorType));
        System.Reflection.FieldInfo field = Assert.Single(accumulatorType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => accumulator.Apply(null!));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply(new string(
            'x',
            OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator.MaximumEventLength + 1)));
        Assert.Equal(
            [typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageDelayRecoveryDecision), method.ReturnType);
        Assert.Equal([typeof(string)], method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumEventLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(accumulatorType.GetProperties());
        Assert.Empty(accumulatorType.GetEvents());
        Assert.True(accumulatorType.IsSealed);
        AssertRequestUnchanged(input);
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
        OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulator accumulator)
    {
        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply("{\"type\":\"response.in_progress\"}"));
    }

    private static void AssertRequestUnchanged(
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.Candidates[0], Assert.Single(input.Request.Candidates));
        Assert.Same(input.DelayResult, input.Candidates[0].DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidates[0].RecoveryEvent);
    }
}
