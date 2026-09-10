using System.Reflection;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterModelBinding binding = OpenAiCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource);

        Assert.Same(CharacterModelProvider.OpenAi, binding.Provider);
        Assert.Same(settings, binding.Settings);
        OpenAiCharacterModel model = Assert.IsType<OpenAiCharacterModel>(binding.Model);
        OpenAiCharacterModelConnectionTester tester =
            Assert.IsType<OpenAiCharacterModelConnectionTester>(binding.ConnectionTester);
        Assert.Same(client, Field<HttpClient>(model, "client"));
        Assert.Same(client, Field<HttpClient>(tester, "client"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(model, "credentialSource"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(tester, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(model, "settings"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(model, "endpoint"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(tester, "endpoint"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void CreatesAnExactEndpointSpecificBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "local-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();
        OpenAiEndpoint endpoint = new(new Uri("http://127.0.0.1:8000/v1"));

        CharacterModelBinding binding = OpenAiCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource,
            endpoint);

        Assert.Same(
            endpoint,
            Field<OpenAiEndpoint>(Assert.IsType<OpenAiCharacterModel>(binding.Model), "endpoint"));
        Assert.Same(
            endpoint,
            Field<OpenAiEndpoint>(
                Assert.IsType<OpenAiCharacterModelConnectionTester>(binding.ConnectionTester),
                "endpoint"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void CreatesAnExactDiagnosticsEnabledOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();
        StubTimeProvider timeProvider = new();
        ThrowingDiagnosticSink diagnosticSink = new();

        CharacterModelBinding binding = OpenAiCharacterModelBindingFactory.Create(
            settings,
            client,
            credentialSource,
            timeProvider,
            diagnosticSink);

        OpenAiCharacterModel model = Assert.IsType<OpenAiCharacterModel>(binding.Model);
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
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();
        StubTimeProvider timeProvider = new();
        ThrowingDiagnosticSink diagnosticSink = new();

        Assert.Throws<ArgumentNullException>(() => OpenAiCharacterModelBindingFactory.Create(
            null!, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() => OpenAiCharacterModelBindingFactory.Create(
            settings, null!, credentialSource));
        Assert.Throws<ArgumentNullException>(() => OpenAiCharacterModelBindingFactory.Create(
            settings, client, null!));
        Assert.Throws<ArgumentException>(() => OpenAiCharacterModelBindingFactory.Create(
            anthropic, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() => OpenAiCharacterModelBindingFactory.Create(
            settings, client, credentialSource, (OpenAiEndpoint)null!));
        Assert.Throws<ArgumentNullException>(() => OpenAiCharacterModelBindingFactory.Create(
            settings, client, credentialSource, null!, diagnosticSink));
        Assert.Throws<ArgumentNullException>(() => OpenAiCharacterModelBindingFactory.Create(
            settings, client, credentialSource, timeProvider, null!));
        Assert.Throws<ArgumentException>(() => OpenAiCharacterModelBindingFactory.Create(
            anthropic, client, credentialSource, timeProvider, diagnosticSink));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void ExposesOnlyTheFourExactCreationOverloads()
    {
        MethodInfo[] methods = typeof(OpenAiCharacterModelBindingFactory).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Collection(
            methods.OrderBy(method => method.GetParameters().Length),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(IOpenAiCredentialSource),
                ],
                method.GetParameters().Select(parameter => parameter.ParameterType)),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(IOpenAiCredentialSource),
                    typeof(OpenAiEndpoint),
                ],
                method.GetParameters().Select(parameter => parameter.ParameterType)),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(IOpenAiCredentialSource),
                    typeof(TimeProvider),
                    typeof(ICharacterModelGenerationDiagnosticSink),
                ],
                method.GetParameters().Select(parameter => parameter.ParameterType)),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(IOpenAiCredentialSource),
                    typeof(OpenAiEndpoint),
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

    private sealed class ThrowingCredentialSource : IOpenAiCredentialSource
    {
        public int LoadCount { get; private set; }

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
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
