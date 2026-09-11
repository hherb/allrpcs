using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Identifiers;

public sealed class DomainIdentifierTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("healer")]
    [InlineData("village-square-2")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456789-abcdefghijklmnopqrstuvwxyza")]
    public void ConstructsEveryIdentifierFromCanonicalText(string value)
    {
        Assert.Equal(value, new CharacterId(value).Value);
        Assert.Equal(value, new ObjectId(value).Value);
        Assert.Equal(value, new LocationId(value).Value);
        Assert.Equal(value, new EvidenceId(value).Value);
        Assert.Equal(value, new KnowledgeId(value).Value);
        Assert.Equal(value, new WorldEventId(value).Value);
        Assert.Equal(value, new CharacterTurnId(value).Value);
        Assert.Equal(value, new EpisodicMemoryId(value).Value);
        Assert.Equal(value, new EpistemicMemoryId(value).Value);
        Assert.Equal(value, new RelationshipReasonId(value).Value);
        Assert.Equal(value, new CharacterCommitmentId(value).Value);
        Assert.Equal(value, new CharacterConversationSummaryId(value).Value);
        Assert.Equal(value, new DiseaseId(value).Value);
        Assert.Equal(value, new IngredientId(value).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Healer")]
    [InlineData("2-healer")]
    [InlineData("-healer")]
    [InlineData("healer-")]
    [InlineData("village--square")]
    [InlineData("village_square")]
    [InlineData("hérisson")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456789-abcdefghijklmnopqrstuvwxyz12")]
    public void RejectsNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => new CharacterId(value));
        Assert.Throws<ArgumentException>(() => new ObjectId(value));
        Assert.Throws<ArgumentException>(() => new LocationId(value));
        Assert.Throws<ArgumentException>(() => new EvidenceId(value));
        Assert.Throws<ArgumentException>(() => new KnowledgeId(value));
        Assert.Throws<ArgumentException>(() => new WorldEventId(value));
        Assert.Throws<ArgumentException>(() => new CharacterTurnId(value));
        Assert.Throws<ArgumentException>(() => new EpisodicMemoryId(value));
        Assert.Throws<ArgumentException>(() => new EpistemicMemoryId(value));
        Assert.Throws<ArgumentException>(() => new RelationshipReasonId(value));
        Assert.Throws<ArgumentException>(() => new CharacterCommitmentId(value));
        Assert.Throws<ArgumentException>(() => new CharacterConversationSummaryId(value));
        Assert.Throws<ArgumentException>(() => new DiseaseId(value));
        Assert.Throws<ArgumentException>(() => new IngredientId(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterId(null!));
        Assert.Throws<ArgumentNullException>(() => new ObjectId(null!));
        Assert.Throws<ArgumentNullException>(() => new LocationId(null!));
        Assert.Throws<ArgumentNullException>(() => new EvidenceId(null!));
        Assert.Throws<ArgumentNullException>(() => new KnowledgeId(null!));
        Assert.Throws<ArgumentNullException>(() => new WorldEventId(null!));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnId(null!));
        Assert.Throws<ArgumentNullException>(() => new EpisodicMemoryId(null!));
        Assert.Throws<ArgumentNullException>(() => new EpistemicMemoryId(null!));
        Assert.Throws<ArgumentNullException>(() => new RelationshipReasonId(null!));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentId(null!));
        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummaryId(null!));
        Assert.Throws<ArgumentNullException>(() => new DiseaseId(null!));
        Assert.Throws<ArgumentNullException>(() => new IngredientId(null!));
    }

    [Fact]
    public void EqualityRequiresMatchingTypeAndCanonicalText()
    {
        CharacterId firstCharacter = new("healer");
        CharacterId secondCharacter = new("healer");
        CharacterId differentCharacter = new("seeker");
        ObjectId sameTextObject = new("healer");

        Assert.Equal(firstCharacter, secondCharacter);
        Assert.True(firstCharacter == secondCharacter);
        Assert.NotEqual(firstCharacter, differentCharacter);
        Assert.False(firstCharacter.Equals(sameTextObject));
    }

    [Fact]
    public void CanonicalTextRoundTripsForEveryIdentifierType()
    {
        AssertRoundTrip(new CharacterId("healer-2"), CharacterId.Parse);
        AssertRoundTrip(new ObjectId("healing-herb"), ObjectId.Parse);
        AssertRoundTrip(new LocationId("village-square"), LocationId.Parse);
        AssertRoundTrip(new EvidenceId("laundry-residue"), EvidenceId.Parse);
        AssertRoundTrip(new KnowledgeId("stream-contamination"), KnowledgeId.Parse);
        AssertRoundTrip(new WorldEventId("patient-stabilized"), WorldEventId.Parse);
        AssertRoundTrip(new CharacterTurnId("healer-turn-2"), CharacterTurnId.Parse);
        AssertRoundTrip(new EpisodicMemoryId("moonwort-warning"), EpisodicMemoryId.Parse);
        AssertRoundTrip(new EpistemicMemoryId("moonwort-cooling"), EpistemicMemoryId.Parse);
        AssertRoundTrip(new RelationshipReasonId("kept-promise"), RelationshipReasonId.Parse);
        AssertRoundTrip(new CharacterCommitmentId("examine-herbs"), CharacterCommitmentId.Parse);
        AssertRoundTrip(
            new CharacterConversationSummaryId("earlier-moonwort-talk"),
            CharacterConversationSummaryId.Parse);
        AssertRoundTrip(new DiseaseId("grey-sleep"), DiseaseId.Parse);
        AssertRoundTrip(new IngredientId("moonwort-leaf"), IngredientId.Parse);
    }

    private static void AssertRoundTrip<TIdentifier>(
        TIdentifier identifier,
        Func<string, TIdentifier> parse)
    {
        string canonicalText = identifier?.ToString()
            ?? throw new InvalidOperationException("Identifier text cannot be null.");

        Assert.Equal(identifier, parse(canonicalText));
    }
}
