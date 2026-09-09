using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsTests
{
    [Fact]
    public void RetainsTheExactProviderAndModelName()
    {
        CharacterModelProvider provider = CharacterModelProvider.OpenAi;
        string modelName = "gpt-test-model";

        CharacterModelSettings settings = new(provider, modelName);

        Assert.Same(provider, settings.Provider);
        Assert.Same(modelName, settings.ModelName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" model")]
    [InlineData("model ")]
    [InlineData("model\tname")]
    [InlineData("model\0name")]
    public void RejectsInvalidModelNames(string modelName)
    {
        Assert.Throws<ArgumentException>(
            () => new CharacterModelSettings(CharacterModelProvider.OpenAi, modelName));
    }

    [Fact]
    public void RejectsNullsAndExcessiveModelNames()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterModelSettings(
            null!,
            "model"));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelSettings(
            CharacterModelProvider.OpenAi,
            null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettings(
            CharacterModelProvider.OpenAi,
            new string('m', CharacterModelSettings.MaximumModelNameLength + 1)));
    }
}
