using Allrpcs.Infrastructure.Dialogue;
using Allrpcs.TestSupport;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class ProviderHttpTransportTests
{
    [Fact]
    public void ProductionHandlerRefusesCrossHostRedirects()
    {
        using HttpClientHandler handler = ProviderHttpTransport.CreateHandler();
        Assert.False(handler.AllowAutoRedirect);
    }

    [Fact]
    public async Task StalledHeadersExpireWithoutCancellingCaller()
    {
        using CancellationTokenSource caller = new();
        using HttpClient client = new(new StalledHandler());
        ManualTimeProvider clock = new();
        ProviderHttpTimeoutOptions options = new(timeProvider: clock);
        using HttpRequestMessage message = new(HttpMethod.Get, "https://example.invalid");
        Task<HttpResponseMessage> request = ProviderHttpTransport.SendAsync(client, message, options, caller.Token);
        clock.Advance(TimeSpan.FromSeconds(120));
        await Assert.ThrowsAsync<HttpRequestException>(() => request);
        Assert.False(caller.IsCancellationRequested);
    }

    [Fact]
    public async Task RequestDeadlineRemainsActiveAfterHeadersAndIsReleasedWithResponse()
    {
        using ProviderDeadlineTestTransport transport = new();
        using HttpClient client = new(transport);
        using HttpRequestMessage message = new(HttpMethod.Get, "https://example.invalid");
        using HttpResponseMessage response = await ProviderHttpTransport.SendAsync(
            client, message, transport.Options, default);
        Assert.Equal(1, transport.Clock.ActiveTimerCount);
        transport.Clock.Advance(TimeSpan.FromSeconds(100));
        Stream stream = await response.Content.ReadAsStreamAsync();
        Task<int> read = stream.ReadAsync(new byte[1]).AsTask();
        await transport.Body.Started.Task;
        transport.Clock.Advance(TimeSpan.FromSeconds(19));
        Assert.False(read.IsCompleted);
        transport.Clock.Advance(TimeSpan.FromSeconds(1));
        await Assert.ThrowsAsync<IOException>(() => read);
        response.Dispose();
        Assert.Equal(0, transport.Clock.ActiveTimerCount);
        Assert.True(transport.Body.Disposed);
        Assert.False(transport.Disposed);
    }

    [Fact]
    public async Task DisposingUnreadResponseReleasesTimerAndOriginalContent()
    {
        using ProviderDeadlineTestTransport transport = new();
        using HttpClient client = new(transport);
        using HttpRequestMessage message = new(HttpMethod.Get, "https://example.invalid");
        HttpResponseMessage response = await ProviderHttpTransport.SendAsync(client, message, transport.Options, default);
        Assert.Equal(1, transport.Clock.ActiveTimerCount);
        response.Dispose();
        Assert.Equal(0, transport.Clock.ActiveTimerCount);
        Assert.True(transport.Body.Disposed);
        Assert.False(transport.Disposed);
        Assert.False(transport.Body.Started.Task.IsCompleted);
    }

    private sealed class StalledHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException();
        }
    }
}
