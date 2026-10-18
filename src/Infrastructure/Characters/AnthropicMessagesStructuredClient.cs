using System.Net.Http.Headers;
using System.Text;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Obtains a structured decision through the Anthropic Messages stream.</summary>
public sealed class AnthropicMessagesStructuredClient<TRequest, TDecision> : IStructuredDecisionClient<TRequest, TDecision>
    where TRequest : class where TDecision : class
{
    private static readonly Uri MessagesEndpoint = new("https://api.anthropic.com/v1/messages");
    private readonly HttpClient client;
    private readonly IAnthropicCredentialSource credentialSource;
    private readonly CharacterModelSettings settings;
    private readonly StructuredDecisionDescriptor<TRequest, TDecision> descriptor;
    private readonly ProviderHttpTimeoutOptions timeoutOptions;

    /// <summary>Retains exact caller dependencies without loading credentials or sending a request.</summary>
    public AnthropicMessagesStructuredClient(HttpClient client, IAnthropicCredentialSource credentialSource,
        CharacterModelSettings settings, StructuredDecisionDescriptor<TRequest, TDecision> descriptor,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (settings.Provider != CharacterModelProvider.Anthropic)
            throw new ArgumentException("Settings must select the Anthropic provider.", nameof(settings));
        this.client = client;
        this.credentialSource = credentialSource;
        this.settings = settings;
        this.descriptor = descriptor;
        this.timeoutOptions = timeoutOptions ?? new();
    }

    /// <inheritdoc />
    public async ValueTask<TDecision> DecideAsync(TRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        AnthropicCredentialLoadResult credential = await credentialSource.LoadApiKeyAsync(cancellationToken)
            .ConfigureAwait(false);
        if (!AnthropicCredentialPolicy.IsValid(credential?.ApiKey))
            throw new InvalidOperationException("An Anthropic credential is unavailable.");
        using HttpRequestMessage message = new(HttpMethod.Post, MessagesEndpoint)
        {
            Content = new StringContent(descriptor.Serialize(request, settings.Provider, settings.ModelName),
                Encoding.UTF8, "application/json"),
        };
        message.Headers.Add("x-api-key", credential!.ApiKey);
        message.Headers.Add("anthropic-version", "2023-06-01");
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        return await StructuredDecisionHttpTransport.CompleteAsync(client, message, "text/event-stream", timeoutOptions,
            (stream, token) => new AnthropicStructuredDecisionAccumulator<TRequest, TDecision>(request, descriptor)
                .CompleteAsync(stream, token), cancellationToken).ConfigureAwait(false);
    }
}
