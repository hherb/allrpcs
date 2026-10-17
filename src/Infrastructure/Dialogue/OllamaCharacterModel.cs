using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Streams one stateless response through a caller-selected loopback Ollama server.</summary>
public sealed class OllamaCharacterModel : ICharacterModel
{
    private const string NdjsonMediaType = "application/x-ndjson";
    private readonly HttpClient client;
    private readonly ProviderHttpTimeoutOptions timeoutOptions;
    private readonly ICharacterModelGenerationDiagnosticSink? diagnosticSink;
    private readonly TimeProvider? diagnosticTimeProvider;
    private readonly OllamaEndpoint endpoint;
    private readonly CharacterModelSettings settings;

    /// <summary>Creates an adapter over caller-owned HTTP using the conventional local endpoint.</summary>
    public OllamaCharacterModel(HttpClient client, CharacterModelSettings settings)
        : this(client, settings, OllamaEndpoint.Default)
    {
    }

    /// <summary>Creates an adapter over caller-owned HTTP and one validated loopback endpoint.</summary>
    public OllamaCharacterModel(
        HttpClient client,
        CharacterModelSettings settings,
        OllamaEndpoint endpoint,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (settings.Provider != CharacterModelProvider.Ollama)
        {
            throw new ArgumentException(
                "Settings must select the Ollama provider.",
                nameof(settings));
        }

        this.client = client;
        this.timeoutOptions = timeoutOptions ?? new ProviderHttpTimeoutOptions();
        this.settings = settings;
        this.endpoint = endpoint;
    }

    /// <summary>Creates an adapter with caller-owned redacted diagnostics.</summary>
    public OllamaCharacterModel(
        HttpClient client,
        CharacterModelSettings settings,
        OllamaEndpoint endpoint,
        TimeProvider diagnosticTimeProvider,
        ICharacterModelGenerationDiagnosticSink diagnosticSink)
        : this(client, settings, endpoint)
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

        using HttpRequestMessage message = new(HttpMethod.Post, endpoint.ChatUri)
        {
            Content = new StringContent(
                OllamaCharacterModelRequestSerializer.Serialize(request, settings.ModelName),
                Encoding.UTF8,
                "application/json"),
        };
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(NdjsonMediaType));

        HttpResponseMessage? response = null;
        CharacterModelFailureCode? transportFailure = null;
        try
        {
            response = await ProviderHttpTransport.SendAsync(
                    client,
                    message,
                    timeoutOptions,
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
                    CharacterModelHttpFailurePolicy.Termination(response, timeoutOptions.TimeProvider),
                    diagnosticStartTimestamp,
                    null,
                    null,
                    null,
                    cancellationToken).ConfigureAwait(false);
                yield break;
            }

            string? mediaType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(mediaType, NdjsonMediaType, StringComparison.OrdinalIgnoreCase))
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

            OllamaCharacterModelNdjsonResult result =
                await OllamaCharacterModelNdjsonCompletion.CompleteWithUsageAsync(
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
                CharacterModelProvider.Ollama,
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

    private static CharacterModelStreamTermination Failure(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));
}
