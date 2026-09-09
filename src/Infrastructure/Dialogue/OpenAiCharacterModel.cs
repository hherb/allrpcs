using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Streams one stateless character response through the OpenAI Responses HTTP endpoint.
/// </summary>
public sealed class OpenAiCharacterModel : ICharacterModel
{
    private static readonly Uri ResponsesEndpoint =
        new("https://api.openai.com/v1/responses", UriKind.Absolute);

    private readonly HttpClient client;
    private readonly IOpenAiCredentialSource credentialSource;
    private readonly CharacterModelSettings settings;

    /// <summary>Gets the maximum accepted OpenAI API-key length.</summary>
    public const int MaximumApiKeyLength = OpenAiCredentialPolicy.MaximumApiKeyLength;

    /// <summary>
    /// Creates an adapter over caller-owned HTTP and credential dependencies.
    /// </summary>
    public OpenAiCharacterModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        this.client = client;
        this.credentialSource = credentialSource;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
        CharacterModelGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        OpenAiCredentialLoadResult credential =
            await credentialSource.LoadApiKeyAsync(cancellationToken)
            .ConfigureAwait(false);
        string? apiKey = credential.ApiKey;
        if (!IsValidApiKey(apiKey))
        {
            yield return Failure(CharacterModelFailureCode.Authentication);
            yield break;
        }

        using HttpRequestMessage message = new(HttpMethod.Post, ResponsesEndpoint)
        {
            Content = new StringContent(
                OpenAiCharacterModelRequestSerializer.Serialize(request, settings.ModelName),
                Encoding.UTF8,
                "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        HttpResponseMessage? response = null;
        CharacterModelFailureCode? transportFailure = null;
        try
        {
            response = await client.SendAsync(
                    message,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            transportFailure = CharacterModelFailureCode.Transport;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            transportFailure = CharacterModelFailureCode.Transport;
        }

        if (transportFailure is not null)
        {
            yield return Failure(transportFailure.Value);
            yield break;
        }

        using (response!)
        {
            if (!response!.IsSuccessStatusCode)
            {
                yield return Failure(OpenAiHttpFailurePolicy.Classify(response.StatusCode));
                yield break;
            }

            string? mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
            {
                yield return Failure(CharacterModelFailureCode.InvalidResponse);
                yield break;
            }

            Stream? responseStream = null;
            try
            {
                responseStream = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                transportFailure = CharacterModelFailureCode.Transport;
            }
            catch (IOException)
            {
                transportFailure = CharacterModelFailureCode.Transport;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                transportFailure = CharacterModelFailureCode.Transport;
            }

            if (transportFailure is not null)
            {
                yield return Failure(transportFailure.Value);
                yield break;
            }

            CharacterModelStreamTermination termination =
                await OpenAiCharacterModelSseCompletion.CompleteAsync(
                        request.TurnRequest,
                        responseStream!,
                        cancellationToken)
                    .ConfigureAwait(false);
            yield return termination;
        }
    }

    private static bool IsValidApiKey(string? apiKey) =>
        OpenAiCredentialPolicy.IsValid(apiKey);

    private static CharacterModelStreamTermination Failure(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));
}
