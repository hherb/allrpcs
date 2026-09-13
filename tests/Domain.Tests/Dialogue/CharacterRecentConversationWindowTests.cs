using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterRecentConversationWindowTests
{
    [Fact]
    public void EmptyWindowIsExplicitlyValidAndRetainsExactCharacter()
    {
        CharacterId characterId = new("healer");

        CharacterRecentConversationWindow window = new(characterId, []);

        Assert.Same(characterId, window.CharacterId);
        Assert.Empty(window.Turns);
    }

    [Fact]
    public void PreservesOrderExactInstancesAndDistinctValueEqualCharacters()
    {
        CharacterRecentConversationTurn first = Turn("turn-one", "First?", "First.");
        CharacterRecentConversationTurn second = Turn("turn-two", "Second?", "Second.");
        CharacterId distinctEqualCharacterId = new("healer");

        CharacterRecentConversationWindow window = new(
            distinctEqualCharacterId,
            [first, second]);

        Assert.Same(distinctEqualCharacterId, window.CharacterId);
        Assert.Collection(
            window.Turns,
            turn => Assert.Same(first, turn),
            turn => Assert.Same(second, turn));
    }

    [Fact]
    public void CopiesTheCallerCollectionAndExposesItReadOnly()
    {
        CharacterRecentConversationTurn original = Turn("turn-one", "First?", "First.");
        CharacterRecentConversationTurn replacement = Turn("turn-two", "Second?", "Second.");
        CharacterRecentConversationTurn[] source = [original];

        CharacterRecentConversationWindow window = new(new CharacterId("healer"), source);
        source[0] = replacement;

        Assert.Same(original, Assert.Single(window.Turns));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterRecentConversationTurn>)window.Turns).Add(replacement));
    }

    [Fact]
    public void AcceptsExactCountAndAggregateTextBounds()
    {
        string text = new('a', DialogueUtterance.MaximumTextLength);
        CharacterRecentConversationTurn[] maximumText = Enumerable
            .Range(0, CharacterRecentConversationWindow.MaximumTextLength / (text.Length * 2))
            .Select(index => Turn($"text-{index}", text, text))
            .ToArray();
        CharacterRecentConversationTurn[] maximumCount = Enumerable
            .Range(0, CharacterRecentConversationWindow.MaximumTurnCount)
            .Select(index => Turn($"turn-{index}", "a", "b"))
            .ToArray();

        CharacterRecentConversationWindow textWindow = new(
            new CharacterId("healer"),
            maximumText);
        CharacterRecentConversationWindow countWindow = new(
            new CharacterId("healer"),
            maximumCount);

        Assert.Equal(maximumText.Length, textWindow.Turns.Count);
        Assert.Equal(CharacterRecentConversationWindow.MaximumTurnCount, countWindow.Turns.Count);
    }

    [Fact]
    public void RejectsExcessCountOrAggregateText()
    {
        CharacterRecentConversationTurn[] excessCount = Enumerable
            .Range(0, CharacterRecentConversationWindow.MaximumTurnCount + 1)
            .Select(index => Turn($"turn-{index}", "a", "b"))
            .ToArray();
        string maximumUtterance = new('a', DialogueUtterance.MaximumTextLength);
        int boundedTurnCount = CharacterRecentConversationWindow.MaximumTextLength
            / (maximumUtterance.Length * 2);
        CharacterRecentConversationTurn[] excessText = Enumerable
            .Range(0, boundedTurnCount + 1)
            .Select(index => Turn($"text-{index}", maximumUtterance, maximumUtterance))
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRecentConversationWindow(
            new CharacterId("healer"),
            excessCount));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRecentConversationWindow(
            new CharacterId("healer"),
            excessText));
    }

    [Fact]
    public void RejectsNullMismatchedAndDuplicateIdentities()
    {
        CharacterRecentConversationTurn healerTurn = Turn("turn-one", "Hello?", "Hello.");
        CharacterRecentConversationTurn otherTurn = Turn(
            "turn-two",
            "Hello?",
            "Hello.",
            "hermit");
        CharacterRecentConversationTurn duplicateTurn = Turn(
            "turn-one",
            "Again?",
            "Again.");

        Assert.Throws<ArgumentNullException>(() => new CharacterRecentConversationWindow(
            null!,
            []));
        Assert.Throws<ArgumentNullException>(() => new CharacterRecentConversationWindow(
            new CharacterId("healer"),
            null!));
        Assert.Throws<ArgumentException>(() => new CharacterRecentConversationWindow(
            new CharacterId("healer"),
            [null!]));
        Assert.Throws<ArgumentException>(() => new CharacterRecentConversationWindow(
            new CharacterId("healer"),
            [otherTurn]));
        Assert.Throws<ArgumentException>(() => new CharacterRecentConversationWindow(
            new CharacterId("healer"),
            [healerTurn, duplicateTurn]));
    }

    [Fact]
    public void ExposesOnlyTheBoundedReadOnlyWindowSurface()
    {
        Type windowType = typeof(CharacterRecentConversationWindow);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CharacterId", typeof(CharacterId)),
            ("Turns", typeof(IReadOnlyList<CharacterRecentConversationTurn>)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = windowType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(windowType.GetEvents());
        Assert.True(windowType.IsSealed);
    }

    internal static CharacterRecentConversationTurn Turn(
        string turnId,
        string heardText,
        string responseText,
        string characterId = "healer")
    {
        CharacterTurnRequest request = Request(characterId, heardText);
        CharacterTurnActivity activity = new(
            request,
            CharacterRecentConversationTurnTests.CreateResponse(request, responseText),
            CharacterTurnActivityState.Completed);
        return new CharacterRecentConversationTurn(new CharacterTurnId(turnId), activity);
    }

    private static CharacterTurnRequest Request(string characterId, string text)
    {
        CharacterId seekerId = new("seeker");
        CharacterId responderId = new(characterId);
        DialogueUtterance utterance = new(seekerId, text);
        return new CharacterTurnRequest(
            responderId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [responderId])),
            new DialogueAct(utterance, DialogueActType.Request));
    }
}
