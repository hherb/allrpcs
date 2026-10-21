using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using static Allrpcs.Application.Tests.Diseases.FirstPatientDiseaseProgressionScheduleTests;

namespace Allrpcs.Application.Tests.Diseases;

public sealed class FirstPatientSliceOutcomePolicyTests
{
    // The two authored dose events; time-passing tests never log them.
    private static readonly WorldEventId[] Doses = [new("dose-1"), new("dose-2")];

    private static FirstPatientSliceOutcome Evaluate(
        DiseaseTruthSnapshot truth,
        WorldEventLogSnapshot log,
        FirstPatientRecoveryStage? recovery = null) =>
        FirstPatientSliceOutcomePolicy.Evaluate(
            truth, Patient, Events, log, Doses, recovery ?? FirstPatientRecoveryStage.NotHeard);

    [Fact]
    public void AnUntreatedPatientWithTimeLeftIsInProgress()
    {
        Assert.Equal(FirstPatientSliceOutcome.InProgress, Evaluate(Truth(DiseaseStage.DeepSleep), After(6).Log));
        Assert.Equal(
            FirstPatientSliceOutcome.InProgress,
            Evaluate(Truth(DiseaseStage.FinalDecline, lastProgression: Advances[5].EventId), After(8).Log));
    }

    [Fact]
    public void AnotherIntervalAtFinalDeclineLosesThePatient()
    {
        DiseaseTruthSnapshot final = Truth(DiseaseStage.FinalDecline, lastProgression: Advances[5].EventId);

        Assert.Equal(FirstPatientSliceOutcome.LostToDecline, Evaluate(final, After(9).Log));
    }

    [Fact]
    public void BothDosesWithoutStabilizationLoseThePatient()
    {
        WorldEventLogSnapshot log = new([.. Doses]);

        Assert.Equal(FirstPatientSliceOutcome.LostAfterDoses, Evaluate(Truth(DiseaseStage.DeepSleep), log));
    }

    [Fact]
    public void AStabilizedPatientCompletesTheSliceOnceTheSageIsConsulted()
    {
        DiseaseTruthSnapshot stabilized = Truth(DiseaseStage.FinalDecline, DiseaseCareState.Stabilized, Advances[0].EventId);
        WorldEventLogSnapshot log = After(9).Log;

        Assert.Equal(
            FirstPatientSliceOutcome.InProgress,
            Evaluate(stabilized, log, FirstPatientRecoveryStage.PatientTestimonyHeard));
        Assert.Equal(
            FirstPatientSliceOutcome.Completed,
            Evaluate(stabilized, log, FirstPatientRecoveryStage.SageConsulted));
    }

    [Fact]
    public void RejectsNulls()
    {
        DiseaseTruthSnapshot truth = Truth(DiseaseStage.DeepSleep);
        WorldEventLogSnapshot log = After(0).Log;
        FirstPatientRecoveryStage stage = FirstPatientRecoveryStage.NotHeard;

        Assert.Throws<ArgumentNullException>(() => FirstPatientSliceOutcomePolicy.Evaluate(null!, Patient, Events, log, Doses, stage));
        Assert.Throws<ArgumentNullException>(() => FirstPatientSliceOutcomePolicy.Evaluate(truth, null!, Events, log, Doses, stage));
        Assert.Throws<ArgumentNullException>(() => FirstPatientSliceOutcomePolicy.Evaluate(truth, Patient, null!, log, Doses, stage));
        Assert.Throws<ArgumentNullException>(() => FirstPatientSliceOutcomePolicy.Evaluate(truth, Patient, Events, null!, Doses, stage));
        Assert.Throws<ArgumentNullException>(() => FirstPatientSliceOutcomePolicy.Evaluate(truth, Patient, Events, log, null!, stage));
        Assert.Throws<ArgumentNullException>(() => FirstPatientSliceOutcomePolicy.Evaluate(truth, Patient, Events, log, Doses, null!));
    }

    [Fact]
    public void EveryEndingHasCopyAndAnOngoingSliceHasNone()
    {
        foreach (FirstPatientSliceOutcome outcome in Enum.GetValues<FirstPatientSliceOutcome>()
            .Where(outcome => outcome != FirstPatientSliceOutcome.InProgress))
        {
            (string title, string body) = FirstPatientStakesCopy.Ending(outcome);
            Assert.False(string.IsNullOrWhiteSpace(title));
            Assert.False(string.IsNullOrWhiteSpace(body));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => FirstPatientStakesCopy.Ending(FirstPatientSliceOutcome.InProgress));
        Assert.Equal(
            "Patient: Appears severely weakened.",
            FirstPatientStakesCopy.WorsenedStatus(new Domain.Characters.CharacterCondition(
                "severely-weakened", "Appears severely weakened.")));
    }
}
