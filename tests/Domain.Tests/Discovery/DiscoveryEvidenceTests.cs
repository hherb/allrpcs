using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryEvidenceTests
{
    [Fact]
    public void RetainsExactAuthoredValues()
    {
        EvidenceId evidenceId = new("healing-herb-appearance");
        ObjectId objectId = new("garden-herb");

        DiscoveryEvidence evidence = new(
            evidenceId,
            objectId,
            VisibilityState.Identified,
            isCritical: true);

        Assert.Same(evidenceId, evidence.EvidenceId);
        Assert.Same(objectId, evidence.ObjectId);
        Assert.Same(VisibilityState.Identified, evidence.MinimumVisibilityState);
        Assert.True(evidence.IsCritical);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        EvidenceId evidenceId = new("healing-herb-appearance");
        ObjectId objectId = new("garden-herb");

        Assert.Throws<ArgumentNullException>(() => new DiscoveryEvidence(
            null!, objectId, VisibilityState.Noticed, isCritical: false));
        Assert.Throws<ArgumentNullException>(() => new DiscoveryEvidence(
            evidenceId, null!, VisibilityState.Noticed, isCritical: false));
        Assert.Throws<ArgumentNullException>(() => new DiscoveryEvidence(
            evidenceId, objectId, null!, isCritical: false));
    }
}
