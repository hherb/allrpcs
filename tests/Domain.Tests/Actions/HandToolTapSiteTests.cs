using System.Numerics;
using Allrpcs.Domain.Actions;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class HandToolTapSiteTests
{
    [Fact]
    public void DeclaresOnlyApprovedEnvironmentalMaterialAndPoint()
    {
        HandToolTapSite site = new(HandToolTapSite.SiteKey, new(1700, 900));
        Assert.Equal("Communal wooden mallet and wooden practice block", site.Description);
        Assert.Equal(new Vector2(1700, 900), site.Position);
        Assert.Throws<ArgumentException>(() => new HandToolTapSite("other", site.Position));
        foreach (Vector2 position in new[] { new Vector2(1701, 900), new(float.NaN, 900), new(1700, float.PositiveInfinity) })
        { Assert.Throws<ArgumentException>(() => new HandToolTapSite(HandToolTapSite.SiteKey, position)); }
        Assert.Throws<ArgumentNullException>(() => new HandToolTapSite(null!, site.Position));
    }
}
