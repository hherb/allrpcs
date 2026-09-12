namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one caller-composed participation model for one explicit provider and setting.
/// </summary>
public sealed class CharacterTurnParticipationModelBinding
{
    /// <summary>Creates an immutable caller-composed participation-model binding.</summary>
    public CharacterTurnParticipationModelBinding(
        CharacterModelProvider provider,
        CharacterModelSettings settings,
        ICharacterTurnParticipationModel model)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Provider = provider;
        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact provider this binding registers.</summary>
    public CharacterModelProvider Provider { get; }

    /// <summary>Gets the exact immutable provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed participation model.</summary>
    public ICharacterTurnParticipationModel Model { get; }
}
