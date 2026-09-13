using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnTranscriptAppendTransitionTests
{
    [Fact]
    public void AppendsExactEntryAfterStableRetainedEntries()
    {
        CharacterTurnTranscriptEntry first = Speech("I will inspect it.");
        CharacterTurnTranscriptEntry second = Action(CharacterIntentionType.ExamineObject);
        CharacterTurnTranscriptEntry appended = Speech("It appears safe.");
        CharacterTurnTranscriptSnapshot original = new([first, second]);

        CharacterTurnTranscriptSnapshot result =
            CharacterTurnTranscriptAppendTransition.Append(appended, original);

        Assert.NotSame(original, result);
        Assert.Collection(
            original.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(appended, entry));
    }

    [Fact]
    public void AllowsRepeatedOccurrencesOfTheExactEntry()
    {
        CharacterTurnTranscriptEntry repeated = Speech("Careful.");
        CharacterTurnTranscriptSnapshot original = new([repeated]);

        CharacterTurnTranscriptSnapshot result =
            CharacterTurnTranscriptAppendTransition.Append(repeated, original);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(repeated, entry),
            entry => Assert.Same(repeated, entry));
    }

    [Fact]
    public void RejectsNullInputsWithoutChangingTheSnapshot()
    {
        CharacterTurnTranscriptEntry retained = Speech("Remain here.");
        CharacterTurnTranscriptSnapshot transcript = new([retained]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnTranscriptAppendTransition.Append(null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnTranscriptAppendTransition.Append(retained, null!));
        Assert.Same(retained, Assert.Single(transcript.Entries));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureAppendOperation()
    {
        Type transitionType = typeof(CharacterTurnTranscriptAppendTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Append", method.Name);
        Assert.Equal(typeof(CharacterTurnTranscriptSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterTurnTranscriptEntry), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterTurnTranscriptSnapshot),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static CharacterTurnTranscriptEntry Speech(string text) =>
        new(new DialogueDelivery(
            new DialogueUtterance(new CharacterId("healer"), text),
            DialogueDeliveryMode.OrdinarySpeech));

    private static CharacterTurnTranscriptEntry Action(CharacterIntentionType type) =>
        new(
            new CharacterId("healer"),
            new CharacterIntention(type, new ObjectId("moonwort")));
}
