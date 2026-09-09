using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelProviderTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalProviderSet()
    {
        Assert.Collection(
            CharacterModelProvider.All,
            provider => Assert.Same(CharacterModelProvider.OpenAi, provider),
            provider => Assert.Same(CharacterModelProvider.Anthropic, provider),
            provider => Assert.Same(CharacterModelProvider.Ollama, provider));
        Assert.Equal(
            ["openai", "anthropic", "ollama"],
            CharacterModelProvider.All.Select(provider => provider.Value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToExactInstances()
    {
        foreach (CharacterModelProvider provider in CharacterModelProvider.All)
        {
            Assert.Same(provider, CharacterModelProvider.Parse(provider.Value));
            Assert.Equal(provider.Value, provider.ToString());
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("OpenAI")]
    [InlineData("open-ai")]
    [InlineData("openai ")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterModelProvider.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterModelProvider.Parse(null!));
    }
}
