using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed concealment model.</summary>
public sealed class CharacterObjectSabotageConcealmentModelBinding
{
    /// <summary>Creates an immutable provider-selected concealment-model binding.</summary>
    public CharacterObjectSabotageConcealmentModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageConcealmentDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable provider-selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed concealment-decision model.</summary>
    public ICharacterObjectSabotageConcealmentDecisionModel Model { get; }
}
