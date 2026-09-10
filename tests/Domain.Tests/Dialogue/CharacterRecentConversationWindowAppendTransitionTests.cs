using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterRecentConversationWindowAppendTransitionTests
{
    [Fact]
    public void AppendsExactTurnAfterStableRetainedTurns()
    {
        CharacterRecentConversationTurn first = Turn("turn-one", "First?", "First.");
        CharacterRecentConversationTurn second = Turn("turn-two", "Second?", "Second.");
        CharacterRecentConversationWindow original = new(new CharacterId("healer"), [first]);

        CharacterRecentConversationWindow result =
            CharacterRecentConversationWindowAppendTransition.Append(second, original);

        Assert.NotSame(original, result);
        Assert.Same(original.CharacterId, result.CharacterId);
        Assert.Same(first, Assert.Single(original.Turns));
        Assert.Collection(
            result.Turns,
            turn => Assert.Same(first, turn),
            turn => Assert.Same(second, turn));
    }

    [Fact]
    public void EvictsTheOldestTurnWhenAppendingAtTheCountBound()
    {
        CharacterRecentConversationTurn[] existing = Enumerable
            .Range(0, CharacterRecentConversationWindow.MaximumTurnCount)
            .Select(index => Turn($"turn-{index}", "a", "b"))
            .ToArray();
        CharacterRecentConversationTurn appended = Turn("appended", "c", "d");
        CharacterRecentConversationWindow original = new(new CharacterId("healer"), existing);

        CharacterRecentConversationWindow result =
            CharacterRecentConversationWindowAppendTransition.Append(appended, original);

        Assert.Equal(CharacterRecentConversationWindow.MaximumTurnCount, result.Turns.Count);
        Assert.Same(existing[1], result.Turns[0]);
        Assert.Same(existing[^1], result.Turns[^2]);
        Assert.Same(appended, result.Turns[^1]);
        Assert.Same(existing[0], original.Turns[0]);
    }

    [Fact]
    public void EvictsOldestTurnsUntilTheAggregateTextFits()
    {
        int maximum = DialogueUtterance.MaximumTextLength;
        CharacterRecentConversationTurn first = Turn(
            "turn-one",
            new string('a', maximum),
            new string('a', maximum));
        CharacterRecentConversationTurn second = Turn(
            "turn-two",
            new string('b', maximum),
            new string('b', maximum));
        CharacterRecentConversationTurn third = Turn(
            "turn-three",
            new string('c', maximum),
            new string('c', maximum));
        CharacterRecentConversationTurn fourth = Turn(
            "turn-four",
            new string('d', maximum),
            new string('d', maximum));
        CharacterRecentConversationTurn appended = Turn(
            "appended",
            new string('e', maximum),
            new string('e', maximum));
        CharacterRecentConversationWindow original = new(
            new CharacterId("healer"),
            [first, second, third, fourth]);

        CharacterRecentConversationWindow result =
            CharacterRecentConversationWindowAppendTransition.Append(appended, original);

        Assert.Collection(
            result.Turns,
            turn => Assert.Same(second, turn),
            turn => Assert.Same(third, turn),
            turn => Assert.Same(fourth, turn),
            turn => Assert.Same(appended, turn));
    }

    [Fact]
    public void RejectsNullMismatchedAndDuplicateInputsWithoutChangingTheWindow()
    {
        CharacterRecentConversationTurn retained = Turn("turn-one", "First?", "First.");
        CharacterRecentConversationWindow window = new(new CharacterId("healer"), [retained]);
        CharacterRecentConversationTurn mismatched =
            CharacterRecentConversationWindowTests.Turn(
                "turn-two",
                "Other?",
                "Other.",
                "hermit");
        CharacterRecentConversationTurn duplicate = Turn("turn-one", "Again?", "Again.");

        Assert.Throws<ArgumentNullException>(() =>
            CharacterRecentConversationWindowAppendTransition.Append(null!, window));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterRecentConversationWindowAppendTransition.Append(retained, null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterRecentConversationWindowAppendTransition.Append(mismatched, window));
        Assert.Throws<ArgumentException>(() =>
            CharacterRecentConversationWindowAppendTransition.Append(duplicate, window));
        Assert.Same(retained, Assert.Single(window.Turns));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureAppendOperation()
    {
        Type transitionType = typeof(CharacterRecentConversationWindowAppendTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Append", method.Name);
        Assert.Equal(typeof(CharacterRecentConversationWindow), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterRecentConversationTurn),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterRecentConversationWindow),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static CharacterRecentConversationTurn Turn(
        string turnId,
        string heardText,
        string responseText) =>
        CharacterRecentConversationWindowTests.Turn(turnId, heardText, responseText);
}
