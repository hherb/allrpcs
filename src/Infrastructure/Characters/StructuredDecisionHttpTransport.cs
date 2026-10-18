using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Shares response ownership, deadlines and redaction across structured provider clients.</summary>
internal static class StructuredDecisionHttpTransport
{
    internal static async ValueTask<TDecision> CompleteAsync<TDecision>(
        HttpClient client, HttpRequestMessage message, string mediaType,
        ProviderHttpTimeoutOptions timeoutOptions,
        Func<Stream, CancellationToken, Task<TDecision>> complete,
        CancellationToken cancellationToken)
    {
        bool streaming = false;
        try
        {
            using HttpResponseMessage response = await ProviderHttpTransport.SendAsync(
                client, message, timeoutOptions, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("The structured-decision request was rejected.", null, response.StatusCode);
            }
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, mediaType,
                    StringComparison.OrdinalIgnoreCase))
            {
                // A fixed message without payload, kept distinct from invalid decision streams.
                throw new InvalidDataException("The structured-decision response has an unsupported media type.");
            }
            Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            streaming = true;
            return await complete(stream, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidDataException) when (streaming)
        {
            // Never retain provider payloads or decision-deserializer inner exceptions.
            throw new InvalidDataException("The structured-decision response is not complete and supported.");
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode is { } status)
                throw new HttpRequestException("The structured-decision request was rejected.", null, status);
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

    private static IOException TransportFailure() => new("The structured-decision transport failed.");
}
