using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefAssessmentSnapshotTests
{
    [Fact]
    public void EmptySnapshotRetainsExactCharacter()
    {
        CharacterId characterId = new("healer");

        CharacterBeliefAssessmentSnapshot snapshot = new(characterId, []);

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Empty(snapshot.Assessments);
    }

    [Fact]
    public void PreservesOrderExactInstancesAndCopiesCollection()
    {
        CharacterId characterId = new("healer");
        CharacterBeliefAssessment first = Assessment(characterId, "first");
        CharacterBeliefAssessment second = Assessment(characterId, "second");
        CharacterBeliefAssessment[] source = [first, second];

        CharacterBeliefAssessmentSnapshot snapshot = new(characterId, source);
        source[0] = second;

        Assert.Collection(
            snapshot.Assessments,
            assessment => Assert.Same(first, assessment),
            assessment => Assert.Same(second, assessment));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterBeliefAssessment>)snapshot.Assessments).Add(first));
    }

    [Fact]
    public void RejectsDuplicateBeliefsMismatchedCharactersAndNulls()
    {
        CharacterId characterId = new("healer");
        CharacterBeliefAssessment[] maximum = Enumerable
            .Range(0, CharacterEpistemicMemorySnapshot.MaximumMemoryCount)
            .Select(index => Assessment(characterId, $"belief-{index}"))
            .ToArray();
        CharacterBeliefAssessment first = Assessment(characterId, "first");
        CharacterBeliefAssessment duplicate = Assessment(characterId, "first");
        CharacterBeliefAssessment other = Assessment(new CharacterId("hermit"), "other");

        Assert.Equal(
            CharacterEpistemicMemorySnapshot.MaximumMemoryCount,
            new CharacterBeliefAssessmentSnapshot(characterId, maximum).Assessments.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterBeliefAssessmentSnapshot(
            characterId,
            maximum.Append(Assessment(characterId, "extra"))));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefAssessmentSnapshot(
            characterId,
            [first, duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefAssessmentSnapshot(
            characterId,
            [other]));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefAssessmentSnapshot(
            characterId,
            [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefAssessmentSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefAssessmentSnapshot(
            characterId,
            null!));
    }

    private static CharacterBeliefAssessment Assessment(
        CharacterId characterId,
        string memoryId) =>
        new(CharacterBeliefAssessmentTests.Belief(characterId, memoryId), 500, [], []);
}
