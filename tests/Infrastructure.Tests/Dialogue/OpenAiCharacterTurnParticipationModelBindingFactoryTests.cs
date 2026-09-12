using System.Reflection;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterTurnParticipationModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiParticipationBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterTurnParticipationModelBinding binding =
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                settings,
                client,
                credentialSource);

        Assert.Same(CharacterModelProvider.OpenAi, binding.Provider);
        Assert.Same(settings, binding.Settings);
        OpenAiCharacterTurnParticipationModel model =
            Assert.IsType<OpenAiCharacterTurnParticipationModel>(binding.Model);
        Assert.Same(client, Field<HttpClient>(model, "client"));
        Assert.Same(
            credentialSource,
            Field<IOpenAiCredentialSource>(model, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(model, "settings"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(model, "endpoint"));
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

        CharacterTurnParticipationModelBinding binding =
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                settings,
                client,
                credentialSource,
                endpoint);

        Assert.Same(
            endpoint,
            Field<OpenAiEndpoint>(
                Assert.IsType<OpenAiCharacterTurnParticipationModel>(binding.Model),
                "endpoint"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void RejectsInvalidInputsBeforeInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                null!, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                settings, null!, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                settings, client, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                anthropic, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTurnParticipationModelBindingFactory.Create(
                settings, client, credentialSource, null!));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void ExposesOnlyTheTwoExactCreationOverloads()
    {
        MethodInfo[] methods = typeof(OpenAiCharacterTurnParticipationModelBindingFactory)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

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
                method.GetParameters().Select(parameter => parameter.ParameterType)));
        Assert.All(methods, method =>
        {
            Assert.Equal("Create", method.Name);
            Assert.Equal(typeof(CharacterTurnParticipationModelBinding), method.ReturnType);
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
}
