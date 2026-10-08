using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class FootstepContactCadenceTests
{
    private static readonly FootstepLeg[] ExpectedLegs = [FootstepLeg.Left, FootstepLeg.Right, FootstepLeg.Left, FootstepLeg.Right];
    private static readonly int[] ExpectedVariants = [1, 2, 3, 4];
    private static readonly int[] MaterialVariants = [1, 1, 2, 2, 3, 3, 4];
    private static readonly int[] WrappedVariants = [1, 2, 3, 4, 5, 6, 1];

    private static FootstepMaterial? Earth(Vector2 position) { _ = position; return FootstepMaterial.Earth; }

    [Fact]
    public void MeasuredDistanceProducesAlternatingContactsRatherThanInputOrWallPressure()
    {
        FootstepContactCadence cadence = new();
        Assert.Null(cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.02, 240), Earth).Contact);
        List<FootstepContact> contacts = [];
        for (int tick = 2; tick <= 73; tick++)
        {
            float start = (tick - 2) * 4.8f;
            FootstepCadenceResult result = cadence.Advance(new(tick, new(start, 0), new(start + 4.8f, 0), 0.02, 240), Earth);
            if (result.Contact is { } contact) { contacts.Add(contact); }
        }
        Assert.Equal(4, contacts.Count);
        Assert.Equal(ExpectedLegs, contacts.Select(c => c.Leg));
        Assert.Equal(ExpectedVariants, contacts.Select(c => c.Variant));
        Assert.True(cadence.Advance(new(74, new(345.6f, 0), new(345.6f, 0), 0.02, 240), Earth).Cancelled);
        Assert.Null(cadence.Advance(new(75, new(345.6f, 0), new(345.6f, 0), 0.02, 240), Earth).Contact);
    }

    [Fact]
    public void ContactMaterialUsesInterpolatedThresholdInsteadOfFrameEnd()
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.02, 240), Earth);
        cadence.Advance(new(2, Vector2.Zero, new(40, 0), 0.1, 400), Earth);
        cadence.Advance(new(3, new(40, 0), new(80, 0), 0.1, 400), Earth);
        FootstepContact contact = cadence.Advance(new(4, new(80, 0), new(120, 0), 0.1, 400),
            p => p.X < 90 ? FootstepMaterial.Earth : FootstepMaterial.Gravel).Contact!;
        Assert.Equal(FootstepMaterial.Earth, contact.Material);
        Assert.InRange(contact.Position.X, 86.399f, 86.401f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.11)]
    [InlineData(-0.01)]
    public void InvalidOrStalledDeltaCancelsWithoutCatchUp(double delta)
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.02, 240), Earth);
        cadence.Advance(new(2, Vector2.Zero, new(24, 0), 0.1, 240), Earth);
        Assert.True(cadence.Advance(new(3, new(24, 0), new(48, 0), delta, 240), Earth).Cancelled);
        Assert.Null(cadence.Advance(new(4, new(48, 0), new(72, 0), 0.1, 240), Earth).Contact);
    }

    [Fact]
    public void TeleportsDuplicatesAndUnmappedSegmentCancelEvenWhenEndpointsLookEligible()
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.1, 240), Earth);
        Assert.True(cadence.Advance(new(2, new(5, 0), new(29, 0), 0.1, 240), Earth).Cancelled);
        Assert.True(cadence.Advance(new(2, new(29, 0), new(53, 0), 0.1, 240), Earth).Cancelled);
        Assert.True(cadence.Advance(new(3, new(29, 0), new(200, 0), 0.1, 240), Earth).Cancelled);
        Assert.True(cadence.Advance(new(4, new(200, 0), new(224, 0), 0.1, 240),
            p => p.X > 210 && p.X < 214 ? null : FootstepMaterial.Earth).Cancelled);
        cadence.Reset();
        Assert.Null(cadence.Advance(new(5, new(1000, 0), new(1024, 0), 0.1, 240), Earth).Contact);
    }

    [Fact]
    public void FasterMovementConsumesExcessContactsWithoutQueueAndKeepsLegAndVariants()
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.1, 2000), Earth);
        Assert.Null(cadence.Advance(new(2, Vector2.Zero, new(200, 0), 0.1, 2000), Earth).Contact);
        FootstepContact contact = cadence.Advance(new(3, new(200, 0), new(400, 0), 0.1, 2000), Earth).Contact!;
        Assert.Equal(3, contact.Variant);
        Assert.Equal(FootstepLeg.Left, contact.Leg);
        Assert.Null(cadence.Advance(new(4, new(400, 0), new(600, 0), 0.1, 2000), Earth).Contact);
    }

    [Fact]
    public void TurnsAndMaterialChangesPreserveDistanceAndEachMaterialsVariantCounter()
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.1, 240), Earth);
        Vector2 start = Vector2.Zero;
        List<FootstepContact> contacts = [];
        for (int tick = 2; tick < 30; tick++)
        {
            Vector2 end = start + (tick % 2 == 0 ? new Vector2(24, 0) : new Vector2(0, 24));
            FootstepCadenceResult result = cadence.Advance(new(tick, start, end, 0.1, 240),
                _ => contacts.Count % 2 == 0 ? FootstepMaterial.Earth : FootstepMaterial.Leaves);
            if (result.Contact is { } contact) { contacts.Add(contact); }
            start = end;
        }
        Assert.Equal(7, contacts.Count);
        Assert.Equal(MaterialVariants, contacts.Select(c => c.Variant));
    }

    [Fact]
    public void SixVariantsWrapAndExplicitResetStartsWithFreshDisplacement()
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.1, 240), Earth);
        List<FootstepContact> contacts = [];
        for (int tick = 2; tick < 30; tick++)
        {
            float start = (tick - 2) * 24;
            FootstepContact? contact = cadence.Advance(new(tick, new(start, 0), new(start + 24, 0), 0.1, 240), Earth).Contact;
            if (contact is not null) { contacts.Add(contact); }
        }
        Assert.Equal(WrappedVariants, contacts.Select(c => c.Variant));
        cadence.Reset();
        Assert.Null(cadence.Advance(new(100, Vector2.Zero, new(24, 0), 0.1, 240), Earth).Contact);
        FootstepContact? restarted = null;
        for (int tick = 101; tick < 105; tick++)
        {
            float start = (tick - 100) * 24;
            restarted ??= cadence.Advance(new(tick, new(start, 0), new(start + 24, 0), 0.1, 240), Earth).Contact;
        }
        Assert.Equal(1, restarted!.Variant);
        Assert.Equal(FootstepLeg.Left, restarted.Leg);
    }

    [Fact]
    public void MalformedCoordinatesAndImpossibleWorldDisplacementFailBeforeSampling()
    {
        FootstepContactCadence cadence = new();
        cadence.Advance(new(1, Vector2.Zero, Vector2.Zero, 0.1, 240), Earth);
        Assert.True(cadence.Advance(new(2, Vector2.Zero, new(float.NaN, 0), 0.1, 240), Earth).Cancelled);
        cadence.Reset();
        cadence.Advance(new(3, Vector2.Zero, Vector2.Zero, 0.1, 100000), Earth);
        Assert.True(cadence.Advance(new(4, Vector2.Zero, new(10000, 0), 0.1, 100000),
            _ => throw new InvalidOperationException("Invalid displacement reached material sampling")).Cancelled);
        Assert.True(cadence.Advance(new(5, new(10000, 0), new(10001, 0), 0.1, 0), Earth).Cancelled);
    }
}
