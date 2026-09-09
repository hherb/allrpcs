namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains one bounded provider-neutral character-model failure without provider details.
/// </summary>
public sealed record CharacterModelFailure
{
    /// <summary>
    /// Creates a failure from one defined provider-neutral code.
    /// </summary>
    public CharacterModelFailure(CharacterModelFailureCode code)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        Code = code;
    }

    /// <summary>
    /// Gets the provider-neutral failure code.
    /// </summary>
    public CharacterModelFailureCode Code { get; }
}
