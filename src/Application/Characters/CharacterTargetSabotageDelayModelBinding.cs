using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed target-delay model.</summary>
public sealed class CharacterTargetSabotageDelayModelBinding
{
    /// <summary>Creates an immutable OpenAI target-delay model binding.</summary>
    public CharacterTargetSabotageDelayModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageDelayDecisionModel model)
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

    /// <summary>Gets the exact caller-composed target-delay decision model.</summary>
    public ICharacterTargetSabotageDelayDecisionModel Model { get; }
}
