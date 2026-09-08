using System.Reflection;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterCommitmentTests
{
    [Fact]
    public void RetainsExactProposedIntentionsInDeclaredOrder()
    {
        CharacterIntention examine = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("moonwort"));
        CharacterIntention wait = new(CharacterIntentionType.Wait, null);
        CharacterIntention collect = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("moonwort"));

        CharacterCommitment commitment = new([examine, wait, collect]);

        Assert.Collection(
            commitment.ProposedIntentions,
            intention => Assert.Same(examine, intention),
            intention => Assert.Same(wait, intention),
            intention => Assert.Same(collect, intention));
    }

    [Fact]
    public void CopiesTheCallerCollection()
    {
        CharacterIntention first = new(CharacterIntentionType.Wait, null);
        CharacterIntention replacement = new(CharacterIntentionType.Wait, null);
        List<CharacterIntention> intentions = [first];
        CharacterCommitment commitment = new(intentions);

        intentions[0] = replacement;
        intentions.Add(replacement);

        Assert.Single(commitment.ProposedIntentions);
        Assert.Same(first, commitment.ProposedIntentions[0]);
    }

    [Fact]
    public void AllowsRepeatedProposedIntentions()
    {
        CharacterIntention wait = new(CharacterIntentionType.Wait, null);

        CharacterCommitment commitment = new([wait, wait]);

        Assert.Equal(2, commitment.ProposedIntentions.Count);
        Assert.All(commitment.ProposedIntentions, intention => Assert.Same(wait, intention));
    }

    [Fact]
    public void RejectsNullEmptyOrNullContainingInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitment(null!));
        Assert.Throws<ArgumentException>(() => new CharacterCommitment([]));
        Assert.Throws<ArgumentException>(() => new CharacterCommitment(
            [new CharacterIntention(CharacterIntentionType.Wait, null), null!]));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDeclarationSurface()
    {
        Type commitmentType = typeof(CharacterCommitment);
        PropertyInfo[] properties = commitmentType.GetProperties();

        PropertyInfo property = Assert.Single(properties);
        Assert.Equal("ProposedIntentions", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterIntention>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Empty(commitmentType.GetEvents());
        Assert.True(commitmentType.IsSealed);
    }
}
