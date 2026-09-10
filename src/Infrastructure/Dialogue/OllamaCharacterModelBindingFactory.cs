using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Composes exact caller-owned Ollama dependencies without performing input/output.</summary>
public static class OllamaCharacterModelBindingFactory
{
    /// <summary>Creates an Ollama model binding over exact caller-owned dependencies.</summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        OllamaEndpoint endpoint)
    {
        Validate(settings, client, endpoint);
        return new CharacterModelBinding(
            CharacterModelProvider.Ollama,
            settings,
            new OllamaCharacterModel(client, settings, endpoint),
            new OllamaCharacterModelConnectionTester(client, endpoint));
    }

    /// <summary>
    /// Creates an Ollama model binding with exact caller-owned redacted diagnostics.
    /// </summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        OllamaEndpoint endpoint,
        TimeProvider diagnosticTimeProvider,
        ICharacterModelGenerationDiagnosticSink diagnosticSink)
    {
        Validate(settings, client, endpoint);
        ArgumentNullException.ThrowIfNull(diagnosticTimeProvider);
        ArgumentNullException.ThrowIfNull(diagnosticSink);
        return new CharacterModelBinding(
            CharacterModelProvider.Ollama,
            settings,
            new OllamaCharacterModel(
                client,
                settings,
                endpoint,
                diagnosticTimeProvider,
                diagnosticSink),
            new OllamaCharacterModelConnectionTester(client, endpoint));
    }

    private static void Validate(
        CharacterModelSettings settings,
        HttpClient client,
        OllamaEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (settings.Provider != CharacterModelProvider.Ollama)
        {
            throw new ArgumentException("Settings must select the Ollama provider.", nameof(settings));
        }
    }
}
