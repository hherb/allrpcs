using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class HealerFootstepSpatialProjectionTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(72, true)]
    [InlineData(191.999, true)]
    [InlineData(192, false)]
    [InlineData(193, false)]
    public void WorldRangeIsExclusiveIndependentOfCaptionAndOutput(float distance, bool expected) =>
        Assert.Equal(expected, HealerFootstepSpatialProjection.Admitted(
            new(FootstepMaterial.Earth, FootstepLeg.Left, 1, new(distance, 0)), Vector2.Zero, true, true));

    [Fact]
    public void UnknownForeignUnsupportedAndMalformedContactsCannotConveySpatialInformation()
    {
        FootstepContact contact = new(FootstepMaterial.Earth, FootstepLeg.Left, 1, Vector2.Zero);
        Assert.False(HealerFootstepSpatialProjection.Admitted(null, Vector2.Zero, true, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact with { Material = (FootstepMaterial)99 }, Vector2.Zero, true, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact with { Leg = (FootstepLeg)99 }, Vector2.Zero, true, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact with { Variant = 0 }, Vector2.Zero, true, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact with { Position = new(float.NaN, 0) }, Vector2.Zero, true, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact, new(0, float.PositiveInfinity), true, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact, Vector2.Zero, false, true));
        Assert.False(HealerFootstepSpatialProjection.Admitted(contact, Vector2.Zero, true, false));
    }

    [Fact]
    public void BearingsUseScreenSpaceWithoutIdentityAndRetainInclusiveViewportEdges()
    {
        FootstepContact contact = new(FootstepMaterial.Leaves, FootstepLeg.Left, 1, new(1000, 350));
        Assert.Equal("Footsteps in leaves", HealerFootstepSpatialProjection.Caption(contact, new(1280, 720), new(1280, 720)));
        Assert.Equal("Footsteps in leaves right", HealerFootstepSpatialProjection.Caption(contact, new(1300, 360), new(1280, 720)));
        Assert.Equal("Footsteps in leaves above", HealerFootstepSpatialProjection.Caption(contact, new(640, -1), new(1280, 720)));
    }

    [Fact]
    public void IndependentCadenceUsesActualHealerSpeedAndPreservesSeekerCounters()
    {
        FootstepContactCadence healer = new(), seeker = new();
        FootstepMaterial? Earth(Vector2 p) { _ = p; return FootstepMaterial.Earth; }
        healer.Advance(new(1, Vector2.Zero, Vector2.Zero, 1d / 60, 180), Earth);
        seeker.Advance(new(1, Vector2.Zero, Vector2.Zero, 1d / 60, 240), Earth);
        List<FootstepContact> contacts = [];
        for (int tick = 2; tick <= 60; tick++)
        {
            var step = healer.Advance(new(tick, new((tick - 2) * 3, 0), new((tick - 1) * 3, 0), 1d / 60, 180), Earth);
            if (step.Contact is { } c) { contacts.Add(c); }
        }
        Assert.Equal(2, contacts.Count);
        Assert.Equal(FootstepLeg.Left, contacts[0].Leg);
        Assert.Equal(FootstepLeg.Right, contacts[1].Leg);
        Assert.Equal(2, contacts[1].Variant);
        Assert.Null(seeker.Advance(new(2, Vector2.Zero, new(24, 0), 0.1, 240), Earth).Contact);
        Assert.True(healer.Advance(new(61, new(177, 0), new(177, 0), 1d / 60, 180), Earth).Cancelled);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-0.01)]
    [InlineData(0)]
    [InlineData(0.101)]
    public void InvalidPresentationTimeCannotAdvanceHistoryOrCaption(double seconds) =>
        Assert.False(HealerFootstepSpatialProjection.ValidDelta(seconds));
}
