namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Dispatches provider-neutral connection tests through exact caller-composed bindings.
/// </summary>
public static class CharacterModelBindingConnectionTestController
{
    /// <summary>
    /// Tests the exact selected binding or returns a redacted unavailable result when absent.
    /// </summary>
    public static async ValueTask<CharacterModelConnectionTestResult> TestAsync(
        CharacterModelProvider provider,
        CharacterModelBindingSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(snapshot);

        CharacterModelBinding? binding = CharacterModelBindingSelector.Select(provider, snapshot);
        if (binding is null)
        {
            return CharacterModelConnectionTestResult.Failed(
                CharacterModelFailureCode.Unavailable);
        }

        return await binding.ConnectionTester.TestConnectionAsync(
            binding.Settings,
            cancellationToken).ConfigureAwait(false);
    }
}
