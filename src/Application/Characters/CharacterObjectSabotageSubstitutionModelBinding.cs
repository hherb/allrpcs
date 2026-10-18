using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed substitution model.</summary>
public sealed class CharacterObjectSabotageSubstitutionModelBinding
{
    /// <summary>Creates an immutable OpenAI substitution-model binding.</summary>
    public CharacterObjectSabotageSubstitutionModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageSubstitutionDecisionModel model)
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

    /// <summary>Gets the exact caller-composed substitution-decision model.</summary>
    public ICharacterObjectSabotageSubstitutionDecisionModel Model { get; }
}
