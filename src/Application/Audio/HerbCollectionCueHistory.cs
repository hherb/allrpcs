namespace Allrpcs.Application.Audio;

/// <summary>Independent bounded anonymous local records; the live receiver owns token deduplication.</summary>
public sealed class HerbCollectionCueHistory
{
    /// <summary>Oldest-first capacity, independent of all prior histories.</summary>
    public const int Capacity = 16;
    private readonly List<string> entries = [];
    private long sequence;
    /// <summary>A detached read-only snapshot, without coordinates or world timestamps.</summary>
    public IReadOnlyList<string> Entries => Array.AsReadOnly(entries.ToArray());
    /// <summary>Records one admitted unique commit without discarding rapid legitimate commits.</summary>
    public bool Record()
    {
        if (sequence == long.MaxValue) { return false; }
        entries.Add($"{++sequence}. {HerbCollectionPresentationProjection.Phrase}");
        if (entries.Count > Capacity) { entries.RemoveAt(0); }
        return true;
    }
    /// <summary>Successful reset/teardown clears only this owner's list and local sequence.</summary>
    public void Clear() { entries.Clear(); sequence = 0; }
}
