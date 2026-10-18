using System.Net.Http.Headers;
using System.Text;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Obtains one typed structured decision through the shared bounded Responses transport.</summary>
public sealed class OpenAiResponsesStructuredClient<TRequest, TDecision> : IStructuredDecisionClient<TRequest, TDecision>
    where TRequest : class
    where TDecision : class
{
    private readonly HttpClient client;
    private readonly ProviderHttpTimeoutOptions timeoutOptions;
    private readonly IOpenAiCredentialSource credentialSource;
    private readonly OpenAiEndpoint endpoint;
    private readonly CharacterModelSettings settings;
    private readonly StructuredDecisionDescriptor<TRequest, TDecision> descriptor;

    /// <summary>Creates an adapter over caller-owned dependencies and one validated API base.</summary>
    public OpenAiResponsesStructuredClient(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings,
        OpenAiEndpoint endpoint,
        StructuredDecisionDescriptor<TRequest, TDecision> descriptor,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        this.client = client;
        this.timeoutOptions = timeoutOptions ?? new ProviderHttpTimeoutOptions();
        this.credentialSource = credentialSource;
        this.settings = settings;
        this.endpoint = endpoint;
        this.descriptor = descriptor;
    }

    /// <summary>Returns one decision while preserving request cancellation and response ownership.</summary>
    public async ValueTask<TDecision> DecideAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        OpenAiCredentialLoadResult credential = await credentialSource
            .LoadApiKeyAsync(cancellationToken)
            .ConfigureAwait(false);
        string? apiKey = credential?.ApiKey;
        if (!OpenAiCredentialPolicy.IsValid(apiKey))
        {
            throw new InvalidOperationException("An OpenAI credential is unavailable.");
        }

        string requestDocument = descriptor.Serialize(request, settings.Provider, settings.ModelName);
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint.ResponsesUri)
        {
            Content = new StringContent(requestDocument, Encoding.UTF8, "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        return await StructuredDecisionHttpTransport.CompleteAsync(
            client, message, "text/event-stream", timeoutOptions,
            (stream, token) => descriptor.CreateOpenAiAccumulator(request).CompleteAsync(stream, token), cancellationToken).ConfigureAwait(false);
    }
}
