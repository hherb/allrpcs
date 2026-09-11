using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProtectionApplicationOutcomeTests
{
    [Fact]
    public void RetainsExactEvidenceViewDefinitionAndAuthoredEvent()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseProtectionDefinition definition = new(disease, new EvidenceId("covered-water"));
        CharacterEvidenceViewSnapshot evidenceView = new(
            new CharacterId("patient"),
            [definition.EvidenceId]);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "water-covered",
            WorldEventTimeEffect.NoAdvance);

        DiseaseProtectionApplicationOutcome outcome = new(
            evidenceView,
            definition,
            worldEvent);

        Assert.Same(evidenceView, outcome.EvidenceView);
        Assert.Same(definition, outcome.Definition);
        Assert.Same(worldEvent, outcome.WorldEvent);
    }

    [Fact]
    public void RejectsNullValues()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseProtectionDefinition definition = new(disease, new EvidenceId("covered-water"));
        CharacterEvidenceViewSnapshot evidenceView = new(new CharacterId("patient"), []);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "water-covered",
            WorldEventTimeEffect.NoAdvance);

        Assert.Throws<ArgumentNullException>(() =>
            new DiseaseProtectionApplicationOutcome(null!, definition, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new DiseaseProtectionApplicationOutcome(evidenceView, null!, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new DiseaseProtectionApplicationOutcome(evidenceView, definition, null!));
    }
}
