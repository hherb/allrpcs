using System.Numerics;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class StreamWaterContactCommitTests
{
    [Fact]
    public void RequiresConsistentExactRequestAndNextSequence()
    {
        Character actor = StreamWaterContactRequestTests.Actor();
        StreamWaterContactSite site = StreamWaterContactRequestTests.Site();
        StreamWaterContactRequest request = new(actor, site, new(actor, site, 8));
        StreamWaterContactCommit commit = new(request, 9);
        Assert.Same(request, commit.Request); Assert.Equal(9, commit.Sequence);
        Assert.Throws<ArgumentException>(() => new StreamWaterContactCommit(request, 8));
        Assert.Throws<ArgumentException>(() => new StreamWaterContactCommit(new(actor, site, new(actor, site, long.MaxValue)), 0));
        Assert.Throws<ArgumentException>(() => new StreamWaterContactCommit(new(StreamWaterContactRequestTests.Actor(), site, request.State), 9));
        Assert.Throws<ArgumentException>(() => new StreamWaterContactCommit(new(actor, StreamWaterContactRequestTests.Site(), request.State), 9));
        Assert.Throws<ArgumentNullException>(() => new StreamWaterContactCommit(null!, 1));
    }
}
