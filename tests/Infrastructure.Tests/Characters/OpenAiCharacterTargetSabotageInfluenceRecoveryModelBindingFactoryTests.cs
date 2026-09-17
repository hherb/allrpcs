using System.Reflection;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterTargetSabotageInfluenceRecoveryModelBinding binding =
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory.Create(
                settings,
                client,
                credentialSource);

        Assert.Same(settings, binding.Settings);
        OpenAiCharacterTargetSabotageInfluenceRecoveryModel model =
            Assert.IsType<OpenAiCharacterTargetSabotageInfluenceRecoveryModel>(binding.Model);
        Assert.Same(client, Field<HttpClient>(model, "client"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(model, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(model, "settings"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(model, "endpoint"));
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
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory.Create(
                null!,
                client,
                credentials));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory.Create(
                settings,
                null!,
                credentials));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory.Create(
                settings,
                client,
                null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory.Create(
                anthropic,
                client,
                credentials));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory.Create(
                settings,
                client,
                credentials,
                null!));
        MethodInfo[] methods = typeof(
            OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory)
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
            Assert.Equal(
                typeof(CharacterTargetSabotageInfluenceRecoveryModelBinding),
                method.ReturnType);
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
