using System.Numerics;
using Allrpcs.Application.Actions;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Application.Tests.Actions;

public sealed class StreamWaterContactAccessPolicyTests
{
    [Theory]
    [InlineData(1756, 484, true)]
    [InlineData(1766, 484, true)]
    [InlineData(1766.01, 484, false)]
    [InlineData(1748, 464, true)]
    [InlineData(1747.99, 484, false)]
    [InlineData(1756, 463.99, false)]
    [InlineData(1800, 500, false)]
    [InlineData(1734, 484, false)]
    [InlineData(float.NaN, 484, false)]
    [InlineData(float.PositiveInfinity, 484, false)]
    [InlineData(1756, float.NaN, false)]
    public void RequiresInclusiveDryFootprintAnd32PixelReach(float x, float y, bool expected)
    {
        Assert.Equal(expected, StreamWaterContactAccessPolicy.CanReach(new(x, y), true,
            new(StreamWaterContactAccessPolicy.SiteKey, new(1734, 484)), new(1736, 452), new(1864, 548)));
    }
    [Fact]
    public void UnsupportedAndForeignGeometryFailClosed()
    {
        Vector2 point = new(1756, 484); StreamWaterContactSite site = new(StreamWaterContactAccessPolicy.SiteKey, new(1734, 484));
        Assert.False(StreamWaterContactAccessPolicy.CanReach(point, false, site, new(1736, 452), new(1864, 548)));
        Assert.False(StreamWaterContactAccessPolicy.CanReach(point, true, site, new(1735, 452), new(1864, 548)));
        Assert.False(StreamWaterContactAccessPolicy.CanReach(point, true, site, new(1736, 452), new(1865, 548)));
        Assert.False(StreamWaterContactAccessPolicy.CanReach(point, true, new("other", site.Position), new(1736, 452), new(1864, 548)));
        Assert.False(StreamWaterContactAccessPolicy.CanReach(point, true, new(site.Key, new(1734, 483)), new(1736, 452), new(1864, 548)));
    }
}
