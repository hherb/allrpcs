using System.Numerics;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class StreamWaterContactRequestTests
{
    [Fact]
    public void RetainsExactInstancesAndRejectsNulls()
    {
        Character actor = Actor(); StreamWaterContactSite site = Site(); StreamWaterContactState state = new(actor, site);
        StreamWaterContactRequest request = new(actor, site, state);
        Assert.Same(actor, request.Actor); Assert.Same(site, request.Site); Assert.Same(state, request.State);
        Assert.Throws<ArgumentNullException>(() => new StreamWaterContactRequest(null!, site, state));
        Assert.Throws<ArgumentNullException>(() => new StreamWaterContactRequest(actor, null!, state));
        Assert.Throws<ArgumentNullException>(() => new StreamWaterContactRequest(actor, site, null!));
    }
    internal static Character Actor(ControllerType? type = null)
    {
        Character c = RepresentativeSageCharacterSource.Create();
        return new(c.Identity, c.PublicBackground, c.PrivateBackground, c.Capabilities, c.Limitations,
            new(c.Identity.Id, type ?? ControllerType.Human), c.Conditions, c.Inventory, c.Knowledge,
            c.Beliefs, c.Relationships, c.Goals);
    }
    internal static StreamWaterContactSite Site() => new("stream-west-bridge-contact", new(1734, 484));
}
