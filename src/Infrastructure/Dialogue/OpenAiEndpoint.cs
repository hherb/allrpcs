using System.Net;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Retains one validated HTTPS or HTTP-loopback OpenAI-compatible API base.</summary>
public sealed class OpenAiEndpoint
{
    /// <summary>Gets the official OpenAI API base.</summary>
    public static OpenAiEndpoint Default { get; } = new(
        new Uri("https://api.openai.com/v1/", UriKind.Absolute));

    /// <summary>Creates an exact version-one API base, normalizing only its trailing slash.</summary>
    public OpenAiEndpoint(Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        if (!baseUri.IsAbsoluteUri
            || string.IsNullOrEmpty(baseUri.Host)
            || !HasAllowedTransport(baseUri)
            || (baseUri.AbsolutePath != "/v1" && baseUri.AbsolutePath != "/v1/")
            || !string.IsNullOrEmpty(baseUri.Query)
            || !string.IsNullOrEmpty(baseUri.Fragment)
            || !string.IsNullOrEmpty(baseUri.UserInfo))
        {
            throw new ArgumentException(
                "An OpenAI endpoint must be an HTTPS or HTTP-loopback version-one API base.",
                nameof(baseUri));
        }

        BaseUri = baseUri.AbsolutePath.EndsWith('/')
            ? baseUri
            : new Uri(baseUri.AbsoluteUri + "/", UriKind.Absolute);
        IsLoopback = IPAddress.TryParse(baseUri.Host, out IPAddress? address)
            && IPAddress.IsLoopback(address);
        ResponsesUri = new Uri(BaseUri, "responses");
        ChatCompletionsUri = new Uri(BaseUri, "chat/completions");
        ModelsUri = new Uri(BaseUri, "models/");
    }

    /// <summary>Gets the validated caller-supplied API base.</summary>
    public Uri BaseUri { get; }

    /// <summary>Gets whether the validated API base is hosted on numeric loopback.</summary>
    public bool IsLoopback { get; }

    /// <summary>Gets the derived Responses resource.</summary>
    public Uri ResponsesUri { get; }

    /// <summary>Gets the derived Chat Completions resource.</summary>
    public Uri ChatCompletionsUri { get; }

    /// <summary>Gets the derived Models collection resource.</summary>
    public Uri ModelsUri { get; }

    private static bool HasAllowedTransport(Uri baseUri)
    {
        if (string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(baseUri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal)
            && IPAddress.TryParse(baseUri.Host, out IPAddress? address)
            && IPAddress.IsLoopback(address)
            && !baseUri.IsDefaultPort;
    }
}
