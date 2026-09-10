using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiChatCompletionRequestSerializerTests
{
    [Fact]
    public void SerializesOneDeterministicReasoningFreeStructuredRequest()
    {
        CharacterModelGenerationRequest request = CreateRequest();

        using JsonDocument document = JsonDocument.Parse(
            OpenAiChatCompletionRequestSerializer.Serialize(request, "local-model"));
        JsonElement root = document.RootElement;

        Assert.Equal("local-model", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());
        Assert.Equal(0, root.GetProperty("temperature").GetInt32());
        Assert.Equal(0, root.GetProperty("seed").GetInt32());
        Assert.Equal(0, root.GetProperty("thinking_budget").GetInt32());
        Assert.Equal("none", root.GetProperty("reasoning_effort").GetString());
        Assert.False(
            root.GetProperty("chat_template_kwargs")
                .GetProperty("enable_thinking")
                .GetBoolean());
        Assert.False(
            root.GetProperty("chat_template_kwargs")
                .GetProperty("preserve_thinking")
                .GetBoolean());
        Assert.Equal(
            "none",
            root.GetProperty("chat_template_kwargs")
                .GetProperty("reasoning_effort")
                .GetString());
        Assert.Equal(
            "json_schema",
            root.GetProperty("response_format").GetProperty("type").GetString());
        Assert.True(
            root.GetProperty("response_format")
                .GetProperty("json_schema")
                .GetProperty("strict")
                .GetBoolean());

        JsonElement[] messages = root.GetProperty("messages").EnumerateArray().ToArray();
        Assert.Equal(["system", "user"], messages.Select(
            message => message.GetProperty("role").GetString()));
        Assert.Contains("respondingCharacterId", messages[0].GetProperty("content").GetString());
        Assert.Equal(
            request.TurnRequest.DialogueEvent.Utterance.Text,
            messages[1].GetProperty("content").GetString());
    }

    [Fact]
    public void RejectsInvalidInputs()
    {
        CharacterModelGenerationRequest request = CreateRequest();

        Assert.Throws<ArgumentNullException>(() =>
            OpenAiChatCompletionRequestSerializer.Serialize(null!, "local-model"));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiChatCompletionRequestSerializer.Serialize(request, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiChatCompletionRequestSerializer.Serialize(request, " model "));
    }

    private static CharacterModelGenerationRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Please wait here.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
        return new CharacterModelGenerationRequest(
            request,
            new CharacterModelContextSnapshot(
                [
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.ValidAffordance,
                        "type=wait;target=null"),
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        utterance.Text),
                ]));
    }
}
