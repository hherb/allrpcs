using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Actions;

public sealed class CaskCoverHandlingStateTests
{
    [Fact]
    public void BoundsSessionStateAndAuthorsOnlyThisExactCaskMaterial()
    {
        Character actor = RepresentativeSageCharacterSource.Create(); var material = CaskCoverHandlingRequestTests.Material();
        CaskCoverHandlingState state = new(actor, material, 7, 4);
        Assert.Same(actor, state.Actor); Assert.Same(material, state.Material); Assert.Equal(7, state.Sequence);
        Assert.Equal(4, state.Cooldown); Assert.Equal("Wooden lid and cotton outer cover", material.Description);
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingMaterial(null!));
        Assert.Throws<ArgumentException>(() => new CaskCoverHandlingMaterial(new WorldObject(new(new("other"), "Other"), new("Other"))));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CaskCoverHandlingState(actor, material, -1));
        foreach (double time in new[] { -1, 4.1, double.NaN, double.PositiveInfinity })
        { Assert.Throws<ArgumentOutOfRangeException>(() => new CaskCoverHandlingState(actor, material, 0, time)); }
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingState(null!, material));
        Assert.Throws<ArgumentNullException>(() => new CaskCoverHandlingState(actor, null!));
    }
}
