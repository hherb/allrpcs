using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterCommitmentMemorySnapshotTests
{
    [Fact]
    public void PreservesOrderExactInstancesAndCopiesCollection()
    {
        CharacterId characterId = new("healer");
        CharacterCommitmentMemory first = Commitment(characterId, "first");
        CharacterCommitmentMemory second = Commitment(characterId, "second");
        CharacterCommitmentMemory[] source = [first, second];

        CharacterCommitmentMemorySnapshot snapshot = new(characterId, source);
        source[0] = second;

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Collection(
            snapshot.Commitments,
            commitment => Assert.Same(first, commitment),
            commitment => Assert.Same(second, commitment));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterCommitmentMemory>)snapshot.Commitments).Add(first));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterId characterId = new("healer");
        CharacterCommitmentMemory[] maximum = Enumerable
            .Range(0, CharacterCommitmentMemorySnapshot.MaximumCommitmentCount)
            .Select(index => Commitment(characterId, $"commitment-{index}"))
            .ToArray();

        Assert.Empty(new CharacterCommitmentMemorySnapshot(characterId, []).Commitments);
        Assert.Equal(
            CharacterCommitmentMemorySnapshot.MaximumCommitmentCount,
            new CharacterCommitmentMemorySnapshot(characterId, maximum).Commitments.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterCommitmentMemorySnapshot(
            characterId,
            maximum.Append(Commitment(characterId, "extra"))));
    }

    [Fact]
    public void RejectsInvalidInputs()
    {
        CharacterId characterId = new("healer");
        CharacterCommitmentMemory first = Commitment(characterId, "first");
        CharacterCommitmentMemory duplicate = Commitment(characterId, "first");
        CharacterCommitmentMemory other = Commitment(new CharacterId("hermit"), "other");

        Assert.Throws<ArgumentException>(() => new CharacterCommitmentMemorySnapshot(
            characterId,
            [first, duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterCommitmentMemorySnapshot(
            characterId,
            [other]));
        Assert.Throws<ArgumentException>(() => new CharacterCommitmentMemorySnapshot(
            characterId,
            [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemorySnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterCommitmentMemorySnapshot(
            characterId,
            null!));
    }

    private static CharacterCommitmentMemory Commitment(
        CharacterId characterId,
        string commitmentId) =>
        CharacterCommitmentMemoryTests.Commitment(
            characterId,
            commitmentId: commitmentId);
}
