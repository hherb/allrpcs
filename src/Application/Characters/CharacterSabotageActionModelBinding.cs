using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed sabotage-action model.</summary>
public sealed class CharacterSabotageActionModelBinding
{
    /// <summary>Creates an immutable provider-selected sabotage-action model binding.</summary>
    public CharacterSabotageActionModelBinding(
        CharacterModelSettings settings,
        ICharacterSabotageActionDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed sabotage-action decision model.</summary>
    public ICharacterSabotageActionDecisionModel Model { get; }
}
