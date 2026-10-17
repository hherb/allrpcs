using System.Net.Http.Headers;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Tests Anthropic credential and model access without generating content.</summary>
public sealed class AnthropicCharacterModelConnectionTester : ICharacterModelConnectionTester
{
    private const string ApiKeyHeaderName = "x-api-key";
    private const string VersionHeaderName = "anthropic-version";
    private const string VersionHeaderValue = "2023-06-01";
    private static readonly Uri ModelsEndpoint =
        new("https://api.anthropic.com/v1/models/", UriKind.Absolute);

    private readonly HttpClient client;
    private readonly ProviderHttpTimeoutOptions timeoutOptions;
    private readonly IAnthropicCredentialSource credentialSource;

    /// <summary>Creates a tester over caller-owned HTTP and credential dependencies.</summary>
    public AnthropicCharacterModelConnectionTester(
        HttpClient client,
        IAnthropicCredentialSource credentialSource,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        this.client = client;
        this.timeoutOptions = timeoutOptions ?? new ProviderHttpTimeoutOptions();
        this.credentialSource = credentialSource;
    }

    /// <inheritdoc />
    public async ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
        CharacterModelSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();
        if (settings.Provider != CharacterModelProvider.Anthropic)
        {
            throw new ArgumentException(
                "Settings must select the Anthropic provider.",
                nameof(settings));
        }

        AnthropicCredentialLoadResult credential =
            await credentialSource.LoadApiKeyAsync(cancellationToken).ConfigureAwait(false);
        if (credential.Status != AnthropicCredentialLoadStatus.Loaded
            || !AnthropicCredentialPolicy.IsValid(credential.ApiKey))
        {
            return CharacterModelConnectionTestResult.Failed(
                CharacterModelFailureCode.Authentication);
        }

        Uri endpoint = new(ModelsEndpoint, Uri.EscapeDataString(settings.ModelName));
        using HttpRequestMessage message = new(HttpMethod.Get, endpoint);
        _ = message.Headers.TryAddWithoutValidation(ApiKeyHeaderName, credential.ApiKey);
        _ = message.Headers.TryAddWithoutValidation(VersionHeaderName, VersionHeaderValue);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using HttpResponseMessage response = await ProviderHttpTransport.SendAsync(
                    client,
                    message,
                    timeoutOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? CharacterModelConnectionTestResult.Connected()
                : CharacterModelConnectionTestResult.Failed(
                    CharacterModelHttpFailurePolicy.Classify(response.StatusCode));
        }
        catch (HttpRequestException)
        {
            return CharacterModelConnectionTestResult.Failed(
                CharacterModelFailureCode.Transport);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return CharacterModelConnectionTestResult.Failed(
                CharacterModelFailureCode.Transport);
        }
    }
}
