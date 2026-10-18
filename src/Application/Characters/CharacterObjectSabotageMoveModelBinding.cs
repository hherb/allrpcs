using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed move model.</summary>
public sealed class CharacterObjectSabotageMoveModelBinding
{
    /// <summary>Creates an immutable OpenAI move-model binding.</summary>
    public CharacterObjectSabotageMoveModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageMoveDecisionModel model)
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

    /// <summary>Gets the exact caller-composed move-decision model.</summary>
    public ICharacterObjectSabotageMoveDecisionModel Model { get; }
}
