using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryEvidenceProjectionTests
{
    private static readonly string[] ExpectedEvidenceIds =
    [
        "visible-herb",
        "concealed-herb",
        "partial-inscription",
        "residue",
        "tool-gated-clue",
    ];

    [Fact]
    public void InitialRepresentativeViewExposesOnlyAlreadyVisibleEvidence()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        CharacterEvidenceViewSnapshot view = DiscoveryEvidenceProjection.ForCharacter(
            scenario.Evidence,
            scenario.InitialPerception);

        Assert.Same(scenario.ObserverCharacterId, view.CharacterId);
        EvidenceId visibleEvidenceId = Assert.Single(view.EvidenceIds);
        Assert.Same(scenario.Evidence.Entries[0].EvidenceId, visibleEvidenceId);
        Assert.Equal("visible-herb", visibleEvidenceId.Value);
        Assert.DoesNotContain(view.EvidenceIds, id => id.Value == "concealed-herb");
        Assert.DoesNotContain(view.EvidenceIds, id => id.Value == "partial-inscription");
        Assert.DoesNotContain(view.EvidenceIds, id => id.Value == "residue");
        Assert.DoesNotContain(view.EvidenceIds, id => id.Value == "tool-gated-clue");
    }

    [Fact]
    public void RevealsEvidenceAtOrAboveThresholdInAuthoredOrder()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        CharacterPerceptionSnapshot perception = new(
            new CharacterId("seeker"),
            scenario.Evidence.Entries.Select(definition =>
                new WorldObjectVisibility(definition.ObjectId, definition.MinimumVisibilityState)));

        CharacterEvidenceViewSnapshot view = DiscoveryEvidenceProjection.ForCharacter(
            scenario.Evidence,
            perception);

        Assert.Equal(
            ExpectedEvidenceIds,
            view.EvidenceIds.Select(id => id.Value));
        Assert.Equal(scenario.ObserverCharacterId, view.CharacterId);
        Assert.NotSame(scenario.ObserverCharacterId, view.CharacterId);
    }

    [Fact]
    public void OmitsUnknownTargetsAndPreservesInputs()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        CharacterPerceptionSnapshot perception = new(
            scenario.ObserverCharacterId,
            [new WorldObjectVisibility(new ObjectId("unknown-target"), VisibilityState.Collected)]);

        CharacterEvidenceViewSnapshot view = DiscoveryEvidenceProjection.ForCharacter(
            scenario.Evidence,
            perception);

        Assert.Empty(view.EvidenceIds);
        Assert.Equal(5, scenario.Evidence.Entries.Count);
        Assert.Single(perception.Entries);
        Assert.Equal("unknown-target", perception.Entries[0].ObjectId.Value);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        Assert.Throws<ArgumentNullException>(() =>
            DiscoveryEvidenceProjection.ForCharacter(null!, scenario.InitialPerception));
        Assert.Throws<ArgumentNullException>(() =>
            DiscoveryEvidenceProjection.ForCharacter(scenario.Evidence, null!));
    }
}
