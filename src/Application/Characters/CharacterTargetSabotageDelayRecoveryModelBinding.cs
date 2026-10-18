using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed delay-recovery model.</summary>
public sealed class CharacterTargetSabotageDelayRecoveryModelBinding
{
    /// <summary>Creates an immutable provider-selected delay-recovery model binding.</summary>
    public CharacterTargetSabotageDelayRecoveryModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageDelayRecoveryDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed delay-recovery decision model.</summary>
    public ICharacterTargetSabotageDelayRecoveryDecisionModel Model { get; }
}
