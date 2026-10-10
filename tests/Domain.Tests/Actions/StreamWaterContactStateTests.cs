using System.Numerics;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class StreamWaterContactStateTests
{
    [Fact]
    public void BoundsStateAndSiteWithoutWorldObjectIdentity()
    {
        Character actor = StreamWaterContactRequestTests.Actor(); StreamWaterContactSite site = StreamWaterContactRequestTests.Site();
        StreamWaterContactState state = new(actor, site, 7, 2);
        Assert.Same(actor, state.Actor); Assert.Same(site, state.Site); Assert.Equal(7, state.Sequence); Assert.Equal(2, state.Cooldown);
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWaterContactState(actor, site, -1));
        foreach (double time in new[] { -1, 2.1, double.NaN, double.PositiveInfinity })
        { Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWaterContactState(actor, site, 0, time)); }
        Assert.Throws<ArgumentException>(() => new StreamWaterContactSite(" ", default));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWaterContactSite("site", new(float.NaN, 0)));
        Assert.Throws<ArgumentNullException>(() => new StreamWaterContactState(null!, site));
        Assert.Throws<ArgumentNullException>(() => new StreamWaterContactState(actor, null!));
    }
}
