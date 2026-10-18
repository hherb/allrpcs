using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed recovery-decision model.</summary>
public sealed class CharacterTargetSabotageInfluenceRecoveryModelBinding
{
    /// <summary>Creates an immutable OpenAI influence-recovery model binding.</summary>
    public CharacterTargetSabotageInfluenceRecoveryModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model)
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

    /// <summary>Gets the exact caller-composed influence-recovery decision model.</summary>
    public ICharacterTargetSabotageInfluenceRecoveryDecisionModel Model { get; }
}
