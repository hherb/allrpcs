using System.Net.Http.Headers;
using System.Text;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Obtains one participation decision through an OpenAI Responses HTTP endpoint.
/// </summary>
public sealed class OpenAiCharacterTurnParticipationModel : ICharacterTurnParticipationModel
{
    private readonly HttpClient client;
    private readonly IOpenAiCredentialSource credentialSource;
    private readonly OpenAiEndpoint endpoint;
    private readonly CharacterModelSettings settings;

    /// <summary>Creates an adapter over caller-owned HTTP and credential dependencies.</summary>
    public OpenAiCharacterTurnParticipationModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings)
        : this(client, credentialSource, settings, OpenAiEndpoint.Default)
    {
    }

    /// <summary>Creates an adapter over caller-owned dependencies and one validated API base.</summary>
    public OpenAiCharacterTurnParticipationModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings,
        OpenAiEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        this.client = client;
        this.credentialSource = credentialSource;
        this.settings = settings;
        this.endpoint = endpoint;
    }

    /// <inheritdoc />
    public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
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

        string requestDocument = OpenAiCharacterTurnParticipationRequestSerializer.Serialize(
            request,
            settings.ModelName);
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint.ResponsesUri)
        {
            Content = new StringContent(requestDocument, Encoding.UTF8, "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using HttpResponseMessage response = await SendAsync(message, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                "The OpenAI participation request was rejected.",
                null,
                response.StatusCode);
        }

        string? mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "The OpenAI participation response is not an event stream.");
        }

        Stream responseStream = await ReadResponseStreamAsync(response, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            return await OpenAiCharacterTurnParticipationSseCompletion
                .CompleteAsync(request, responseStream, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (IOException)
        {
            throw TransportFailure();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw TransportFailure();
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.SendAsync(
                    message,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            throw TransportFailure();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw TransportFailure();
        }
    }

    private static async Task<Stream> ReadResponseStreamAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            throw TransportFailure();
        }
        catch (IOException)
        {
            throw TransportFailure();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw TransportFailure();
        }
    }

    private static IOException TransportFailure() =>
        new("The OpenAI participation transport failed.");
}
