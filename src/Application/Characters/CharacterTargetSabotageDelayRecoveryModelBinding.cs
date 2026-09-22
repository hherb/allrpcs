using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed delay-recovery model.</summary>
public sealed class CharacterTargetSabotageDelayRecoveryModelBinding
{
    /// <summary>Creates an immutable OpenAI delay-recovery model binding.</summary>
    public CharacterTargetSabotageDelayRecoveryModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageDelayRecoveryDecisionModel model)
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

    /// <summary>Gets the exact caller-composed delay-recovery decision model.</summary>
    public ICharacterTargetSabotageDelayRecoveryDecisionModel Model { get; }
}
