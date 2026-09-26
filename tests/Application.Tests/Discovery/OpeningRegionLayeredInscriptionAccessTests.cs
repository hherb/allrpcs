using System.Numerics;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Discovery;

public sealed class OpeningRegionLayeredInscriptionAccessTests
{
    private static readonly Vector2 InscriptionPosition = new(110.0f, 90.0f);

    [Fact]
    public void RetainsExactSeekerAccessWithinTheShrine()
    {
        OpeningRegionLayeredInscriptionBinding binding = Binding();
        Vector2 seekerPosition = new(110.0f, 128.0f);

        OpeningRegionLayeredInscriptionAccess access = new(
            binding,
            binding.DiscoveryScenario.ObserverCharacterId,
            seekerPosition);

        Assert.Same(binding, access.InscriptionBinding);
        Assert.Same(binding.DiscoveryScenario.ObserverCharacterId, access.SeekerCharacterId);
        Assert.Equal(seekerPosition, access.SeekerPosition);
        Assert.Equal(InscriptionPosition, access.InscriptionPosition);
        Assert.Equal(112.0f, OpeningRegionLayeredInscriptionAccess.MaximumDistancePixels);
    }

    [Fact]
    public void RejectsForeignOrRebuiltCharactersAndInaccessiblePositions()
    {
        OpeningRegionLayeredInscriptionBinding binding = Binding();
        Vector2 seekerPosition = new(110.0f, 128.0f);

        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionAccess(
            binding,
            new CharacterId(binding.DiscoveryScenario.ObserverCharacterId.Value),
            seekerPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionAccess(
            binding,
            new CharacterId("travelling-healer"),
            seekerPosition));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionAccess(
            binding,
            binding.DiscoveryScenario.ObserverCharacterId,
            new Vector2(216.0f, 136.0f)));
        Assert.Throws<ArgumentException>(() => new OpeningRegionLayeredInscriptionAccess(
            binding,
            binding.DiscoveryScenario.ObserverCharacterId,
            new Vector2(8.0f, 90.0f)));
    }

    private static OpeningRegionLayeredInscriptionBinding Binding()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionOldShrineSceneBinding shrine = new(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        return new OpeningRegionLayeredInscriptionBinding(
            shrine,
            scenario,
            scenario.WorldObjects.Entries.Single(candidate =>
                candidate.Identity.Id.Value == "shrine-inscription"),
            scenario.InitialPerception.Entries.Single(candidate =>
                candidate.ObjectId.Value == "shrine-inscription"),
            InscriptionPosition);
    }
}
