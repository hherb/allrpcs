using System.Net.Http.Headers;
using System.Text;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Obtains one participation decision through a loopback Ollama chat endpoint.
/// </summary>
public sealed class OllamaCharacterTurnParticipationModel : ICharacterTurnParticipationModel
{
    private readonly HttpClient client;
    private readonly OllamaEndpoint endpoint;
    private readonly CharacterModelSettings settings;

    /// <summary>Creates an adapter over caller-owned HTTP dependencies.</summary>
    public OllamaCharacterTurnParticipationModel(
        HttpClient client,
        CharacterModelSettings settings)
        : this(client, settings, OllamaEndpoint.Default)
    {
    }

    /// <summary>Creates an adapter over caller-owned dependencies and one validated API base.</summary>
    public OllamaCharacterTurnParticipationModel(
        HttpClient client,
        CharacterModelSettings settings,
        OllamaEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (settings.Provider != CharacterModelProvider.Ollama)
        {
            throw new ArgumentException("Settings must select the Ollama provider.", nameof(settings));
        }

        this.client = client;
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

        string requestDocument = OllamaCharacterTurnParticipationRequestSerializer.Serialize(
            request,
            settings.ModelName);
        using HttpRequestMessage message = new(HttpMethod.Post, endpoint.ChatUri)
        {
            Content = new StringContent(requestDocument, Encoding.UTF8, "application/json"),
        };
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ndjson"));

        using HttpResponseMessage response = await SendAsync(message, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                "The Ollama participation request was rejected.",
                null,
                response.StatusCode);
        }

        string? mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "application/x-ndjson", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "The Ollama participation response is not an event stream.");
        }

        Stream responseStream = await ReadResponseStreamAsync(response, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            IReadOnlyList<string> documents = await OllamaNewlineDelimitedJsonReader.ReadAsync(
                responseStream, cancellationToken).ConfigureAwait(false);
            OllamaCharacterTurnParticipationStreamAccumulator accumulator = new(request);
            CharacterTurnParticipationDecision? decision = null;
            foreach (string document in documents)
            {
                if (decision is not null)
                {
                    throw new InvalidDataException("The Ollama participation stream continued after completion.");
                }
                decision = accumulator.Apply(document);
            }
            return decision ?? throw new InvalidDataException("The Ollama participation stream is incomplete.");
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
        new("The Ollama participation transport failed.");
}
