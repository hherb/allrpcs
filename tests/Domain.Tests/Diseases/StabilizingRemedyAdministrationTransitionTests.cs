using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class StabilizingRemedyAdministrationTransitionTests
{
    [Fact]
    public void StabilizesOnlyTheExactFirstPatientInfection()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();
        CharacterDiseaseState infection = Assert.Single(input.Scenario.DiseaseTruth.Infections);

        DiseaseTruthSnapshot result = StabilizingRemedyAdministrationTransition.Apply(
            input.Context,
            input.Administration,
            input.WorldEvents,
            input.EventLog);

        CharacterDiseaseState stabilized = Assert.Single(result.Infections);
        Assert.NotSame(input.Scenario.DiseaseTruth, result);
        Assert.Same(input.Scenario.DiseaseTruth.Definitions, result.Definitions);
        Assert.NotSame(infection, stabilized);
        Assert.Same(infection.CharacterId, stabilized.CharacterId);
        Assert.Same(infection.Disease, stabilized.Disease);
        Assert.Same(infection.Stage, stabilized.Stage);
        Assert.Same(infection.LastProgressionEventId, stabilized.LastProgressionEventId);
        Assert.Same(DiseaseCareState.Stabilized, stabilized.CareState);
        Assert.Same(infection, Assert.Single(input.Scenario.DiseaseTruth.Infections));
        Assert.Same(DiseaseCareState.Unassisted, infection.CareState);
    }

    [Fact]
    public void RejectsIneligibleAdministrationWithoutChangingScenarioState()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();

        Assert.Throws<InvalidOperationException>(() =>
            StabilizingRemedyAdministrationTransition.Apply(
                input.Context,
                input.Administration,
                input.WorldEvents,
                new WorldEventLogSnapshot([])));

        CharacterDiseaseState infection = Assert.Single(input.Scenario.DiseaseTruth.Infections);
        Assert.Same(DiseaseCareState.Unassisted, infection.CareState);
        Assert.Same(input.Scenario.PatientId, input.Administration.PatientId);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureTransition()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();

        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationTransition.Apply(
            null!, input.Administration, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationTransition.Apply(
            input.Context, null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationTransition.Apply(
            input.Context, input.Administration, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationTransition.Apply(
            input.Context, input.Administration, input.WorldEvents, null!));

        Type transitionType = typeof(StabilizingRemedyAdministrationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(DiseaseTruthSnapshot), method.ReturnType);
        Assert.Equal(
            [
                typeof(FirstPatientTreatmentContext),
                typeof(IngredientAdministrationOutcome),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
    }
}
