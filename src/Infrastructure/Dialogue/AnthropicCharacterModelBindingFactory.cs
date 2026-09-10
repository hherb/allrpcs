using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Composes exact caller-owned Anthropic dependencies without performing input/output.</summary>
public static class AnthropicCharacterModelBindingFactory
{
    /// <summary>Creates an Anthropic model binding over exact caller-owned dependencies.</summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IAnthropicCredentialSource credentialSource)
    {
        Validate(settings, client, credentialSource);
        return new CharacterModelBinding(
            CharacterModelProvider.Anthropic,
            settings,
            new AnthropicCharacterModel(client, credentialSource, settings),
            new AnthropicCharacterModelConnectionTester(client, credentialSource));
    }

    /// <summary>
    /// Creates an Anthropic model binding with exact caller-owned redacted diagnostics.
    /// </summary>
    public static CharacterModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IAnthropicCredentialSource credentialSource,
        TimeProvider diagnosticTimeProvider,
        ICharacterModelGenerationDiagnosticSink diagnosticSink)
    {
        Validate(settings, client, credentialSource);
        ArgumentNullException.ThrowIfNull(diagnosticTimeProvider);
        ArgumentNullException.ThrowIfNull(diagnosticSink);
        return new CharacterModelBinding(
            CharacterModelProvider.Anthropic,
            settings,
            new AnthropicCharacterModel(
                client,
                credentialSource,
                settings,
                diagnosticTimeProvider,
                diagnosticSink),
            new AnthropicCharacterModelConnectionTester(client, credentialSource));
    }

    private static void Validate(
        CharacterModelSettings settings,
        HttpClient client,
        IAnthropicCredentialSource credentialSource)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        if (settings.Provider != CharacterModelProvider.Anthropic)
        {
            throw new ArgumentException(
                "Settings must select the Anthropic provider.",
                nameof(settings));
        }
    }
}
