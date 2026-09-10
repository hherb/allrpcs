using System.Reflection;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class AnthropicCharacterModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactAnthropicBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.Anthropic, "claude-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterModelBinding binding = AnthropicCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource);

        Assert.Same(CharacterModelProvider.Anthropic, binding.Provider);
        Assert.Same(settings, binding.Settings);
        AnthropicCharacterModel model = Assert.IsType<AnthropicCharacterModel>(binding.Model);
        AnthropicCharacterModelConnectionTester tester =
            Assert.IsType<AnthropicCharacterModelConnectionTester>(binding.ConnectionTester);
        Assert.Same(client, Field<HttpClient>(model, "client"));
        Assert.Same(client, Field<HttpClient>(tester, "client"));
        Assert.Same(credentialSource, Field<IAnthropicCredentialSource>(model, "credentialSource"));
        Assert.Same(credentialSource, Field<IAnthropicCredentialSource>(tester, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(model, "settings"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void CreatesAnExactDiagnosticsEnabledAnthropicBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.Anthropic, "claude-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();
        StubTimeProvider timeProvider = new();
        ThrowingDiagnosticSink diagnosticSink = new();

        CharacterModelBinding binding = AnthropicCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource,
            timeProvider,
            diagnosticSink);

        AnthropicCharacterModel model = Assert.IsType<AnthropicCharacterModel>(binding.Model);
        Assert.Same(timeProvider, Field<TimeProvider>(model, "diagnosticTimeProvider"));
        Assert.Same(
            diagnosticSink,
            Field<ICharacterModelGenerationDiagnosticSink>(model, "diagnosticSink"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void RejectsInvalidInputsBeforeInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.Anthropic, "claude-test");
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();
        StubTimeProvider timeProvider = new();
        ThrowingDiagnosticSink diagnosticSink = new();

        Assert.Throws<ArgumentNullException>(() => AnthropicCharacterModelBindingFactory.Create(
            null!, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() => AnthropicCharacterModelBindingFactory.Create(
            settings, null!, credentialSource));
        Assert.Throws<ArgumentNullException>(() => AnthropicCharacterModelBindingFactory.Create(
            settings, client, null!));
        Assert.Throws<ArgumentException>(() => AnthropicCharacterModelBindingFactory.Create(
            openAi, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() => AnthropicCharacterModelBindingFactory.Create(
            settings, client, credentialSource, null!, diagnosticSink));
        Assert.Throws<ArgumentNullException>(() => AnthropicCharacterModelBindingFactory.Create(
            settings, client, credentialSource, timeProvider, null!));
        Assert.Throws<ArgumentException>(() => AnthropicCharacterModelBindingFactory.Create(
            openAi, client, credentialSource, timeProvider, diagnosticSink));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void ExposesOnlyTheTwoExactCreationOverloads()
    {
        MethodInfo[] methods = typeof(AnthropicCharacterModelBindingFactory).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Collection(
            methods.OrderBy(method => method.GetParameters().Length),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(IAnthropicCredentialSource),
                ],
                method.GetParameters().Select(parameter => parameter.ParameterType)),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(IAnthropicCredentialSource),
                    typeof(TimeProvider),
                    typeof(ICharacterModelGenerationDiagnosticSink),
                ],
                method.GetParameters().Select(parameter => parameter.ParameterType)));
        Assert.All(methods, method =>
        {
            Assert.Equal("Create", method.Name);
            Assert.Equal(typeof(CharacterModelBinding), method.ReturnType);
        });
    }

    private static T Field<T>(object instance, string name) where T : class =>
        Assert.IsAssignableFrom<T>(instance.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance));

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            throw new InvalidOperationException("Construction performed HTTP input/output.");
        }
    }

    private sealed class ThrowingCredentialSource : IAnthropicCredentialSource
    {
        public int LoadCount { get; private set; }

        public ValueTask<AnthropicCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            LoadCount++;
            throw new InvalidOperationException("Construction loaded a credential.");
        }
    }

    private sealed class StubTimeProvider : TimeProvider;

    private sealed class ThrowingDiagnosticSink : ICharacterModelGenerationDiagnosticSink
    {
        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Construction recorded a diagnostic.");
    }
}
