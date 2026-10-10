using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class CaskCoverHandlingCommitTests
{
    [Fact]
    public void RequiresExactConsistentRequestAndNextSequence()
    {
        Character actor = RepresentativeSageCharacterSource.Create(); var material = CaskCoverHandlingRequestTests.Material();
        CaskCoverHandlingState state = new(actor, material, 7); CaskCoverHandlingRequest request = new(actor, material, state);
        CaskCoverHandlingCommit commit = new(request, 8); Assert.Same(request, commit.Request); Assert.Equal(8, commit.Sequence);
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingCommit(null!, 8));
        Assert.Throws<ArgumentException>(() => new CaskCoverHandlingCommit(request, 7));
        Assert.Throws<ArgumentException>(() => new CaskCoverHandlingCommit(new(RepresentativeSageCharacterSource.Create(), material, state), 8));
        Assert.Throws<ArgumentException>(() => new CaskCoverHandlingCommit(new(actor, CaskCoverHandlingRequestTests.Material(), state), 8));
        Assert.Throws<ArgumentException>(() => new CaskCoverHandlingCommit(new(actor, material, new(actor, material, long.MaxValue)), 1));
    }
}
