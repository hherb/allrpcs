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
    private readonly ICharacterModelGenerationDiagnosticSink? diagnosticSink;
    private readonly TimeProvider? diagnosticTimeProvider;
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

    /// <summary>
    /// Creates an adapter with caller-owned observational diagnostics and elapsed-time source.
    /// </summary>
    public OpenAiCharacterModel(
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        CharacterModelSettings settings,
        TimeProvider diagnosticTimeProvider,
        ICharacterModelGenerationDiagnosticSink diagnosticSink)
        : this(client, credentialSource, settings)
    {
        ArgumentNullException.ThrowIfNull(diagnosticTimeProvider);
        ArgumentNullException.ThrowIfNull(diagnosticSink);
        this.diagnosticTimeProvider = diagnosticTimeProvider;
        this.diagnosticSink = diagnosticSink;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
        CharacterModelGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        long? diagnosticStartTimestamp = TryGetDiagnosticTimestamp();

        OpenAiCredentialLoadResult credential =
            await credentialSource.LoadApiKeyAsync(cancellationToken)
            .ConfigureAwait(false);
        string? apiKey = credential.ApiKey;
        if (!IsValidApiKey(apiKey))
        {
            yield return await ObserveAsync(
                Failure(CharacterModelFailureCode.Authentication),
                diagnosticStartTimestamp,
                null,
                null,
                null,
                cancellationToken).ConfigureAwait(false);
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
            yield return await ObserveAsync(
                Failure(transportFailure.Value),
                diagnosticStartTimestamp,
                null,
                null,
                null,
                cancellationToken).ConfigureAwait(false);
            yield break;
        }

        using (response!)
        {
            if (!response!.IsSuccessStatusCode)
            {
                yield return await ObserveAsync(
                    Failure(CharacterModelHttpFailurePolicy.Classify(response.StatusCode)),
                    diagnosticStartTimestamp,
                    null,
                    null,
                    null,
                    cancellationToken).ConfigureAwait(false);
                yield break;
            }

            string? mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
            {
                yield return await ObserveAsync(
                    Failure(CharacterModelFailureCode.InvalidResponse),
                    diagnosticStartTimestamp,
                    null,
                    null,
                    null,
                    cancellationToken).ConfigureAwait(false);
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
                yield return await ObserveAsync(
                    Failure(transportFailure.Value),
                    diagnosticStartTimestamp,
                    null,
                    null,
                    null,
                    cancellationToken).ConfigureAwait(false);
                yield break;
            }

            OpenAiCharacterModelSseResult result =
                await OpenAiCharacterModelSseCompletion.CompleteWithUsageAsync(
                        request.TurnRequest,
                        responseStream!,
                        cancellationToken)
                    .ConfigureAwait(false);
            yield return await ObserveAsync(
                result.Termination,
                diagnosticStartTimestamp,
                result.InputTokenCount,
                result.OutputTokenCount,
                result.TotalTokenCount,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private long? TryGetDiagnosticTimestamp()
    {
        if (diagnosticTimeProvider is null)
        {
            return null;
        }

        try
        {
            return diagnosticTimeProvider.GetTimestamp();
        }
        catch (Exception exception) when (IsObservationalFailure(exception))
        {
            return null;
        }
    }

    private async ValueTask<CharacterModelStreamTermination> ObserveAsync(
        CharacterModelStreamTermination termination,
        long? startTimestamp,
        int? inputTokenCount,
        int? outputTokenCount,
        int? totalTokenCount,
        CancellationToken cancellationToken)
    {
        if (diagnosticSink is null || diagnosticTimeProvider is null || startTimestamp is null)
        {
            return termination;
        }

        try
        {
            TimeSpan elapsed = diagnosticTimeProvider.GetElapsedTime(startTimestamp.Value);
            elapsed = elapsed < TimeSpan.Zero
                ? TimeSpan.Zero
                : elapsed > CharacterModelGenerationDiagnostic.MaximumElapsed
                    ? CharacterModelGenerationDiagnostic.MaximumElapsed
                    : elapsed;
            CharacterModelGenerationDiagnostic diagnostic = new(
                CharacterModelProvider.OpenAi,
                termination.Status,
                termination.Failure?.Code,
                elapsed,
                inputTokenCount,
                outputTokenCount,
                totalTokenCount);
            await CharacterModelGenerationDiagnosticRecorder.TryRecordAsync(
                    diagnosticSink,
                    diagnostic,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsObservationalFailure(exception))
        {
            return termination;
        }

        return termination;
    }

    private static bool IsObservationalFailure(Exception exception) =>
        exception is not OutOfMemoryException
        and not StackOverflowException
        and not AccessViolationException;

    private static bool IsValidApiKey(string? apiKey) =>
        OpenAiCredentialPolicy.IsValid(apiKey);

    private static CharacterModelStreamTermination Failure(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));
}
