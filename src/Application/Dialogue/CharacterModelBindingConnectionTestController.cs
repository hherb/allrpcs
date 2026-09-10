namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Dispatches provider-neutral connection tests through exact caller-composed bindings.
/// </summary>
public static class CharacterModelBindingConnectionTestController
{
    /// <summary>
    /// Tests the exact configured binding or returns a redacted unavailable result when absent.
    /// </summary>
    public static async ValueTask<CharacterModelConnectionTestResult> TestConfiguredAsync(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterModelBindingSnapshot bindings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(bindings);

        CharacterModelBinding? binding = CharacterModelSettingsBindingSelector.Select(
            selection,
            bindings);
        return await TestBindingAsync(binding, cancellationToken).ConfigureAwait(false);
    }

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
        return await TestBindingAsync(binding, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<CharacterModelConnectionTestResult> TestBindingAsync(
        CharacterModelBinding? binding,
        CancellationToken cancellationToken) =>
        binding is null
            ? CharacterModelConnectionTestResult.Failed(CharacterModelFailureCode.Unavailable)
            : await binding.ConnectionTester.TestConnectionAsync(
                binding.Settings,
                cancellationToken).ConfigureAwait(false);
}
