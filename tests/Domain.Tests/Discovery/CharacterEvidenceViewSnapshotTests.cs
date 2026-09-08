using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class CharacterEvidenceViewSnapshotTests
{
    [Fact]
    public void RetainsExactInputsInStableReadOnlyOrder()
    {
        CharacterId characterId = new("seeker");
        EvidenceId first = new("first-evidence");
        EvidenceId second = new("second-evidence");
        EvidenceId[] source = [first, second];

        CharacterEvidenceViewSnapshot snapshot = new(characterId, source);
        source[0] = new EvidenceId("replacement-evidence");

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Collection(
            snapshot.EvidenceIds,
            evidenceId => Assert.Same(first, evidenceId),
            evidenceId => Assert.Same(second, evidenceId));
        Assert.Throws<NotSupportedException>(() =>
            Assert.IsAssignableFrom<IList<EvidenceId>>(snapshot.EvidenceIds).Add(first));
    }

    [Fact]
    public void RejectsNullAndDuplicateInputs()
    {
        CharacterId characterId = new("seeker");
        EvidenceId evidenceId = new("evidence");

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterEvidenceViewSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterEvidenceViewSnapshot(characterId, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterEvidenceViewSnapshot(characterId, [null!]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterEvidenceViewSnapshot(characterId, [evidenceId, evidenceId]));
    }
}
