using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Diseases;

public sealed class EffectiveCharacterControllerSelectorTests
{
    [Theory]
    [MemberData(nameof(NonDormantStages))]
    public void RetainsExactConfiguredAssignmentBeforeDeepSleep(DiseaseStage stage)
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Llm);
        DiseaseDefinition disease = Definition();
        DiseaseTruthSnapshot truth = Truth(State(configured.CharacterId, disease, stage), disease);

        CharacterControllerAssignment result = EffectiveCharacterControllerSelector.Select(
            configured,
            truth);

        Assert.Same(configured, result);
    }

    [Theory]
    [MemberData(nameof(DormantStages))]
    public void SelectsDormantWithoutReplacingConfiguredAssignment(DiseaseStage stage)
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Llm);
        DiseaseDefinition disease = Definition();
        DiseaseTruthSnapshot truth = Truth(State(configured.CharacterId, disease, stage), disease);

        CharacterControllerAssignment result = EffectiveCharacterControllerSelector.Select(
            configured,
            truth);

        Assert.NotSame(configured, result);
        Assert.Same(configured.CharacterId, result.CharacterId);
        Assert.Same(ControllerType.Llm, configured.ControllerType);
        Assert.Same(ControllerType.Dormant, result.ControllerType);
    }

    [Fact]
    public void AnyDeepDiseaseMakesTheEffectiveControllerDormant()
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Human);
        DiseaseDefinition earlyDisease = Definition("marsh-fever");
        DiseaseDefinition deepDisease = Definition("grey-sleep");
        DiseaseTruthSnapshot truth = new(
            new DiseaseDefinitionSnapshot([earlyDisease, deepDisease]),
            [
                State(configured.CharacterId, earlyDisease, DiseaseStage.EarlyIllness),
                State(configured.CharacterId, deepDisease, DiseaseStage.DeepSleep),
            ]);

        Assert.Same(
            ControllerType.Dormant,
            EffectiveCharacterControllerSelector.Select(configured, truth).ControllerType);
    }

    [Fact]
    public void RestoresExactConfiguredControllerAfterLastDeepInfectionIsCured()
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Llm);
        DiseaseDefinition disease = Definition();
        CharacterDiseaseState infection = State(
            configured.CharacterId,
            disease,
            DiseaseStage.DeepSleep);
        DiseaseTruthSnapshot truth = Truth(infection, disease);
        WorldEvent cureEvent = new(new WorldEventId("patient-cured"), WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot worldEvents = new([cureEvent]);
        WorldEventLogSnapshot eventLog = new([cureEvent.EventId]);

        DiseaseTruthSnapshot cured = DiseaseCureTransition.Apply(
            new SuccessfulDiseaseCureOutcome(infection, cureEvent),
            truth,
            worldEvents,
            eventLog);

        Assert.Same(configured, EffectiveCharacterControllerSelector.Select(configured, cured));
    }

    [Fact]
    public void AnotherDeepInfectionKeepsControllerDormantAfterOneCure()
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Llm);
        DiseaseDefinition curedDisease = Definition("grey-sleep");
        DiseaseDefinition retainedDisease = Definition("marsh-fever");
        CharacterDiseaseState curedInfection = State(
            configured.CharacterId,
            curedDisease,
            DiseaseStage.DeepSleep);
        CharacterDiseaseState retainedInfection = State(
            configured.CharacterId,
            retainedDisease,
            DiseaseStage.Wasting);
        DiseaseTruthSnapshot truth = new(
            new DiseaseDefinitionSnapshot([curedDisease, retainedDisease]),
            [curedInfection, retainedInfection]);
        WorldEvent cureEvent = new(new WorldEventId("grey-sleep-cured"), WorldEventTimeEffect.NoAdvance);

        DiseaseTruthSnapshot cured = DiseaseCureTransition.Apply(
            new SuccessfulDiseaseCureOutcome(curedInfection, cureEvent),
            truth,
            new WorldEventSnapshot([cureEvent]),
            new WorldEventLogSnapshot([cureEvent.EventId]));

        Assert.Same(
            ControllerType.Dormant,
            EffectiveCharacterControllerSelector.Select(configured, cured).ControllerType);
        Assert.Same(retainedInfection, Assert.Single(cured.Infections));
    }

    [Fact]
    public void RetainsConfiguredControllerWhenTruthHasNoMatchingInfection()
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Human);
        DiseaseDefinition disease = Definition();
        DiseaseTruthSnapshot truth = Truth(
            State(new CharacterId("healer"), disease, DiseaseStage.DeepSleep),
            disease);

        Assert.Same(configured, EffectiveCharacterControllerSelector.Select(configured, truth));
        Assert.Same(
            configured,
            EffectiveCharacterControllerSelector.Select(
                configured,
                new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot([]), [])));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlySelection()
    {
        CharacterControllerAssignment configured = Assignment("patient", ControllerType.Llm);
        DiseaseTruthSnapshot truth = new(new DiseaseDefinitionSnapshot([]), []);

        Assert.Throws<ArgumentNullException>(() =>
            EffectiveCharacterControllerSelector.Select(null!, truth));
        Assert.Throws<ArgumentNullException>(() =>
            EffectiveCharacterControllerSelector.Select(configured, null!));

        Type selectorType = typeof(EffectiveCharacterControllerSelector);
        System.Reflection.MethodInfo method = Assert.Single(
            selectorType.GetMethods().Where(candidate => candidate.DeclaringType == selectorType));

        Assert.Equal("Select", method.Name);
        Assert.Equal(typeof(CharacterControllerAssignment), method.ReturnType);
        Assert.Empty(selectorType.GetConstructors());
        Assert.Empty(selectorType.GetProperties());
        Assert.Empty(selectorType.GetEvents());
        Assert.True(selectorType.IsAbstract);
        Assert.True(selectorType.IsSealed);
    }

    public static TheoryData<DiseaseStage> NonDormantStages => new()
    {
        DiseaseStage.Exposed,
        DiseaseStage.EarlyIllness,
        DiseaseStage.Drowsing,
    };

    public static TheoryData<DiseaseStage> DormantStages => new()
    {
        DiseaseStage.DeepSleep,
        DiseaseStage.Wasting,
        DiseaseStage.FinalDecline,
    };

    private static CharacterControllerAssignment Assignment(
        string characterId,
        ControllerType controllerType) =>
        new(new CharacterId(characterId), controllerType);

    private static DiseaseDefinition Definition(string id = "grey-sleep") =>
        new(new DiseaseId(id));

    private static CharacterDiseaseState State(
        CharacterId characterId,
        DiseaseDefinition disease,
        DiseaseStage stage) =>
        new(characterId, disease, stage, null);

    private static DiseaseTruthSnapshot Truth(
        CharacterDiseaseState state,
        DiseaseDefinition disease) =>
        new(new DiseaseDefinitionSnapshot([disease]), [state]);
}
