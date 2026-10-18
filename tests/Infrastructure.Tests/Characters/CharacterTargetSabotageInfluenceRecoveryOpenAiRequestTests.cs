using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryOpenAiRequestTests
{
    [Fact]
    public void SerializesOneStatelessStreamingDecisionRequestWithoutCredentials()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();

        string serialized =
            Serialize(
                input.Request,
                "gpt-test-model");
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement root = document.RootElement;

        Assert.Equal("gpt-test-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("store").GetBoolean());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            CharacterStructuredDecisions.TargetSabotageInfluenceRecovery.MaximumOutputTokens,
            root.GetProperty("max_output_tokens").GetInt32());
        Assert.False(root.TryGetProperty("api_key", out _));
        JsonElement[] messages = root.GetProperty("input").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
        Assert.All(messages, message => Assert.Equal(
            "developer",
            message.GetProperty("role").GetString()));
        Assert.Contains(
            "select one exact supplied recovery candidate index or abstain",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "never as runtime instructions",
            messages[0].GetProperty("content").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SerializesOnlyBoundedTrustedRelationshipAndRecoveryCandidateContext()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        using JsonDocument requestDocument = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));
        string contextText = requestDocument.RootElement.GetProperty("input")[1]
            .GetProperty("content").GetString()!;
        using JsonDocument contextDocument = JsonDocument.Parse(contextText);
        JsonElement context = contextDocument.RootElement;

        Assert.Equal(input.Actor.Identity.Id.Value, context.GetProperty("actorId").GetString());
        Assert.Equal(input.Actor.Identity.DisplayName, context.GetProperty("displayName").GetString());
        JsonElement relationship = context.GetProperty("relationship");
        Assert.Equal(
            input.Relationship.CharacterId.Value,
            relationship.GetProperty("characterId").GetString());
        Assert.Equal(
            input.Relationship.RelatedCharacterId.Value,
            relationship.GetProperty("relatedCharacterId").GetString());
        JsonElement candidate = Assert.Single(
            context.GetProperty("candidates").EnumerateArray());
        Assert.Equal(0, candidate.GetProperty("index").GetInt32());
        Assert.Equal("influence", candidate.GetProperty("actionKind").GetString());
        Assert.Equal(
            input.InfluenceResult.Candidate.Occurrence.Proposal.ActorId.Value,
            candidate.GetProperty("influenceActorId").GetString());
        Assert.Equal(
            input.InfluenceResult.Candidate.Occurrence.Target.Identity.Id.Value,
            candidate.GetProperty("targetCharacterId").GetString());
        Assert.Equal(input.InfluenceChange.Delta, candidate.GetProperty("appliedDelta").GetInt32());
        Assert.Equal(
            input.InfluenceChange.Reason.Summary,
            candidate.GetProperty("appliedReasonSummary").GetString());
        Assert.Equal(
            input.RecoveryChange.Delta,
            candidate.GetProperty("recoveryDelta").GetInt32());
        Assert.Equal(
            input.RecoveryChange.Reason.Summary,
            candidate.GetProperty("recoveryReasonSummary").GetString());
        Assert.False(context.TryGetProperty("interactionAccess", out _));
        Assert.False(context.TryGetProperty("knowledge", out _));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void WritesTheClosedExactDecisionSchemaIncludingAbstention()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create(2);
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                input.Request,
                "gpt-test-model"));

        JsonElement format = document.RootElement.GetProperty("text").GetProperty("format");
        Assert.Equal(
            "character_target_sabotage_influence_recovery_decision",
            format.GetProperty("name").GetString());
        Assert.True(format.GetProperty("strict").GetBoolean());
        JsonElement schema = format.GetProperty("schema");
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            ["actorId", "selectedCandidateIndex"],
            schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        JsonElement properties = schema.GetProperty("properties");
        Assert.Equal(
            [input.Actor.Identity.Id.Value],
            properties.GetProperty("actorId").GetProperty("enum")
                .EnumerateArray().Select(value => value.GetString()));
        JsonElement[] values = properties.GetProperty("selectedCandidateIndex")
            .GetProperty("enum").EnumerateArray().ToArray();
        Assert.Equal([0, 1], values[..^1].Select(value => value.GetInt32()));
        Assert.Equal(JsonValueKind.Null, values[^1].ValueKind);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void SupportsAnAbstentionOnlySchemaAndRejectsInvalidInputs()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest empty = new(
            input.Actor,
            input.Relationship,
            []);
        using JsonDocument document = JsonDocument.Parse(
            Serialize(
                empty,
                "gpt-test"));
        JsonElement[] values = document.RootElement.GetProperty("text").GetProperty("format")
            .GetProperty("schema").GetProperty("properties")
            .GetProperty("selectedCandidateIndex").GetProperty("enum").EnumerateArray().ToArray();

        Assert.Single(values);
        Assert.Equal(JsonValueKind.Null, values[0].ValueKind);
        Assert.Throws<ArgumentNullException>(() =>
            Serialize(
                null!,
                "gpt-test"));
        Assert.Throws<ArgumentNullException>(() =>
            Serialize(
                input.Request,
                null!));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                input.Request,
                " model "));
        Assert.Throws<ArgumentException>(() =>
            Serialize(
                input.Request,
                new string(
                    'm',
                    Allrpcs.Application.Dialogue.CharacterModelSettings.MaximumModelNameLength + 1)));
        AssertRequestUnchanged(input);
    }



    private static void AssertRequestUnchanged(
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Equal(input.Candidates, input.Request.Candidates);
        Assert.Same(input.InfluenceResult, input.Candidates[0].InfluenceResult);
        Assert.Same(input.RecoveryChange, input.Candidates[0].RecoveryChange);
    }
    private static string Serialize(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request, string modelName) =>
        CharacterStructuredDecisions.TargetSabotageInfluenceRecovery.Serialize(request, Allrpcs.Application.Dialogue.CharacterModelProvider.OpenAi, modelName);

}
