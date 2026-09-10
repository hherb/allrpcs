using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterModelSettingsJsonDeserializerTests
{
    private const string ValidJson =
        "{\"schemaVersion\":1,\"selectedProvider\":\"anthropic\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"gpt-test\"},{"
        + "\"provider\":\"anthropic\",\"modelName\":\"claude-test\"}]}";

    [Fact]
    public void MaterializesCanonicalProvidersAndStableSettingsOrder()
    {
        CharacterModelSettingsSelectionSnapshot snapshot =
            CharacterModelSettingsJsonDeserializer.Deserialize(ValidJson);

        Assert.Same(CharacterModelProvider.Anthropic, snapshot.SelectedProvider);
        Assert.Collection(
            snapshot.Settings,
            first =>
            {
                Assert.Same(CharacterModelProvider.OpenAi, first.Provider);
                Assert.Equal("gpt-test", first.ModelName);
            },
            second =>
            {
                Assert.Same(CharacterModelProvider.Anthropic, second.Provider);
                Assert.Equal("claude-test", second.ModelName);
            });
        Assert.Equal(ValidJson, CharacterModelSettingsJsonSerializer.Serialize(snapshot));
    }

    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("{\"schemaVersion\":1}")]
    [InlineData(
        "{\"schemaVersion\":2,\"selectedProvider\":\"openai\",\"settings\":[]}")]
    [InlineData(
        "{\"schemaVersion\":1.0,\"selectedProvider\":\"openai\",\"settings\":[]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"schemaVersion\":1,\"selectedProvider\":\"openai\","
        + "\"settings\":[]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[],"
        + "\"credential\":\"secret\"}")]
    public void RejectsMalformedFutureDuplicateOrUnknownRoot(string serialized)
    {
        Assert.Throws<ArgumentException>(() =>
            CharacterModelSettingsJsonDeserializer.Deserialize(serialized));
    }

    [Theory]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"other\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"gpt-test\"}]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"anthropic\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"gpt-test\"}]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"gpt-test\"},{"
        + "\"provider\":\"openai\",\"modelName\":\"other\"}]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"bad model\"}]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"gpt-test\",\"unknown\":true}]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"selectedProvider\":\"openai\",\"settings\":[{"
        + "\"provider\":\"openai\",\"modelName\":\"one\"},{"
        + "\"provider\":\"anthropic\",\"modelName\":\"two\"},{"
        + "\"provider\":\"ollama\",\"modelName\":\"three\"},{"
        + "\"provider\":\"openai\",\"modelName\":\"four\"}]}")]
    public void RejectsInvalidSelectionOrNestedSettings(string serialized)
    {
        Assert.Throws<ArgumentException>(() =>
            CharacterModelSettingsJsonDeserializer.Deserialize(serialized));
    }

    [Fact]
    public void RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelSettingsJsonDeserializer.Deserialize(null!));
    }
}
