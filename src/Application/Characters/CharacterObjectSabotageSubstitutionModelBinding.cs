using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed substitution model.</summary>
public sealed class CharacterObjectSabotageSubstitutionModelBinding
{
    /// <summary>Creates an immutable provider-selected substitution-model binding.</summary>
    public CharacterObjectSabotageSubstitutionModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageSubstitutionDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable provider-selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed substitution-decision model.</summary>
    public ICharacterObjectSabotageSubstitutionDecisionModel Model { get; }
}
