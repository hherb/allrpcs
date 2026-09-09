namespace Allrpcs.Application.Dialogue;

/// <summary>Reports a connected or provider-neutral redacted connection-test outcome.</summary>
public sealed class CharacterModelConnectionTestResult
{
    private CharacterModelConnectionTestResult(bool isConnected, CharacterModelFailure? failure)
    {
        IsConnected = isConnected;
        Failure = failure;
    }

    /// <summary>Gets whether the configured provider and model were reachable.</summary>
    public bool IsConnected { get; }

    /// <summary>Gets the provider-neutral failure, or <see langword="null"/> on success.</summary>
    public CharacterModelFailure? Failure { get; }

    /// <summary>Creates a connected result.</summary>
    public static CharacterModelConnectionTestResult Connected() => new(true, null);

    /// <summary>Creates a redacted failed result.</summary>
    public static CharacterModelConnectionTestResult Failed(CharacterModelFailureCode code) =>
        new(false, new CharacterModelFailure(code));

    /// <inheritdoc />
    public override string ToString() =>
        IsConnected ? "Connected" : $"Failed:{Failure!.Code}";
}
