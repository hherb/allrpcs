using System.Reflection;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceModelBindingFactoryTests
{
    [Fact]
    public void CreatesAnExactOpenAiBindingWithoutInputOutput()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingHandler handler = new();
        using HttpClient client = new(handler);
        ThrowingCredentialSource credentialSource = new();

        CharacterDialogueMisquotationEvidenceModelBinding binding =
            CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
                settings,
                client,
                credentialSource);

        Assert.Same(settings, binding.Settings);
        CharacterDialogueMisquotationEvidenceModel model =
            Assert.IsType<CharacterDialogueMisquotationEvidenceModel>(binding.Model);
        object transport = Field<object>(model, "client");
        Assert.Equal(typeof(OpenAiResponsesStructuredClient<,>), transport.GetType().GetGenericTypeDefinition());
        Assert.Same(client, Field<HttpClient>(transport, "client"));
        Assert.Same(credentialSource, Field<IOpenAiCredentialSource>(transport, "credentialSource"));
        Assert.Same(settings, Field<CharacterModelSettings>(transport, "settings"));
        Assert.Same(OpenAiEndpoint.Default, Field<OpenAiEndpoint>(transport, "endpoint"));
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
        CharacterDialogueMisquotationEvidenceConsiderationRequest request =
            CharacterDialogueMisquotationEvidenceOpenAiRequestTests.CreateRequest();
        RespondingHandler handler = new(request);
        using HttpClient client = new(handler);
        LoadedCredentialSource credentialSource = new();

        CharacterDialogueMisquotationEvidenceModelBinding binding = baseUri is null
            ? CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
                settings,
                client,
                credentialSource)
            : CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
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
            CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
                null!, client, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
                settings, null!, credentialSource));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
                settings, client, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceModelBindingFactory.Create(
                anthropic, client, credentialSource));
        Assert.Equal(0, handler.SendCount);
        Assert.Equal(0, credentialSource.LoadCount);
    }

    [Fact]
    public void ExposesOneProviderNeutralCreationMethod()
    {
        MethodInfo method = Assert.Single(typeof(CharacterDialogueMisquotationEvidenceModelBindingFactory).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal("Create", method.Name);
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
        CharacterDialogueMisquotationEvidenceConsiderationRequest request) : HttpMessageHandler
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
                CharacterDialogueMisquotationEvidenceOpenAiStreamTests.DecisionDocument(
                    request.Actor.Identity.Id.Value,
                    0);
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
