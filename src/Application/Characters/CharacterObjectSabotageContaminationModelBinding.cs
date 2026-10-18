using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed contamination model.</summary>
public sealed class CharacterObjectSabotageContaminationModelBinding
{
    /// <summary>Creates an immutable provider-selected contamination-model binding.</summary>
    public CharacterObjectSabotageContaminationModelBinding(
        CharacterModelSettings settings,
        ICharacterObjectSabotageContaminationDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable provider-selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed contamination-decision model.</summary>
    public ICharacterObjectSabotageContaminationDecisionModel Model { get; }
}
