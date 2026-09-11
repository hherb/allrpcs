using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseExposurePolicyTests
{
    [Fact]
    public void AllowsUnprotectedExposureToExactAuthoredDiseaseAtOccurredEvent()
    {
        ExposureInput input = Input();

        Assert.True(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.Protections,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsExistingInfectionAndActiveMatchingProtection()
    {
        ExposureInput input = Input();
        CharacterDiseaseState infected = new(
            input.Outcome.CharacterId,
            input.Outcome.Disease,
            DiseaseStage.EarlyIllness,
            null);
        DiseaseProtectionDefinition protection = Assert.Single(
            input.Protections.Definitions.Definitions);
        DiseaseProtectionSnapshot activeProtections = new(
            input.Protections.Definitions,
            [new CharacterDiseaseProtection(input.Outcome.CharacterId, protection)]);

        Assert.False(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            new DiseaseTruthSnapshot(input.Truth.Definitions, [infected]),
            input.Protections,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            input.Truth,
            activeProtections,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsExposureWhenDiseaseTruthIsAtCapacity()
    {
        ExposureInput input = Input();
        CharacterDiseaseState[] maximum = Enumerable
            .Range(0, DiseaseTruthSnapshot.MaximumInfectionCount)
            .Select(index => new CharacterDiseaseState(
                new CharacterId($"existing-{index}"),
                input.Outcome.Disease,
                DiseaseStage.Exposed,
                null))
            .ToArray();

        Assert.False(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            new DiseaseTruthSnapshot(input.Truth.Definitions, maximum),
            input.Protections,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void IgnoresProtectionForAnotherCharacterOrDisease()
    {
        ExposureInput input = Input();
        DiseaseDefinition otherDisease = new(new DiseaseId("marsh-fever"));
        DiseaseDefinitionSnapshot diseases = new([input.Outcome.Disease, otherDisease]);
        DiseaseProtectionDefinition matching = Assert.Single(
            input.Protections.Definitions.Definitions);
        DiseaseProtectionDefinition other = new(otherDisease, new EvidenceId("dry-boots"));
        DiseaseProtectionDefinitionSnapshot definitions = new(diseases, [matching, other]);
        DiseaseProtectionSnapshot protections = new(
            definitions,
            [
                new CharacterDiseaseProtection(new CharacterId("healer"), matching),
                new CharacterDiseaseProtection(input.Outcome.CharacterId, other),
            ]);
        DiseaseTruthSnapshot truth = new(diseases, []);

        Assert.True(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            truth,
            protections,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsUnauthoritativeDiseaseEventAndNullInputs()
    {
        ExposureInput input = Input();
        DiseaseDefinition replacementDisease = DiseaseProgressionPolicyTests.Definition();
        WorldEvent replacementEvent = DiseaseProgressionPolicyTests.Event(
            input.Outcome.WorldEvent.EventId.Value,
            WorldEventTimeEffect.NoAdvance);

        Assert.False(DiseaseExposurePolicy.CanApply(
            new DiseaseExposureOutcome(
                input.Outcome.CharacterId,
                replacementDisease,
                input.Outcome.WorldEvent),
            input.Truth,
            input.Protections,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseExposurePolicy.CanApply(
            new DiseaseExposureOutcome(
                input.Outcome.CharacterId,
                input.Outcome.Disease,
                replacementEvent),
            input.Truth,
            input.Protections,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.Protections,
            input.WorldEvents,
            new WorldEventLogSnapshot([])));
        WorldEvent later = DiseaseProgressionPolicyTests.Event(
            "patient-rested",
            WorldEventTimeEffect.NoAdvance);
        Assert.False(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.Protections,
            new WorldEventSnapshot([input.Outcome.WorldEvent, later]),
            new WorldEventLogSnapshot([input.Outcome.WorldEvent.EventId, later.EventId])));
        DiseaseProtectionDefinition protection = Assert.Single(
            input.Protections.Definitions.Definitions);
        DiseaseProtectionSnapshot detachedProtections = new(
            new DiseaseProtectionDefinitionSnapshot(
                new DiseaseDefinitionSnapshot([input.Outcome.Disease]),
                [protection]),
            []);
        Assert.False(DiseaseExposurePolicy.CanApply(
            input.Outcome,
            input.Truth,
            detachedProtections,
            input.WorldEvents,
            input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseExposurePolicy.CanApply(
            null!, input.Truth, input.Protections, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseExposurePolicy.CanApply(
            input.Outcome, null!, input.Protections, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseExposurePolicy.CanApply(
            input.Outcome, input.Truth, null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseExposurePolicy.CanApply(
            input.Outcome, input.Truth, input.Protections, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseExposurePolicy.CanApply(
            input.Outcome, input.Truth, input.Protections, input.WorldEvents, null!));
    }

    internal static ExposureInput Input()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseDefinitionSnapshot diseases = new([disease]);
        DiseaseProtectionDefinition definition = new(
            disease,
            new EvidenceId("covered-water-prevents-exposure"));
        DiseaseProtectionSnapshot protections = new(
            new DiseaseProtectionDefinitionSnapshot(diseases, [definition]),
            []);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "shared-water-consumed",
            WorldEventTimeEffect.NoAdvance);

        return new ExposureInput(
            new DiseaseExposureOutcome(new CharacterId("patient"), disease, worldEvent),
            new DiseaseTruthSnapshot(diseases, []),
            protections,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]));
    }

    internal sealed record ExposureInput(
        DiseaseExposureOutcome Outcome,
        DiseaseTruthSnapshot Truth,
        DiseaseProtectionSnapshot Protections,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog);
}
