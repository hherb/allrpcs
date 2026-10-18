using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed target-delay model.</summary>
public sealed class CharacterTargetSabotageDelayModelBinding
{
    /// <summary>Creates an immutable provider-selected target-delay model binding.</summary>
    public CharacterTargetSabotageDelayModelBinding(
        CharacterModelSettings settings,
        ICharacterTargetSabotageDelayDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable provider-selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed target-delay decision model.</summary>
    public ICharacterTargetSabotageDelayDecisionModel Model { get; }
}
