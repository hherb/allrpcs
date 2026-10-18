using System.Reflection;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageDelayModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterTargetSabotageDelayModelBinding binding =
            OpenAiCharacterTargetSabotageDelayModelBindingFactory.Create(
                settings,
                client,
                credentialSource);

        Assert.Same(settings, binding.Settings);
        OpenAiCharacterTargetSabotageDelayModel model =
            Assert.IsType<OpenAiCharacterTargetSabotageDelayModel>(binding.Model);
        object transport = Field<object>(model, "client");
        Assert.Equal(typeof(OpenAiResponsesStructuredClient<,>), transport.GetType().GetGenericTypeDefinition());
        Assert.Same(client, Field<HttpClient>(transport, "client"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(transport, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(transport, "settings"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(transport, "endpoint"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void RejectsInvalidInputsAndExposesOnlyTwoCreationOverloads()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        using HttpClient client = new(new ThrowingHandler());
        ThrowingCredentialSource credentials = new();

        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayModelBindingFactory.Create(null!, client, credentials));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayModelBindingFactory.Create(settings, null!, credentials));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayModelBindingFactory.Create(settings, client, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterTargetSabotageDelayModelBindingFactory.Create(anthropic, client, credentials));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayModelBindingFactory.Create(
                settings,
                client,
                credentials,
                null!));
        MethodInfo[] methods = typeof(OpenAiCharacterTargetSabotageDelayModelBindingFactory)
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
            Assert.Equal(typeof(CharacterTargetSabotageDelayModelBinding), method.ReturnType);
        });
    }

    private static T Field<T>(object instance, string name) where T : class =>
        Assert.IsAssignableFrom<T>(instance.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance));

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        internal int SendCount { get; private set; }

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
        internal int LoadCount { get; private set; }

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(CancellationToken cancellationToken)
        {
            LoadCount++;
            throw new InvalidOperationException("Construction loaded a credential.");
        }
    }
}
