namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Owns transport deadlines without owning or changing an injected HTTP client.</summary>
public static class ProviderHttpTransport
{
    /// <summary>Creates the production handler, which never forwards credentials through redirects.</summary>
    public static HttpClientHandler CreateHandler() => new() { AllowAutoRedirect = false };

    /// <summary>Sends headers and transfers the request deadline to the returned response content.</summary>
    public static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        HttpRequestMessage message,
        ProviderHttpTimeoutOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(options);
        ProviderRequestDeadline deadline = new(options, cancellationToken);
        try
        {
            HttpResponseMessage response = await client.SendAsync(
                message, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
            response.Content = new ProviderDeadlineContent(response.Content, deadline);
            return response;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            deadline.Dispose();
            throw new HttpRequestException("The provider request deadline expired.");
        }
        catch
        {
            deadline.Dispose();
            throw;
        }
    }
}

internal sealed class ProviderRequestDeadline : IDisposable
{
    private readonly CancellationTokenSource timer;
    private readonly CancellationTokenSource linked;

    internal ProviderRequestDeadline(ProviderHttpTimeoutOptions options, CancellationToken callerToken)
    {
        Options = options;
        CallerToken = callerToken;
        timer = new(options.RequestTimeout, options.TimeProvider);
        linked = CancellationTokenSource.CreateLinkedTokenSource(callerToken, timer.Token);
    }

    internal ProviderHttpTimeoutOptions Options { get; }
    internal CancellationToken CallerToken { get; }
    internal CancellationToken Token => linked.Token;

    internal async ValueTask<T> ReadAsync<T>(
        Func<CancellationToken, ValueTask<T>> read, CancellationToken cancellationToken)
    {
        using CancellationTokenSource idle = new(Options.IdleTimeout, Options.TimeProvider);
        using CancellationTokenSource readCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            Token, idle.Token, cancellationToken);
        try
        {
            return await read(readCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!CallerToken.IsCancellationRequested
            && !cancellationToken.IsCancellationRequested)
        {
            throw new IOException("The provider response deadline expired.");
        }
    }

    public void Dispose()
    {
        linked.Dispose();
        timer.Dispose();
    }
}
