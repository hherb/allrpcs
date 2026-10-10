using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class CaskCoverHandlingRequestTests
{
    [Fact]
    public void RetainsExactActorTargetMaterialAndStateWithNullGuards()
    {
        Character actor = RepresentativeSageCharacterSource.Create(); CaskCoverHandlingMaterial material = Material();
        CaskCoverHandlingState state = new(actor, material); CaskCoverHandlingRequest request = new(actor, material, state);
        Assert.Same(actor, request.Actor); Assert.Same(material, request.Material);
        Assert.Same(material.Target, request.Target); Assert.Same(state, request.State);
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingRequest(null!, material, state));
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingRequest(actor, null!, state));
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingRequest(actor, material, null!));
    }
    internal static CaskCoverHandlingMaterial Material() => new(new WorldObject(
        new(new("village-water-cask"), "Village water cask"), new("A covered cask beside the village well.")));
}
