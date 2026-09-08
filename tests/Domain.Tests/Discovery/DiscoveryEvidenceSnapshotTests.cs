using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryEvidenceSnapshotTests
{
    [Fact]
    public void CopiesStableOrderAndRetainsExactEntries()
    {
        DiscoveryEvidence first = Evidence("visible-herb", "garden-herb");
        DiscoveryEvidence second = Evidence("concealed-herb", "woodland-herb");
        DiscoveryEvidence[] source = [first, second];

        DiscoveryEvidenceSnapshot snapshot = new(source);
        source[0] = Evidence("replacement", "replacement-object");

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Throws<NotSupportedException>(() =>
            Assert.IsAssignableFrom<IList<DiscoveryEvidence>>(snapshot.Entries).Add(first));
    }

    [Fact]
    public void RejectsDuplicateEvidenceIdentifiersNullEntriesAndNullSequence()
    {
        DiscoveryEvidence first = Evidence("visible-herb", "garden-herb");
        DiscoveryEvidence duplicate = Evidence("visible-herb", "woodland-herb");

        Assert.Throws<ArgumentException>(() => new DiscoveryEvidenceSnapshot([first, duplicate]));
        Assert.Throws<ArgumentException>(() => new DiscoveryEvidenceSnapshot([null!]));
        Assert.Throws<ArgumentNullException>(() => new DiscoveryEvidenceSnapshot(null!));
    }

    private static DiscoveryEvidence Evidence(string evidenceId, string objectId) => new(
        new EvidenceId(evidenceId),
        new ObjectId(objectId),
        VisibilityState.Identified,
        isCritical: false);
}
