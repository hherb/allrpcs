using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed move model.</summary>
public sealed class CharacterObjectSabotageMoveModelBinding
{
    /// <summary>Creates an immutable provider-selected move-model binding.</summary>
    public CharacterObjectSabotageMoveModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageMoveDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed move-decision model.</summary>
    public ICharacterObjectSabotageMoveDecisionModel Model { get; }
}
