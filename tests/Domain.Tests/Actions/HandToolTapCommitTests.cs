using System.Numerics;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class HandToolTapCommitTests
{
    [Fact]
    public void RequiresConsistentExactRequestAndNextSequence()
    {
        Character actor = HandToolTapRequestTests.Actor();
        HandToolTapSite site = HandToolTapRequestTests.Site();
        HandToolTapRequest request = new(actor, site, new(actor, site, 8));
        HandToolTapCommit commit = new(request, 9);
        Assert.Same(request, commit.Request); Assert.Equal(9, commit.Sequence);
        Assert.Throws<ArgumentException>(() => new HandToolTapCommit(request, 8));
        Assert.Throws<ArgumentException>(() => new HandToolTapCommit(new(actor, site, new(actor, site, long.MaxValue)), 0));
        Assert.Throws<ArgumentException>(() => new HandToolTapCommit(new(HandToolTapRequestTests.Actor(), site, request.State), 9));
        Assert.Throws<ArgumentException>(() => new HandToolTapCommit(new(actor, HandToolTapRequestTests.Site(), request.State), 9));
        Assert.Throws<ArgumentNullException>(() => new HandToolTapCommit(null!, 1));
    }
}
