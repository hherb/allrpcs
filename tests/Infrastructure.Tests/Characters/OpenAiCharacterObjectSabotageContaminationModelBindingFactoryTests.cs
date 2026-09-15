using System.Reflection;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageContaminationModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterObjectSabotageContaminationModelBinding binding =
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                settings,
                client,
                credentialSource);

        Assert.Same(settings, binding.Settings);
        OpenAiCharacterObjectSabotageContaminationModel model =
            Assert.IsType<OpenAiCharacterObjectSabotageContaminationModel>(binding.Model);
        Assert.Same(client, Field<HttpClient>(model, "client"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(model, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(model, "settings"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(model, "endpoint"));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Theory]
    [InlineData(null, "https://api.openai.com/v1/responses")]
    [InlineData("http://127.0.0.1:8000/v1", "http://127.0.0.1:8000/v1/responses")]
    public async Task UsesTheExpectedEndpointOnlyOnLaterExplicitInvocation(
        string? baseUri,
        string expectedRequestUri)
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "local-test");
        CharacterObjectSabotageContaminationConsiderationRequest request =
            OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest();
        RespondingHandler handler = new(request);
        using HttpClient client = new(handler);
        LoadedCredentialSource credentialSource = new();

        CharacterObjectSabotageContaminationModelBinding binding = baseUri is null
            ? OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                settings,
                client,
                credentialSource)
            : OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                settings,
                client,
                credentialSource,
                new OpenAiEndpoint(new Uri(baseUri)));

        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);

        await binding.Model.DecideAsync(request, CancellationToken.None);

        Assert.Equal(expectedRequestUri, handler.RequestUri?.AbsoluteUri);
        Assert.Equal(1, handler.SendCount);
        Assert.Equal(1, credentialSource.LoadCount);
    }

    [Fact]
    public void RejectsInvalidInputsBeforeInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                null!, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                settings, null!, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                settings, client, null!));
        Assert.Throws<ArgumentException>(() =>
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                anthropic, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            OpenAiCharacterObjectSabotageContaminationModelBindingFactory.Create(
                settings, client, credentialSource, null!));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void ExposesOnlyTheTwoExactCreationOverloads()
    {
        MethodInfo[] methods = typeof(OpenAiCharacterObjectSabotageContaminationModelBindingFactory)
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
            Assert.Equal(typeof(CharacterObjectSabotageContaminationModelBinding), method.ReturnType);
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

    private sealed class LoadedCredentialSource : IOpenAiCredentialSource
    {
        internal int LoadCount { get; private set; }

        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            LoadCount++;
            return ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("test-secret"));
        }
    }

    private sealed class RespondingHandler(
        CharacterObjectSabotageContaminationConsiderationRequest request) : HttpMessageHandler
    {
        internal Uri? RequestUri { get; private set; }

        internal int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage message,
            CancellationToken cancellationToken)
        {
            SendCount++;
            RequestUri = message.RequestUri;
            string document =
                OpenAiCharacterObjectSabotageContaminationStreamAccumulatorTests.DecisionDocument(
                    request.Character.Identity.Id.Value,
                    request.Candidates[0].Evidence.EvidenceId.Value);
            string body = Frame(new { type = "response.created" })
                + Frame(new { type = "response.in_progress" })
                + Frame(new { type = "response.output_text.delta", delta = document })
                + Frame(new { type = "response.output_text.done", text = document })
                + Frame(new { type = "response.completed" });
            HttpResponseMessage response = new()
            {
                Content = new StringContent(body, Encoding.UTF8, "text/event-stream"),
            };
            return Task.FromResult(response);
        }

        private static string Frame(object semanticEvent) =>
            "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";
    }
}
