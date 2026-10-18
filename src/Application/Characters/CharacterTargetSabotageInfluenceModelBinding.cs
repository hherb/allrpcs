using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed influence-decision model.</summary>
public sealed class CharacterTargetSabotageInfluenceModelBinding
{
    /// <summary>Creates an immutable provider-selected influence model binding.</summary>
    public CharacterTargetSabotageInfluenceModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageInfluenceDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed influence decision model.</summary>
    public ICharacterTargetSabotageInfluenceDecisionModel Model { get; }
}
