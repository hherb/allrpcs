using System.Numerics;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class HandToolTapRequestTests
{
    [Fact]
    public void RetainsExactInstancesAndRejectsNulls()
    {
        Character actor = Actor(); HandToolTapSite site = Site(); HandToolTapState state = new(actor, site);
        HandToolTapRequest request = new(actor, site, state);
        Assert.Same(actor, request.Actor); Assert.Same(site, request.Site); Assert.Same(state, request.State);
        Assert.Throws<ArgumentNullException>(() => new HandToolTapRequest(null!, site, state));
        Assert.Throws<ArgumentNullException>(() => new HandToolTapRequest(actor, null!, state));
        Assert.Throws<ArgumentNullException>(() => new HandToolTapRequest(actor, site, null!));
    }
    internal static Character Actor(ControllerType? type = null)
    {
        Character c = RepresentativeSageCharacterSource.Create();
        return new(c.Identity, c.PublicBackground, c.PrivateBackground, c.Capabilities, c.Limitations,
            new(c.Identity.Id, type ?? ControllerType.Human), c.Conditions, c.Inventory, c.Knowledge,
            c.Beliefs, c.Relationships, c.Goals);
    }
    internal static HandToolTapSite Site() => new("village-edge-practice-block", new(1700, 900));
}
