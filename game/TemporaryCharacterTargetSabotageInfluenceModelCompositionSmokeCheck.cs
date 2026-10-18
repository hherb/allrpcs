using System.Reflection;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;
using Godot;
using NetHttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>Checks explicit inert influence-model binding composition and removal.</summary>
public partial class TemporaryCharacterTargetSabotageInfluenceModelCompositionSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterTargetSabotageInfluenceModelSource source = new();
        TemporaryCharacterTargetSabotageInfluenceModelComposition composition = new(source);
        CharacterModelSettings initialSettings = new(CharacterModelProvider.OpenAi, "gpt-initial");
        CharacterModelSettings replacementSettings = new(
            CharacterModelProvider.OpenAi,
            "gpt-replacement");
        ThrowingHandler handler = new();
        using NetHttpClient client = new(handler);
        ThrowingCredentialSource credentials = new();
        ICharacterTargetSabotageInfluenceDecisionModel offline = source.TakeModel();

        bool initialized = composition.TryInitialize(initialSettings, client, credentials);
        CharacterTargetSabotageInfluenceModel? initialModel =
            source.TakeModel() as CharacterTargetSabotageInfluenceModel;
        bool rejectedRepeatedInitialization = !composition.TryInitialize(
            initialSettings,
            client,
            credentials);
        bool replaced = composition.TryReplace(replacementSettings, client, credentials);
        CharacterTargetSabotageInfluenceModel? replacementModel =
            source.TakeModel() as CharacterTargetSabotageInfluenceModel;
        bool cleared = composition.TryClear();
        bool resumedOffline = ReferenceEquals(source.TakeModel(), offline);
        bool rejectedRepeatedClear = !composition.TryClear();

        if (!initialized
            || initialModel is null
            || !rejectedRepeatedInitialization
            || !replaced
            || replacementModel is null
            || ReferenceEquals(replacementModel, initialModel)
            || !cleared
            || !resumedOffline
            || !rejectedRepeatedClear
            || !RetainsExactDependencies(initialModel, initialSettings, client, credentials)
            || !RetainsExactDependencies(replacementModel, replacementSettings, client, credentials)
            || handler.SendCount != 0
            || credentials.LoadCount != 0
            || !RejectsInvalidInputs(composition, client, credentials)
            || !HasClosedSurface())
        {
            Fail("Influence-model binding composition changed state or performed input/output.");
            return;
        }

        GD.Print(
            "Influence-model composition smoke passed: exact configured and replacement "
            + "dependencies, exact-source clearing, stable offline fallback, closed surface, "
            + "and no credential, HTTP, or model input.");
        source.Free();
        GetTree().Quit();
    }

    private static bool RetainsExactDependencies(
        CharacterTargetSabotageInfluenceModel model,
        CharacterModelSettings settings,
        NetHttpClient client,
        IOpenAiCredentialSource credentials)
    {
        object transport = Field<object>(model, "client");
        return transport.GetType().GetGenericTypeDefinition() == typeof(OpenAiResponsesStructuredClient<,>)
            && ReferenceEquals(Field<NetHttpClient>(transport, "client"), client)
            && ReferenceEquals(Field<IOpenAiCredentialSource>(transport, "credentialSource"), credentials)
            && ReferenceEquals(Field<CharacterModelSettings>(transport, "settings"), settings);
    }

    private static bool RejectsInvalidInputs(
        TemporaryCharacterTargetSabotageInfluenceModelComposition composition,
        NetHttpClient client,
        IOpenAiCredentialSource credentials)
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        return ThrowsArgumentNull(
                () => _ = CreateComposition(null!))
            && ThrowsArgumentNull(() => composition.TryInitialize(null!, client, credentials))
            && ThrowsArgumentNull(() => composition.TryInitialize(settings, null!, credentials))
            && ThrowsArgumentNull(() => composition.TryInitialize(settings, client, null!))
            && ThrowsArgumentException(() => composition.TryInitialize(anthropic, client, credentials))
            && ThrowsArgumentNull(() => composition.TryReplace(null!, client, credentials))
            && ThrowsArgumentNull(() => composition.TryReplace(settings, null!, credentials))
            && ThrowsArgumentNull(() => composition.TryReplace(settings, client, null!))
            && ThrowsArgumentException(() => composition.TryReplace(anthropic, client, credentials));
    }

    private static TemporaryCharacterTargetSabotageInfluenceModelComposition CreateComposition(
        TemporaryCharacterTargetSabotageInfluenceModelSource source) => new(source);

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterTargetSabotageInfluenceModelComposition);
        string[] methods = type.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryClear", "TryInitialize", "TryReplace"])
            && type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length == 0;
    }

    private static T Field<T>(object instance, string name) where T : class =>
        (T)instance.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;

    private static bool ThrowsArgumentNull(Action action) => Throws<ArgumentNullException>(action);

    private static bool ThrowsArgumentException(Action action) => Throws<ArgumentException>(action);

    private static bool Throws<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        internal int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            throw new InvalidOperationException("Composition performed HTTP input/output.");
        }
    }

    private sealed class ThrowingCredentialSource : IOpenAiCredentialSource
    {
        internal int LoadCount { get; private set; }

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            LoadCount++;
            throw new InvalidOperationException("Composition loaded a credential.");
        }
    }
}
