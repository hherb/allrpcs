using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCredentialStoreContractTests
{
    [Fact]
    public void LoadResultsExposeOnlyBoundedRedactedStates()
    {
        OpenAiCredentialLoadResult loaded = OpenAiCredentialLoadResult.Loaded("test-secret");
        OpenAiCredentialLoadResult missing = OpenAiCredentialLoadResult.Missing();
        OpenAiCredentialLoadResult failed = OpenAiCredentialLoadResult.Failed();

        Assert.Equal(OpenAiCredentialLoadStatus.Loaded, loaded.Status);
        Assert.Equal("test-secret", loaded.ApiKey);
        Assert.Equal(OpenAiCredentialLoadStatus.Missing, missing.Status);
        Assert.Null(missing.ApiKey);
        Assert.Equal(OpenAiCredentialLoadStatus.Failed, failed.Status);
        Assert.Null(failed.ApiKey);
        Assert.Equal("Loaded", loaded.ToString());
        Assert.Equal("Missing", missing.ToString());
        Assert.Equal("Failed", failed.ToString());
        Assert.DoesNotContain("secret", loaded.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" key with spaces ")]
    [InlineData("key\0value")]
    public void LoadedResultRejectsInvalidKeys(string apiKey)
    {
        Assert.Throws<ArgumentException>(() => OpenAiCredentialLoadResult.Loaded(apiKey));
    }

    [Fact]
    public void LoadedResultRejectsNullAndExcessiveKeys()
    {
        Assert.Throws<ArgumentNullException>(() => OpenAiCredentialLoadResult.Loaded(null!));
        Assert.Throws<ArgumentException>(() => OpenAiCredentialLoadResult.Loaded(
            new string('k', OpenAiCredentialPolicy.MaximumApiKeyLength + 1)));
    }

    [Fact]
    public void StoreReusesCredentialSourceLoadContract()
    {
        Assert.Contains(typeof(IOpenAiCredentialSource), typeof(IOpenAiCredentialStore).GetInterfaces());
        Assert.Equal(
            typeof(ValueTask<OpenAiCredentialLoadResult>),
            typeof(IOpenAiCredentialSource).GetMethod(nameof(IOpenAiCredentialSource.LoadApiKeyAsync))!
                .ReturnType);
    }
}
