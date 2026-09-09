using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Tests local Ollama model availability without generating content.</summary>
public sealed class OllamaCharacterModelConnectionTester : ICharacterModelConnectionTester
{
    private readonly HttpClient client;
    private readonly OllamaEndpoint endpoint;

    /// <summary>Creates a tester using the conventional local Ollama endpoint.</summary>
    public OllamaCharacterModelConnectionTester(HttpClient client)
        : this(client, OllamaEndpoint.Default)
    {
    }

    /// <summary>Creates a tester over caller-owned HTTP and one validated loopback endpoint.</summary>
    public OllamaCharacterModelConnectionTester(HttpClient client, OllamaEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(endpoint);
        this.client = client;
        this.endpoint = endpoint;
    }

    /// <inheritdoc />
    public async ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
        CharacterModelSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();
        if (settings.Provider != CharacterModelProvider.Ollama)
        {
            throw new ArgumentException("Settings must select the Ollama provider.", nameof(settings));
        }

        using HttpRequestMessage message = new(HttpMethod.Post, endpoint.ShowUri)
        {
            Content = new StringContent(
                SerializeModel(settings.ModelName),
                Encoding.UTF8,
                "application/json"),
        };
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

    private static string SerializeModel(string modelName)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", modelName);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
