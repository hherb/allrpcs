using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class RememberedSeekerProfileJsonTests
{
    [Fact]
    public void RoundTripsTheProfileInTheSaveDocumentShape()
    {
        string serialized = RememberedSeekerProfileJson.Serialize(Profile());

        Assert.Equal(
            "{\"schemaVersion\":1,\"seekerProfile\":{\"displayName\":\"Mara\",\"subject\":\"she\",\"objective\":\"her\","
            + "\"possessiveDeterminer\":\"her\",\"possessivePronoun\":\"hers\",\"reflexive\":\"herself\","
            + "\"appearanceKey\":\"weathered-traveller\",\"backgroundKey\":\"forester\"}}",
            serialized);
        Assert.Equal(Profile(), RememberedSeekerProfileJson.Deserialize(serialized));
    }

    [Theory]
    [InlineData("\"schemaVersion\":1", "\"schemaVersion\":2")]
    [InlineData("\"schemaVersion\":1", "\"schemaVersion\":\"1\"")]
    [InlineData("\"backgroundKey\":\"forester\"}", "\"backgroundKey\":\"forester\",\"extra\":1}")]
    [InlineData("\"displayName\":\"Mara\",", "")]
    [InlineData("\"appearanceKey\":\"weathered-traveller\"", "\"appearanceKey\":\"Weathered Traveller\"")]
    [InlineData("{\"schemaVersion\":1,", "{")]
    public void RejectsAnythingButOneExactProfileDocument(string original, string replacement)
    {
        string valid = RememberedSeekerProfileJson.Serialize(Profile());
        string malformed = valid.Replace(original, replacement, StringComparison.Ordinal);

        Assert.NotEqual(valid, malformed);
        Assert.Throws<ArgumentException>(() => RememberedSeekerProfileJson.Deserialize(malformed));
    }

    [Fact]
    public void RejectsMalformedJson()
    {
        Assert.Throws<ArgumentException>(() => RememberedSeekerProfileJson.Deserialize("{"));
    }

    private static SeekerCustomizationSaveState Profile() => new(
        new CharacterIdentity(new CharacterId("seeker"), "Mara"),
        new SeekerPronouns("she", "her", "her", "hers", "herself"),
        "weathered-traveller",
        "forester");
}
