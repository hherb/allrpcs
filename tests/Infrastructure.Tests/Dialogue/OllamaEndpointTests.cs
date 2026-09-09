using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaEndpointTests
{
    [Fact]
    public void DefinesTheDefaultNativeChatEndpoint()
    {
        Assert.Equal("http://127.0.0.1:11434/", OllamaEndpoint.Default.BaseUri.AbsoluteUri);
        Assert.Equal(
            "http://127.0.0.1:11434/api/chat",
            OllamaEndpoint.Default.ChatUri.AbsoluteUri);
        Assert.Equal(
            "http://127.0.0.1:11434/api/show",
            OllamaEndpoint.Default.ShowUri.AbsoluteUri);
    }

    [Theory]
    [InlineData("http://127.0.0.1:22334/")]
    [InlineData("http://[::1]:22334/")]
    public void AcceptsExplicitHttpLoopbackOrigins(string serialized)
    {
        Uri supplied = new(serialized, UriKind.Absolute);

        OllamaEndpoint endpoint = new(supplied);

        Assert.Same(supplied, endpoint.BaseUri);
        Assert.Equal("/api/chat", endpoint.ChatUri.AbsolutePath);
        Assert.Equal(22334, endpoint.ChatUri.Port);
    }

    [Theory]
    [InlineData("https://127.0.0.1:11434/")]
    [InlineData("http://localhost:11434/")]
    [InlineData("http://192.168.1.2:11434/")]
    [InlineData("http://127.0.0.1/")]
    [InlineData("http://127.0.0.1:11434/path")]
    [InlineData("http://127.0.0.1:11434/?query=value")]
    [InlineData("http://127.0.0.1:11434/#fragment")]
    [InlineData("http://user@127.0.0.1:11434/")]
    public void RejectsNonLoopbackOrNonOriginEndpoints(string serialized)
    {
        Assert.Throws<ArgumentException>(() =>
            new OllamaEndpoint(new Uri(serialized, UriKind.Absolute)));
    }

    [Fact]
    public void RejectsNullAndRelativeUris()
    {
        Assert.Throws<ArgumentNullException>(() => new OllamaEndpoint(null!));
        Assert.Throws<ArgumentException>(() =>
            new OllamaEndpoint(new Uri("relative", UriKind.Relative)));
    }
}
