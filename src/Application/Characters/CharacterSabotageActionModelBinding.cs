using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed sabotage-action model.</summary>
public sealed class CharacterSabotageActionModelBinding
{
    /// <summary>Creates an immutable OpenAI sabotage-action model binding.</summary>
    public CharacterSabotageActionModelBinding(
        CharacterModelSettings settings,
        ICharacterSabotageActionDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable OpenAI provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed sabotage-action decision model.</summary>
    public ICharacterSabotageActionDecisionModel Model { get; }
}
