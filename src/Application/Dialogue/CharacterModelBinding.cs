namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one caller-composed model and connection tester for one explicit provider and setting.
/// </summary>
public sealed class CharacterModelBinding
{
    /// <summary>Creates an immutable caller-composed provider binding.</summary>
    public CharacterModelBinding(
        CharacterModelProvider provider,
        CharacterModelSettings settings,
        ICharacterModel model,
        ICharacterModelConnectionTester connectionTester)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(connectionTester);

        Provider = provider;
        Settings = settings;
        Model = model;
        ConnectionTester = connectionTester;
    }

    /// <summary>Gets the exact provider this binding registers.</summary>
    public CharacterModelProvider Provider { get; }

    /// <summary>Gets the exact immutable provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed model.</summary>
    public ICharacterModel Model { get; }

    /// <summary>Gets the exact caller-composed connection tester.</summary>
    public ICharacterModelConnectionTester ConnectionTester { get; }
}
