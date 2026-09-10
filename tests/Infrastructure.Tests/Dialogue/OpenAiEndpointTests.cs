using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiEndpointTests
{
    [Theory]
    [InlineData("https://gateway.example/v1", "https://gateway.example/v1/")]
    [InlineData("https://gateway.example:8443/v1/", "https://gateway.example:8443/v1/")]
    [InlineData("http://127.0.0.1:8000/v1", "http://127.0.0.1:8000/v1/")]
    [InlineData("http://[::1]:8000/v1/", "http://[::1]:8000/v1/")]
    public void RetainsOneValidatedApiBaseAndDerivesResources(
        string supplied,
        string expectedBase)
    {
        OpenAiEndpoint endpoint = new(new Uri(supplied, UriKind.Absolute));

        Assert.Equal(expectedBase, endpoint.BaseUri.AbsoluteUri);
        Assert.Equal(expectedBase + "responses", endpoint.ResponsesUri.AbsoluteUri);
        Assert.Equal(
            expectedBase + "chat/completions",
            endpoint.ChatCompletionsUri.AbsoluteUri);
        Assert.Equal(expectedBase + "models/", endpoint.ModelsUri.AbsoluteUri);
        Assert.Equal(supplied.StartsWith("http://", StringComparison.Ordinal), endpoint.IsLoopback);
    }

    [Fact]
    public void ProvidesTheOfficialApiBaseByDefault()
    {
        Assert.Equal("https://api.openai.com/v1/", OpenAiEndpoint.Default.BaseUri.AbsoluteUri);
        Assert.Equal(
            "https://api.openai.com/v1/responses",
            OpenAiEndpoint.Default.ResponsesUri.AbsoluteUri);
        Assert.False(OpenAiEndpoint.Default.IsLoopback);
    }

    [Theory]
    [InlineData("http://localhost:8000/v1/")]
    [InlineData("http://192.0.2.1:8000/v1/")]
    [InlineData("http://127.0.0.1/v1/")]
    [InlineData("ftp://127.0.0.1:8000/v1/")]
    [InlineData("https://gateway.example/")]
    [InlineData("https://gateway.example/v1/models")]
    [InlineData("https://user@gateway.example/v1/")]
    [InlineData("https://gateway.example/v1/?query=value")]
    [InlineData("https://gateway.example/v1/#fragment")]
    public void RejectsUnsafeOrNonApiBases(string supplied)
    {
        Assert.Throws<ArgumentException>(() =>
            new OpenAiEndpoint(new Uri(supplied, UriKind.Absolute)));
    }

    [Fact]
    public void RejectsNullAndRelativeUris()
    {
        Assert.Throws<ArgumentNullException>(() => new OpenAiEndpoint(null!));
        Assert.Throws<ArgumentException>(() =>
            new OpenAiEndpoint(new Uri("/v1/", UriKind.Relative)));
    }
}
