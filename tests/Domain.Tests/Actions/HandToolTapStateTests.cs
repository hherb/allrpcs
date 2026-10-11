using System.Numerics;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class HandToolTapStateTests
{
    [Fact]
    public void BoundsStateAndSiteWithoutWorldObjectIdentity()
    {
        Character actor = HandToolTapRequestTests.Actor(); HandToolTapSite site = HandToolTapRequestTests.Site();
        HandToolTapState state = new(actor, site, 7, 1);
        Assert.Same(actor, state.Actor); Assert.Same(site, state.Site); Assert.Equal(7, state.Sequence); Assert.Equal(1, state.Cooldown);
        Assert.Throws<ArgumentOutOfRangeException>(() => new HandToolTapState(actor, site, -1));
        foreach (double time in new[] { -1, 1.1, double.NaN, double.PositiveInfinity })
        { Assert.Throws<ArgumentOutOfRangeException>(() => new HandToolTapState(actor, site, 0, time)); }
        Assert.Throws<ArgumentException>(() => new HandToolTapSite(" ", default));
        Assert.Throws<ArgumentException>(() => new HandToolTapSite("site", new(float.NaN, 0)));
        Assert.Throws<ArgumentNullException>(() => new HandToolTapState(null!, site));
        Assert.Throws<ArgumentNullException>(() => new HandToolTapState(actor, null!));
    }
}
