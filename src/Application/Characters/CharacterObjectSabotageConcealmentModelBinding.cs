using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed concealment model.</summary>
public sealed class CharacterObjectSabotageConcealmentModelBinding
{
    /// <summary>Creates an immutable OpenAI concealment-model binding.</summary>
    public CharacterObjectSabotageConcealmentModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageConcealmentDecisionModel model)
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

    /// <summary>Gets the exact caller-composed concealment-decision model.</summary>
    public ICharacterObjectSabotageConcealmentDecisionModel Model { get; }
}
