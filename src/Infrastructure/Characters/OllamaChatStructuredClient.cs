using System.Net.Http.Headers;
using System.Text;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Obtains a structured decision from the caller-selected loopback Ollama server.</summary>
public sealed class OllamaChatStructuredClient<TRequest, TDecision> : IStructuredDecisionClient<TRequest, TDecision>
    where TRequest : class where TDecision : class
{
    private readonly HttpClient client;
    private readonly CharacterModelSettings settings;
    private readonly StructuredDecisionDescriptor<TRequest, TDecision> descriptor;
    private readonly OllamaEndpoint endpoint;
    private readonly ProviderHttpTimeoutOptions timeoutOptions;

    /// <summary>Retains caller-owned transport and a validated local endpoint without I/O.</summary>
    public OllamaChatStructuredClient(HttpClient client, CharacterModelSettings settings,
        StructuredDecisionDescriptor<TRequest, TDecision> descriptor, OllamaEndpoint endpoint,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (settings.Provider != CharacterModelProvider.Ollama)
            throw new ArgumentException("Settings must select the Ollama provider.", nameof(settings));
        this.client = client;
        this.settings = settings;
        this.descriptor = descriptor;
        this.endpoint = endpoint;
        this.timeoutOptions = timeoutOptions ?? new();
    }

    /// <inheritdoc />
    public async ValueTask<TDecision> DecideAsync(TRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint.ChatUri)
        {
            Content = new StringContent(descriptor.Serialize(request, settings.Provider, settings.ModelName),
                Encoding.UTF8, "application/json"),
        };
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ndjson"));
        return await StructuredDecisionHttpTransport.CompleteAsync(client, message, "application/x-ndjson", timeoutOptions,
            (stream, token) => new OllamaStructuredDecisionAccumulator<TRequest, TDecision>(request, descriptor)
                .CompleteAsync(stream, token), cancellationToken).ConfigureAwait(false);
    }
}
