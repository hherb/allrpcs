using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed influence-decision model.</summary>
public sealed class CharacterTargetSabotageInfluenceModelBinding
{
    /// <summary>Creates an immutable OpenAI influence model binding.</summary>
    public CharacterTargetSabotageInfluenceModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageInfluenceDecisionModel model)
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

    /// <summary>Gets the exact caller-composed influence decision model.</summary>
    public ICharacterTargetSabotageInfluenceDecisionModel Model { get; }
}
