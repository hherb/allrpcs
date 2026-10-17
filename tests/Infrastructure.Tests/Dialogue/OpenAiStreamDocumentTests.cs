using System.Text.Json;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiStreamDocumentTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(64)]
    public void AcceptsSchemaEchoesThroughTheDepthBound(int depth)
    {
        using JsonDocument document = OpenAiStreamDocument.Parse(OpenAiSchemaEcho.Event("response.created", depth));

        Assert.Equal("response.created", document.RootElement.GetProperty("type").GetString());
    }

    [Theory]
    [MemberData(nameof(OpenAiSchemaEcho.InvalidEvents), MemberType = typeof(OpenAiSchemaEcho))]
    [InlineData("not-json")]
    [InlineData("{\"type\":")]
    public void RejectsExcessiveDepthAndNonStrictJson(string serializedEvent)
    {
        Assert.ThrowsAny<JsonException>(() => OpenAiStreamDocument.Parse(serializedEvent));
    }

    [Fact]
    public void RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => OpenAiStreamDocument.Parse(null!));
    }
}
