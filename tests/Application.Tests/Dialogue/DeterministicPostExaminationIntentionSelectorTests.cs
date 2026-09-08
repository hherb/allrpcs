using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class DeterministicPostExaminationIntentionSelectorTests
{
    [Fact]
    public void SelectsAFreshCollectionOfTheExactTargetOnlyWhenSafe()
    {
        CharacterTurnWorldEventOccurrence occurrence = ExaminationOccurrence();
        CharacterExaminationResult result = new(
            occurrence,
            CharacterExaminationSafety.SafeToCollect);

        CharacterIntention first =
            DeterministicPostExaminationIntentionSelector.Select(result);
        CharacterIntention second =
            DeterministicPostExaminationIntentionSelector.Select(result);

        Assert.NotSame(first, second);
        Assert.Same(CharacterIntentionType.CollectObject, first.Type);
        Assert.Same(result.TargetObjectId, first.TargetObjectId);
        Assert.Same(occurrence, result.Occurrence);
        Assert.Same(CharacterExaminationSafety.SafeToCollect, result.Safety);
    }

    [Fact]
    public void SelectsAFreshWaitWithoutATargetWhenUnsafe()
    {
        CharacterTurnWorldEventOccurrence occurrence = ExaminationOccurrence();
        CharacterExaminationResult result = new(
            occurrence,
            CharacterExaminationSafety.UnsafeToCollect);

        CharacterIntention first =
            DeterministicPostExaminationIntentionSelector.Select(result);
        CharacterIntention second =
            DeterministicPostExaminationIntentionSelector.Select(result);

        Assert.NotSame(first, second);
        Assert.Same(CharacterIntentionType.Wait, first.Type);
        Assert.Null(first.TargetObjectId);
        Assert.Same(occurrence, result.Occurrence);
        Assert.Same(CharacterExaminationSafety.UnsafeToCollect, result.Safety);
    }

    [Fact]
    public void RejectsANullResult()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DeterministicPostExaminationIntentionSelector.Select(null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureSelectionOperation()
    {
        Type selectorType = typeof(DeterministicPostExaminationIntentionSelector);
        System.Reflection.MethodInfo method = Assert.Single(
            selectorType.GetMethods().Where(candidate => candidate.DeclaringType == selectorType));

        Assert.Equal("Select", method.Name);
        Assert.Equal(typeof(CharacterIntention), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterExaminationResult),
                parameter.ParameterType));
        Assert.Empty(selectorType.GetConstructors());
        Assert.Empty(selectorType.GetProperties());
        Assert.Empty(selectorType.GetEvents());
        Assert.True(selectorType.IsAbstract);
        Assert.True(selectorType.IsSealed);
    }

    private static CharacterTurnWorldEventOccurrence ExaminationOccurrence()
    {
        CharacterIntention examination = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("flowers"));
        return new CharacterTurnWorldEventOccurrence(
            new WorldEvent(
                new WorldEventId("healer-examined-flowers"),
                WorldEventTimeEffect.Advance),
            new CharacterTurnTranscriptEntry(new CharacterId("healer"), examination));
    }
}
