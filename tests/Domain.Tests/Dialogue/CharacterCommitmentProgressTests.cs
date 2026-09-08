using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterCommitmentProgressTests
{
    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(1, false, 1)]
    [InlineData(2, true, null)]
    public void RetainsExactCommitmentAndExposesOnlyTheNextIntention(
        int nextIntentionIndex,
        bool expectedComplete,
        int? expectedIntentionIndex)
    {
        CharacterIntention first = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("flowers"));
        CharacterIntention second = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("flowers"));
        CharacterCommitment commitment = new([first, second]);

        CharacterCommitmentProgress progress = new(commitment, nextIntentionIndex);

        Assert.Same(commitment, progress.Commitment);
        Assert.Equal(nextIntentionIndex, progress.NextIntentionIndex);
        Assert.Equal(expectedComplete, progress.IsComplete);
        if (expectedIntentionIndex is int index)
        {
            Assert.Same(commitment.ProposedIntentions[index], progress.NextIntention);
        }
        else
        {
            Assert.Null(progress.NextIntention);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void RejectsAnIndexOutsideTheCommitmentBounds(int nextIntentionIndex)
    {
        CharacterCommitment commitment = new(
        [
            new CharacterIntention(CharacterIntentionType.Wait, null),
            new CharacterIntention(CharacterIntentionType.Wait, null),
        ]);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterCommitmentProgress(commitment, nextIntentionIndex));
    }

    [Fact]
    public void RejectsANullCommitment()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentProgress(null!, 0));
    }

    [Fact]
    public void ExposesOnlyImmutableProgressState()
    {
        Type progressType = typeof(CharacterCommitmentProgress);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Commitment", typeof(CharacterCommitment)),
            ("IsComplete", typeof(bool)),
            ("NextIntention", typeof(CharacterIntention)),
            ("NextIntentionIndex", typeof(int)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = progressType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(progressType.GetEvents());
        Assert.True(progressType.IsSealed);
    }
}
