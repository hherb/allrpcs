using System.Reflection;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOllamaBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, "granite-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        OllamaEndpoint endpoint = new(new Uri("http://127.0.0.1:22445/"));

        CharacterModelBinding binding = OllamaCharacterModelBindingFactory.Create(
            settings,
            client,
            endpoint);

        Assert.Same(CharacterModelProvider.Ollama, binding.Provider);
        Assert.Same(settings, binding.Settings);
        OllamaCharacterModel model = Assert.IsType<OllamaCharacterModel>(binding.Model);
        OllamaCharacterModelConnectionTester tester =
            Assert.IsType<OllamaCharacterModelConnectionTester>(binding.ConnectionTester);
        Assert.Same(client, Field<HttpClient>(model, "client"));
        Assert.Same(client, Field<HttpClient>(tester, "client"));
        Assert.Same(endpoint, Field<OllamaEndpoint>(model, "endpoint"));
        Assert.Same(endpoint, Field<OllamaEndpoint>(tester, "endpoint"));
        Assert.Same(settings, Field<CharacterModelSettings>(model, "settings"));
        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public void CreatesAnExactDiagnosticsEnabledOllamaBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, "granite-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        OllamaEndpoint endpoint = new(new Uri("http://[::1]:22445/"));
        StubTimeProvider timeProvider = new();
        ThrowingDiagnosticSink diagnosticSink = new();

        CharacterModelBinding binding = OllamaCharacterModelBindingFactory.Create(
            settings,
            client,
            endpoint,
            timeProvider,
            diagnosticSink);

        OllamaCharacterModel model = Assert.IsType<OllamaCharacterModel>(binding.Model);
        Assert.Same(timeProvider, Field<TimeProvider>(model, "diagnosticTimeProvider"));
        Assert.Same(
            diagnosticSink,
            Field<ICharacterModelGenerationDiagnosticSink>(model, "diagnosticSink"));
        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public void RejectsInvalidInputsBeforeInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        OllamaEndpoint endpoint = new(new Uri("http://127.0.0.1:22445/"));
        StubTimeProvider timeProvider = new();
        ThrowingDiagnosticSink diagnosticSink = new();

        Assert.Throws<ArgumentNullException>(() => OllamaCharacterModelBindingFactory.Create(
            null!, client, endpoint));
        Assert.Throws<ArgumentNullException>(() => OllamaCharacterModelBindingFactory.Create(
            settings, null!, endpoint));
        Assert.Throws<ArgumentNullException>(() => OllamaCharacterModelBindingFactory.Create(
            settings, client, null!));
        Assert.Throws<ArgumentException>(() => OllamaCharacterModelBindingFactory.Create(
            openAi, client, endpoint));
        Assert.Throws<ArgumentNullException>(() => OllamaCharacterModelBindingFactory.Create(
            settings, client, endpoint, null!, diagnosticSink));
        Assert.Throws<ArgumentNullException>(() => OllamaCharacterModelBindingFactory.Create(
            settings, client, endpoint, timeProvider, null!));
        Assert.Throws<ArgumentException>(() => OllamaCharacterModelBindingFactory.Create(
            openAi, client, endpoint, timeProvider, diagnosticSink));
        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public void ExposesOnlyTheTwoExactCreationOverloads()
    {
        MethodInfo[] methods = typeof(OllamaCharacterModelBindingFactory).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Collection(
            methods.OrderBy(method => method.GetParameters().Length),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(OllamaEndpoint),
                ],
                method.GetParameters().Select(parameter => parameter.ParameterType)),
            method => Assert.Equal(
                [
                    typeof(CharacterModelSettings),
                    typeof(HttpClient),
                    typeof(OllamaEndpoint),
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

    private sealed class StubTimeProvider : TimeProvider;

    private sealed class ThrowingDiagnosticSink : ICharacterModelGenerationDiagnosticSink
    {
        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Construction recorded a diagnostic.");
    }
}
