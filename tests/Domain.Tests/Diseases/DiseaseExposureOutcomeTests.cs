using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseExposureOutcomeTests
{
    [Fact]
    public void RetainsExactCharacterDiseaseAndAuthoredEvent()
    {
        CharacterId characterId = new("patient");
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "shared-water-consumed",
            WorldEventTimeEffect.NoAdvance);

        DiseaseExposureOutcome outcome = new(characterId, disease, worldEvent);

        Assert.Same(characterId, outcome.CharacterId);
        Assert.Same(disease, outcome.Disease);
        Assert.Same(worldEvent, outcome.WorldEvent);
    }

    [Fact]
    public void RejectsNullValues()
    {
        CharacterId characterId = new("patient");
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "shared-water-consumed",
            WorldEventTimeEffect.NoAdvance);

        Assert.Throws<ArgumentNullException>(() => new DiseaseExposureOutcome(null!, disease, worldEvent));
        Assert.Throws<ArgumentNullException>(() => new DiseaseExposureOutcome(characterId, null!, worldEvent));
        Assert.Throws<ArgumentNullException>(() => new DiseaseExposureOutcome(characterId, disease, null!));
    }
}
