using System.Net.Http.Headers;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Tests OpenAI credential and model access without generating content.</summary>
public sealed class OpenAiCharacterModelConnectionTester : ICharacterModelConnectionTester
{
    private readonly HttpClient client;
    private readonly IOpenAiCredentialSource credentialSource;
    private readonly OpenAiEndpoint endpoint;

    /// <summary>Creates a tester over caller-owned HTTP and credential dependencies.</summary>
    public OpenAiCharacterModelConnectionTester(
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        : this(client, credentialSource, OpenAiEndpoint.Default)
    {
    }

    /// <summary>Creates a tester over caller-owned dependencies and one validated API base.</summary>
    public OpenAiCharacterModelConnectionTester(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        OpenAiEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        ArgumentNullException.ThrowIfNull(endpoint);
        this.client = client;
        this.credentialSource = credentialSource;
        this.endpoint = endpoint;
    }

    /// <inheritdoc />
    public async ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
        CharacterModelSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        OpenAiCredentialLoadResult credential =
            await credentialSource.LoadApiKeyAsync(cancellationToken).ConfigureAwait(false);
        if (credential.Status != OpenAiCredentialLoadStatus.Loaded
            || !OpenAiCredentialPolicy.IsValid(credential.ApiKey))
        {
            return CharacterModelConnectionTestResult.Failed(
                CharacterModelFailureCode.Authentication);
        }

        Uri modelEndpoint = new(endpoint.ModelsUri, Uri.EscapeDataString(settings.ModelName));
        using HttpRequestMessage message = new(HttpMethod.Get, modelEndpoint);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential.ApiKey);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using HttpResponseMessage response = await client.SendAsync(
                    message,
                    HttpCompletionOption.ResponseHeadersRead,
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
