using System.Net;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Retains one explicit HTTP loopback origin for a caller-owned Ollama server.</summary>
public sealed class OllamaEndpoint
{
    /// <summary>Gets the conventional local Ollama endpoint.</summary>
    public static OllamaEndpoint Default { get; } = new(
        new Uri("http://127.0.0.1:11434/", UriKind.Absolute));

    /// <summary>Creates a loopback-only origin with an explicit non-default port.</summary>
    public OllamaEndpoint(Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        if (!baseUri.IsAbsoluteUri
            || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal)
            || !IPAddress.TryParse(baseUri.Host, out IPAddress? address)
            || !IPAddress.IsLoopback(address)
            || baseUri.IsDefaultPort
            || baseUri.AbsolutePath != "/"
            || !string.IsNullOrEmpty(baseUri.Query)
            || !string.IsNullOrEmpty(baseUri.Fragment)
            || !string.IsNullOrEmpty(baseUri.UserInfo))
        {
            throw new ArgumentException(
                "An Ollama endpoint must be an HTTP loopback origin with an explicit port.",
                nameof(baseUri));
        }

        BaseUri = baseUri;
        ChatUri = new Uri(baseUri, "api/chat");
        ShowUri = new Uri(baseUri, "api/show");
    }

    /// <summary>Gets the exact validated caller-supplied loopback origin.</summary>
    public Uri BaseUri { get; }

    /// <summary>Gets the derived native chat resource.</summary>
    public Uri ChatUri { get; }

    /// <summary>Gets the derived native model-inspection resource.</summary>
    public Uri ShowUri { get; }
}
