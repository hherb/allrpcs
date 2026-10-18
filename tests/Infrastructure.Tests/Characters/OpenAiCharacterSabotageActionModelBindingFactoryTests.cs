using System.Reflection;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterSabotageActionModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterSabotageActionModelBinding binding =
            CharacterSabotageActionModelBindingFactory.Create(
                settings,
                client,
                credentialSource);

        Assert.Same(settings, binding.Settings);
        CharacterSabotageActionModel model =
            Assert.IsType<CharacterSabotageActionModel>(binding.Model);
        object transport = Field<object>(model, "client");
        Assert.Equal(typeof(OpenAiResponsesStructuredClient<,>), transport.GetType().GetGenericTypeDefinition());
        Assert.Same(client, Field<HttpClient>(transport, "client"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(transport, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(transport, "settings"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void RejectsInvalidInputsAndExposesOneProviderNeutralCreationMethod()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        using HttpClient client = new(new ThrowingHandler());
        ThrowingCredentialSource credentials = new();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionModelBindingFactory.Create(null!, client, credentials));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionModelBindingFactory.Create(settings, null!, credentials));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionModelBindingFactory.Create(settings, client, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionModelBindingFactory.Create(anthropic, client, credentials));
        MethodInfo[] methods = typeof(CharacterSabotageActionModelBindingFactory)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Single(methods);
        Assert.All(methods, method =>
        {
            Assert.Equal("Create", method.Name);
            Assert.Equal(typeof(CharacterSabotageActionModelBinding), method.ReturnType);
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

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            LoadCount++;
            throw new InvalidOperationException("Construction loaded a credential.");
        }
    }
}
