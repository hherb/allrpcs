using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Composes exact caller-owned OpenAI dependencies without performing input/output.</summary>
public static class OpenAiCharacterModelBindingFactory
{
    /// <summary>Creates an OpenAI model binding over exact caller-owned dependencies.</summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        => Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates a binding over exact dependencies and one validated API base.</summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        OpenAiEndpoint endpoint)
    {
        Validate(settings, client, credentialSource);
        ArgumentNullException.ThrowIfNull(endpoint);
        return new CharacterModelBinding(
            CharacterModelProvider.OpenAi,
            settings,
            new OpenAiCharacterModel(client, credentialSource, settings, endpoint),
            new OpenAiCharacterModelConnectionTester(client, credentialSource, endpoint));
    }

    /// <summary>
    /// Creates an OpenAI model binding with exact caller-owned redacted diagnostics.
    /// </summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        TimeProvider diagnosticTimeProvider,
        ICharacterModelGenerationDiagnosticSink diagnosticSink)
        => Create(
            settings,
            client,
            credentialSource,
            OpenAiEndpoint.Default,
            diagnosticTimeProvider,
            diagnosticSink);

    /// <summary>Creates an endpoint-specific binding with exact redacted diagnostics.</summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        OpenAiEndpoint endpoint,
        TimeProvider diagnosticTimeProvider,
        ICharacterModelGenerationDiagnosticSink diagnosticSink)
    {
        Validate(settings, client, credentialSource);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(diagnosticTimeProvider);
        ArgumentNullException.ThrowIfNull(diagnosticSink);
        return new CharacterModelBinding(
            CharacterModelProvider.OpenAi,
            settings,
            new OpenAiCharacterModel(
                client,
                credentialSource,
                settings,
                endpoint,
                diagnosticTimeProvider,
                diagnosticSink),
            new OpenAiCharacterModelConnectionTester(client, credentialSource, endpoint));
    }

    private static void Validate(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }
    }
}
