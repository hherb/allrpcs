namespace Allrpcs.Application.Dialogue;

/// <summary>Tests one provider/model setting without generating in-world content.</summary>
public interface ICharacterModelConnectionTester
{
    /// <summary>Returns one redacted connection outcome while observing caller cancellation.</summary>
    ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
        CharacterModelSettings settings,
        CancellationToken cancellationToken);
}
