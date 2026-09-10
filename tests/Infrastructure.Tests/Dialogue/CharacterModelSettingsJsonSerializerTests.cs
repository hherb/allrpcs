using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterModelSettingsJsonSerializerTests
{
    [Fact]
    public void WritesStableVersionOneDocumentInExactSettingsOrder()
    {
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettingsSelectionSnapshot snapshot = new(
            [anthropic, openAi],
            CharacterModelProvider.OpenAi);

        string serialized = CharacterModelSettingsJsonSerializer.Serialize(snapshot);

        Assert.Equal(
            "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[{"
            + "\"provider\":\"anthropic\",\"modelName\":\"claude-test\"},{"
            + "\"provider\":\"openai\",\"modelName\":\"gpt-test\"}]}",
            serialized);
        Assert.Same(anthropic, snapshot.Settings[0]);
        Assert.Same(openAi, snapshot.Settings[1]);
    }

    [Fact]
    public void ValueEqualSnapshotsSerializeIdentically()
    {
        static CharacterModelSettingsSelectionSnapshot Snapshot() => new(
            [new CharacterModelSettings(CharacterModelProvider.Ollama, "granite-test")],
            CharacterModelProvider.Ollama);

        Assert.Equal(
            CharacterModelSettingsJsonSerializer.Serialize(Snapshot()),
            CharacterModelSettingsJsonSerializer.Serialize(Snapshot()));
    }

    [Fact]
    public void RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelSettingsJsonSerializer.Serialize(null!));
    }
}
