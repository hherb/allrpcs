using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Composes an exact loopback Ollama participation binding without input/output.</summary>
public static class OllamaCharacterTurnParticipationModelBindingFactory
{
    /// <summary>Creates a participation binding over caller-owned HTTP and endpoint dependencies.</summary>
    public static CharacterTurnParticipationModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        OllamaEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return new CharacterTurnParticipationModelBinding(
            CharacterModelProvider.Ollama,
            settings,
            new OllamaCharacterTurnParticipationModel(client, settings, endpoint));
    }
}
