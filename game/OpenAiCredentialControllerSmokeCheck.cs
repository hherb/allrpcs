using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit redacted credential operations through an injected store.</summary>
public partial class OpenAiCredentialControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENAI_CREDENTIAL_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        OpenAiCredentialController controller = GetNode<OpenAiCredentialController>(
            "../OpenAiCredentialController");
        RecordingCredentialStore store = new(
            ["sk-test-one", "sk-test-two"],
            [OpenAiCredentialWriteStatus.Saved, OpenAiCredentialWriteStatus.Failed],
            [
                OpenAiCredentialDeleteStatus.Deleted,
                OpenAiCredentialDeleteStatus.Missing,
                OpenAiCredentialDeleteStatus.Failed,
            ]);

        if (store.OperationCount != 0 || !controller.TryInitialize(store))
        {
            Fail("OpenAI credential controller performed startup I/O or rejected its exact store.");
            return;
        }

        try
        {
            controller.TryInitialize(null!);
            Fail("OpenAI credential controller accepted a null store.");
            return;
        }
        catch (ArgumentNullException)
        {
        }

        if (controller.TryInitialize(store)
            || Save(controller, string.Empty) != OpenAiCredentialWriteStatus.Failed
            || store.OperationCount != 0
            || Save(controller, "sk-test-one") != OpenAiCredentialWriteStatus.Saved
            || Save(controller, "sk-test-two") != OpenAiCredentialWriteStatus.Failed
            || Delete(controller) != OpenAiCredentialDeleteStatus.Deleted
            || Delete(controller) != OpenAiCredentialDeleteStatus.Missing
            || Delete(controller) != OpenAiCredentialDeleteStatus.Failed
            || store.SaveCount != 2
            || store.DeleteCount != 3
            || store.LoadCount != 0
            || !store.ReceivedOnlyExpectedApiKeys)
        {
            Fail("OpenAI credential controller did not preserve exact bounded store outcomes.");
            return;
        }

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        try
        {
            Save(controller, "sk-cancelled", cancellation.Token);
            Fail("OpenAI credential controller ignored cancelled save input.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        try
        {
            Delete(controller, cancellation.Token);
            Fail("OpenAI credential controller ignored cancelled delete input.");
            return;
        }
        catch (OperationCanceledException)
        {
        }

        if (store.OperationCount != 5 || !HasBoundedPublicSurface())
        {
            Fail("OpenAI credential controller delegated cancellation or exposed extra state.");
            return;
        }

        GD.Print(
            "OpenAI credential controller smoke passed: zero-I/O startup, exact injected-store "
            + "delegation, redacted saved, deleted, missing, and failed outcomes, cancellation, "
            + "one-time initialization, and credential-free public state were correct.");
        GetTree().Quit();
    }

    private static OpenAiCredentialWriteStatus Save(
        OpenAiCredentialController controller,
        string apiKey,
        CancellationToken cancellationToken = default) =>
        controller.SaveApiKeyAsync(apiKey, cancellationToken).AsTask().GetAwaiter().GetResult();

    private static OpenAiCredentialDeleteStatus Delete(
        OpenAiCredentialController controller,
        CancellationToken cancellationToken = default) =>
        controller.DeleteApiKeyAsync(cancellationToken).AsTask().GetAwaiter().GetResult();

    private static bool HasBoundedPublicSurface()
    {
        Type type = typeof(OpenAiCredentialController);
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return properties.Length == 0
            && methods.SequenceEqual(
                ["DeleteApiKeyAsync", "SaveApiKeyAsync", nameof(OpenAiCredentialController._Ready)]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class RecordingCredentialStore : IOpenAiCredentialStore
    {
        private readonly Queue<string> expectedApiKeys;
        private readonly Queue<OpenAiCredentialWriteStatus> writeStatuses;
        private readonly Queue<OpenAiCredentialDeleteStatus> deleteStatuses;

        public RecordingCredentialStore(
            IEnumerable<string> expectedApiKeys,
            IEnumerable<OpenAiCredentialWriteStatus> writeStatuses,
            IEnumerable<OpenAiCredentialDeleteStatus> deleteStatuses)
        {
            this.expectedApiKeys = new Queue<string>(expectedApiKeys);
            this.writeStatuses = new Queue<OpenAiCredentialWriteStatus>(writeStatuses);
            this.deleteStatuses = new Queue<OpenAiCredentialDeleteStatus>(deleteStatuses);
        }

        public int SaveCount { get; private set; }

        public int LoadCount { get; private set; }

        public int DeleteCount { get; private set; }

        public int OperationCount => SaveCount + LoadCount + DeleteCount;

        public bool ReceivedOnlyExpectedApiKeys { get; private set; } = true;

        public ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
            string apiKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReceivedOnlyExpectedApiKeys &= expectedApiKeys.Count > 0
                && apiKey == expectedApiKeys.Dequeue();
            SaveCount++;
            return ValueTask.FromResult(writeStatuses.Dequeue());
        }

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LoadCount++;
            return ValueTask.FromResult(OpenAiCredentialLoadResult.Failed());
        }

        public ValueTask<OpenAiCredentialDeleteStatus> DeleteApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteCount++;
            return ValueTask.FromResult(deleteStatuses.Dequeue());
        }
    }
}
