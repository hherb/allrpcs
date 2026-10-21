using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Diseases;

public sealed class FirstPatientDiseaseProgressionScheduleTests
{
    internal static readonly CharacterId Patient = new("first-sleeping-patient");

    // Advance events a0..a8 plus one NoAdvance event that must never count as time passing.
    internal static readonly WorldEvent Ready = new(new WorldEventId("ready"), WorldEventTimeEffect.NoAdvance);
    internal static readonly WorldEvent[] Advances = Enumerable.Range(0, 9)
        .Select(index => new WorldEvent(new WorldEventId($"a{index}"), WorldEventTimeEffect.Advance))
        .ToArray();
    internal static readonly WorldEventSnapshot Events = new([Ready, .. Advances]);

    internal static DiseaseTruthSnapshot Truth(
        DiseaseStage stage,
        DiseaseCareState? care = null,
        WorldEventId? lastProgression = null)
    {
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        return new DiseaseTruthSnapshot(
            new DiseaseDefinitionSnapshot([disease]),
            [new CharacterDiseaseState(Patient, disease, stage, lastProgression, care ?? DiseaseCareState.Unassisted)]);
    }

    // The log holds Ready and then the first `count` Advance events; time is the latest one.
    internal static (WorldEventLogSnapshot Log, WorldTimeSnapshot Time) After(int count)
    {
        WorldEventId[] entries = [Ready.EventId, .. Advances.Take(count).Select(entry => entry.EventId)];
        return (new WorldEventLogSnapshot(entries), new WorldTimeSnapshot(count == 0 ? null : entries[^1]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void KeepsTheSameTruthBeforeAWholeIntervalHasPassed(int count)
    {
        DiseaseTruthSnapshot truth = Truth(DiseaseStage.DeepSleep);
        (WorldEventLogSnapshot log, WorldTimeSnapshot time) = After(count);

        Assert.Same(truth, FirstPatientDiseaseProgressionSchedule.Apply(truth, Patient, Events, log, time));
        Assert.Equal(count, FirstPatientDiseaseProgressionSchedule.AdvanceEventsSinceProgression(
            truth.Infections[0], Events, log));
    }

    [Fact]
    public void WorsensOneStageAfterAnIntervalAndRestartsTheCount()
    {
        (WorldEventLogSnapshot log, WorldTimeSnapshot time) = After(FirstPatientDiseaseProgressionSchedule.StageInterval);

        DiseaseTruthSnapshot result = FirstPatientDiseaseProgressionSchedule.Apply(
            Truth(DiseaseStage.DeepSleep), Patient, Events, log, time);

        CharacterDiseaseState worsened = Assert.Single(result.Infections);
        Assert.Same(DiseaseStage.Wasting, worsened.Stage);
        Assert.Equal(Advances[2].EventId, worsened.LastProgressionEventId);
        Assert.Equal(0, FirstPatientDiseaseProgressionSchedule.AdvanceEventsSinceProgression(worsened, Events, log));
        // Evaluating again at the same time never progresses twice.
        Assert.Same(result, FirstPatientDiseaseProgressionSchedule.Apply(result, Patient, Events, log, time));
    }

    [Fact]
    public void CountsOnlyAdvanceEventsAfterTheLastProgression()
    {
        DiseaseTruthSnapshot truth = Truth(DiseaseStage.Wasting, lastProgression: Advances[2].EventId);
        (WorldEventLogSnapshot log, WorldTimeSnapshot time) = After(5);

        Assert.Equal(2, FirstPatientDiseaseProgressionSchedule.AdvanceEventsSinceProgression(
            truth.Infections[0], Events, log));
        Assert.Same(truth, FirstPatientDiseaseProgressionSchedule.Apply(truth, Patient, Events, log, time));

        (log, time) = After(6);
        Assert.Same(
            DiseaseStage.FinalDecline,
            FirstPatientDiseaseProgressionSchedule.Apply(truth, Patient, Events, log, time).Infections[0].Stage);
    }

    [Fact]
    public void NeverWorsensAStabilizedPatientOrPastFinalDecline()
    {
        (WorldEventLogSnapshot log, WorldTimeSnapshot time) = After(9);
        DiseaseTruthSnapshot stabilized = Truth(DiseaseStage.DeepSleep, DiseaseCareState.Stabilized);
        DiseaseTruthSnapshot final = Truth(DiseaseStage.FinalDecline);

        Assert.Same(stabilized, FirstPatientDiseaseProgressionSchedule.Apply(stabilized, Patient, Events, log, time));
        Assert.Same(final, FirstPatientDiseaseProgressionSchedule.Apply(final, Patient, Events, log, time));
    }

    [Fact]
    public void LeavesTruthWithoutThePatientUnchangedAndRejectsNulls()
    {
        DiseaseTruthSnapshot truth = Truth(DiseaseStage.DeepSleep);
        (WorldEventLogSnapshot log, WorldTimeSnapshot time) = After(3);

        Assert.Same(truth, FirstPatientDiseaseProgressionSchedule.Apply(truth, new CharacterId("other"), Events, log, time));
        Assert.Throws<ArgumentNullException>(() => FirstPatientDiseaseProgressionSchedule.Apply(null!, Patient, Events, log, time));
        Assert.Throws<ArgumentNullException>(() => FirstPatientDiseaseProgressionSchedule.Apply(truth, null!, Events, log, time));
        Assert.Throws<ArgumentNullException>(() => FirstPatientDiseaseProgressionSchedule.Apply(truth, Patient, null!, log, time));
        Assert.Throws<ArgumentNullException>(() => FirstPatientDiseaseProgressionSchedule.Apply(truth, Patient, Events, null!, time));
        Assert.Throws<ArgumentNullException>(() => FirstPatientDiseaseProgressionSchedule.Apply(truth, Patient, Events, log, null!));
    }
}
