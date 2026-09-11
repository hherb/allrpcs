using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProtectionApplicationPolicyTests
{
    [Fact]
    public void AllowsExactDefinitionKnownToCharacterAtOccurredAuthoredEvent()
    {
        ProtectionInput input = Input();

        Assert.True(DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome,
            input.Protections,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsUnknownEvidenceReplacementDefinitionAndExistingProtection()
    {
        ProtectionInput input = Input();
        DiseaseProtectionDefinition replacement = new(
            input.Outcome.Definition.Disease,
            new EvidenceId(input.Outcome.Definition.EvidenceId.Value));
        DiseaseProtectionApplicationOutcome unknownEvidence = new(
            new CharacterEvidenceViewSnapshot(input.Outcome.EvidenceView.CharacterId, []),
            input.Outcome.Definition,
            input.Outcome.WorldEvent);
        DiseaseProtectionSnapshot active = new(
            input.Protections.Definitions,
            [new CharacterDiseaseProtection(
                input.Outcome.EvidenceView.CharacterId,
                input.Outcome.Definition)]);

        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            unknownEvidence,
            input.Protections,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            new DiseaseProtectionApplicationOutcome(
                input.Outcome.EvidenceView,
                replacement,
                input.Outcome.WorldEvent),
            input.Protections,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome,
            active,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsApplicationWhenActiveProtectionsAreAtCapacity()
    {
        ProtectionInput input = Input();
        CharacterDiseaseProtection[] maximum = Enumerable
            .Range(0, DiseaseProtectionSnapshot.MaximumActiveProtectionCount)
            .Select(index => new CharacterDiseaseProtection(
                new CharacterId($"protected-{index}"),
                input.Outcome.Definition))
            .ToArray();
        DiseaseProtectionSnapshot full = new(input.Protections.Definitions, maximum);

        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome,
            full,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsReplacementAndUnoccurredEventsAndNullInputs()
    {
        ProtectionInput input = Input();
        WorldEvent replacement = DiseaseProgressionPolicyTests.Event(
            input.Outcome.WorldEvent.EventId.Value,
            WorldEventTimeEffect.NoAdvance);

        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            new DiseaseProtectionApplicationOutcome(
                input.Outcome.EvidenceView,
                input.Outcome.Definition,
                replacement),
            input.Protections,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome,
            input.Protections,
            input.WorldEvents,
            new WorldEventLogSnapshot([])));
        WorldEvent later = DiseaseProgressionPolicyTests.Event(
            "patient-rested",
            WorldEventTimeEffect.NoAdvance);
        Assert.False(DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome,
            input.Protections,
            new WorldEventSnapshot([input.Outcome.WorldEvent, later]),
            new WorldEventLogSnapshot([input.Outcome.WorldEvent.EventId, later.EventId])));
        Assert.Throws<ArgumentNullException>(() => DiseaseProtectionApplicationPolicy.CanApply(
            null!, input.Protections, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome, null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome, input.Protections, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseProtectionApplicationPolicy.CanApply(
            input.Outcome, input.Protections, input.WorldEvents, null!));
    }

    internal static ProtectionInput Input()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        DiseaseDefinitionSnapshot diseases = new([disease]);
        DiseaseProtectionDefinition definition = new(disease, new EvidenceId("covered-water"));
        DiseaseProtectionSnapshot protections = new(
            new DiseaseProtectionDefinitionSnapshot(diseases, [definition]),
            []);
        CharacterEvidenceViewSnapshot evidenceView = new(
            new CharacterId("patient"),
            [definition.EvidenceId]);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "water-covered",
            WorldEventTimeEffect.NoAdvance);

        return new ProtectionInput(
            new DiseaseProtectionApplicationOutcome(evidenceView, definition, worldEvent),
            protections,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]));
    }

    internal sealed record ProtectionInput(
        DiseaseProtectionApplicationOutcome Outcome,
        DiseaseProtectionSnapshot Protections,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog);
}
