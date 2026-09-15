using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed contamination model.</summary>
public sealed class CharacterObjectSabotageContaminationModelBinding
{
    /// <summary>Creates an immutable OpenAI contamination-model binding.</summary>
    public CharacterObjectSabotageContaminationModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageContaminationDecisionModel model)
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

    /// <summary>Gets the exact caller-composed contamination-decision model.</summary>
    public ICharacterObjectSabotageContaminationDecisionModel Model { get; }
}
