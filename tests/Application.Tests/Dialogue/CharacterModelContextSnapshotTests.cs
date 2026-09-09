using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextSnapshotTests
{
    [Fact]
    public void RetainsExactEntriesInStableOrderAndCopiesInput()
    {
        CharacterModelContextEntry first = Entry("first");
        CharacterModelContextEntry second = Entry("second");
        CharacterModelContextEntry[] source = [first, second];

        CharacterModelContextSnapshot snapshot = new(source);
        source[0] = Entry("changed");

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void AcceptsEmptyAndBoundarySizedContext()
    {
        CharacterModelContextEntry[] entries = Enumerable
            .Range(0, CharacterModelContextSnapshot.MaximumEntryCount)
            .Select(index => Entry(index.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            .ToArray();

        CharacterModelContextSnapshot empty = new([]);
        CharacterModelContextSnapshot boundary = new(entries);

        Assert.Empty(empty.Entries);
        Assert.Equal(CharacterModelContextSnapshot.MaximumEntryCount, boundary.Entries.Count);
    }

    [Fact]
    public void RejectsTooManyEntriesAndExcessiveCombinedText()
    {
        CharacterModelContextEntry[] tooMany = Enumerable
            .Range(0, CharacterModelContextSnapshot.MaximumEntryCount + 1)
            .Select(index => Entry(index.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            .ToArray();
        CharacterModelContextEntry[] tooLong = Enumerable
            .Range(0, (CharacterModelContextSnapshot.MaximumTotalTextLength
                / CharacterModelContextEntry.MaximumTextLength) + 1)
            .Select(_ => Entry(new string('x', CharacterModelContextEntry.MaximumTextLength)))
            .ToArray();

        Assert.Throws<ArgumentException>(() => new CharacterModelContextSnapshot(tooMany));
        Assert.Throws<ArgumentException>(() => new CharacterModelContextSnapshot(tooLong));
    }

    [Fact]
    public void RejectsNullInputAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterModelContextSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelContextSnapshot([null!]));
    }

    private static CharacterModelContextEntry Entry(string text) => new(
        CharacterModelContextEntryKind.Goal,
        text);
}
