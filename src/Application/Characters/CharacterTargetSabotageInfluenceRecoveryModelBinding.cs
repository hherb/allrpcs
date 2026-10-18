using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed recovery-decision model.</summary>
public sealed class CharacterTargetSabotageInfluenceRecoveryModelBinding
{
    /// <summary>Creates an immutable provider-selected influence-recovery model binding.</summary>
    public CharacterTargetSabotageInfluenceRecoveryModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed influence-recovery decision model.</summary>
    public ICharacterTargetSabotageInfluenceRecoveryDecisionModel Model { get; }
}
