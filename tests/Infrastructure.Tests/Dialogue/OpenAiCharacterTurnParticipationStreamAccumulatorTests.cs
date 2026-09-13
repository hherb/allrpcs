using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterTurnParticipationStreamAccumulatorTests
{
    [Theory]
    [InlineData(true, "respond", null)]
    [InlineData(false, "interject", "compare-evidence")]
    [InlineData(false, "remain-silent", "compare-evidence")]
    public void CompletesAnExactAcceptedDecision(
        bool isDirectlyAddressed,
        string kind,
        string? motiveKey)
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(isDirectlyAddressed);
        OpenAiCharacterTurnParticipationStreamAccumulator accumulator = new(request);
        string response = DecisionDocument(kind, motiveKey);

        Assert.Null(accumulator.Apply("{\"type\":\"response.created\"}"));
        Assert.Null(accumulator.Apply(Delta(response)));
        Assert.Null(accumulator.Apply(Done(response)));

        CharacterTurnParticipationDecision? decision = accumulator.Apply(
            "{\"type\":\"response.completed\"}");

        Assert.Same(request.ScheduledCharacter.Identity.Id, decision?.ScheduledCharacterId);
        Assert.Same(CharacterTurnParticipationKind.Parse(kind), decision?.Kind);
        if (motiveKey is null)
        {
            Assert.Null(decision?.Motive);
        }
        else
        {
            Assert.Same(Assert.Single(request.ScheduledCharacter.Goals.Entries), decision?.Motive);
        }
    }

    [Fact]
    public void IgnoresSupportedMetadataAndDoesNotRetainProviderTextOrIdentifiers()
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(true);
        OpenAiCharacterTurnParticipationStreamAccumulator accumulator = new(request);
        string response = DecisionDocument("respond", null);

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
        OpenAiCharacterTurnParticipationStreamAccumulator accumulator = new(CreateRequest(true));

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => accumulator.Apply(serializedEvent));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsDuplicateDoneEventsAndEventsAfterCompletion()
    {
        string response = DecisionDocument("respond", null);
        OpenAiCharacterTurnParticipationStreamAccumulator duplicate = new(CreateRequest(true));
        Assert.Null(duplicate.Apply(Done(response)));

        Assert.Throws<InvalidDataException>(() => duplicate.Apply(Done(response)));

        OpenAiCharacterTurnParticipationStreamAccumulator completed = new(CreateRequest(true));
        Assert.Null(completed.Apply(Done(response)));
        Assert.NotNull(completed.Apply("{\"type\":\"response.completed\"}"));
        Assert.Throws<InvalidOperationException>(() => completed.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsEventsAfterFailure()
    {
        OpenAiCharacterTurnParticipationStreamAccumulator accumulator = new(CreateRequest(true));
        Assert.Throws<InvalidDataException>(() => accumulator.Apply("not-json"));

        Assert.Throws<InvalidOperationException>(() => accumulator.Apply(
            "{\"type\":\"response.created\"}"));
    }

    [Fact]
    public void RejectsBoundViolationsAndNulls()
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(true);
        OpenAiCharacterTurnParticipationStreamAccumulator excessiveEvent = new(request);
        OpenAiCharacterTurnParticipationStreamAccumulator excessiveDeltas = new(request);
        OpenAiCharacterTurnParticipationStreamAccumulator excessiveDone = new(request);

        Assert.Throws<InvalidDataException>(() => excessiveEvent.Apply(
            new string('x', OpenAiCharacterTurnParticipationStreamAccumulator.MaximumEventLength + 1)));
        Assert.Null(excessiveDeltas.Apply(Delta(new string(
            'x', CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength))));
        Assert.Throws<InvalidDataException>(() => excessiveDeltas.Apply(Delta("x")));
        Assert.Throws<InvalidDataException>(() => excessiveDone.Apply(Done(new string(
            'x', CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength + 1))));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterTurnParticipationStreamAccumulator(null!));
        Assert.Throws<ArgumentNullException>(() => excessiveEvent.Apply(null!));
    }

    [Fact]
    public void ExposesOnlyTheBoundedAccumulatorSurface()
    {
        Type accumulatorType = typeof(OpenAiCharacterTurnParticipationStreamAccumulator);
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            accumulatorType.GetConstructors());
        System.Reflection.MethodInfo method = Assert.Single(
            accumulatorType.GetMethods().Where(candidate => candidate.DeclaringType == accumulatorType));
        System.Reflection.FieldInfo field = Assert.Single(accumulatorType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal([typeof(CharacterTurnParticipationConsiderationRequest)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(CharacterTurnParticipationDecision), method.ReturnType);
        Assert.Equal([typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumEventLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(accumulatorType.GetProperties());
        Assert.Empty(accumulatorType.GetEvents());
        Assert.True(accumulatorType.IsSealed);
    }

    internal static CharacterTurnParticipationConsiderationRequest CreateRequest(
        bool isDirectlyAddressed)
    {
        CharacterId characterId = new("village-sage");
        Character character = new(
            new CharacterIdentity(characterId, "Village Sage"),
            new PublicCharacterBackground("An evidence-comparison consultant."),
            new PrivateCharacterBackground("Challenges unsupported assumptions."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [new CharacterGoal("compare-evidence", "Compare the available evidence.")]));
        CharacterId speakerCharacterId = new("seeker");
        DialogueEvent heardEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerCharacterId, [characterId]));
        return new CharacterTurnParticipationConsiderationRequest(
            character,
            heardEvent,
            isDirectlyAddressed);
    }

    internal static string DecisionDocument(string kind, string? motiveKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { kind, motiveKey });

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
