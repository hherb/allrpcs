using System.Reflection;
using Allrpcs.Domain.Collections;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests;

public sealed class LongSessionAppendTests
{
    [Fact]
    public void MembershipAndCanonicalLookupNeverScanTheOrderedEntries()
    {
        WorldEventId canonical = new("retained");
        WorldEventLogSnapshot log = new([canonical]);
        ObservedEntries observed = new(log.Entries);
        typeof(WorldEventLogSnapshot).GetField("<Entries>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(log, observed);
        Assert.True(WorldEventLogAppendPolicy.CanAppend(new("absent"), log));
        Assert.False(WorldEventLogAppendPolicy.CanAppend(new("retained"), log));
        Assert.Same(canonical, WorldEventLogLookup.Find(new("retained"), log));
        Assert.Equal(0, observed.Reads);
    }

    private sealed class ObservedEntries(IReadOnlyList<WorldEventId> entries) : IReadOnlyList<WorldEventId>
    {
        internal int Reads { get; private set; }
        public int Count => entries.Count;
        public WorldEventId this[int index] { get { Reads++; return entries[index]; } }
        public IEnumerator<WorldEventId> GetEnumerator()
        {
            foreach (WorldEventId entry in entries) { Reads++; yield return entry; }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void EventSnapshotsRetainOnlyBoundedCopiedElementStorage()
    {
        List<WorldEventLogSnapshot> snapshots = [new([])];
        for (int index = 0; index < 257; index++)
        {
            snapshots.Add(WorldEventLogAppendTransition.Append(new($"event-{index}"), snapshots[^1]));
        }
        Assert.InRange(CountElementSlots<WorldEventId>(snapshots.Select(snapshot => snapshot.Entries)), 0, 32 * 257);
        Assert.Empty(snapshots[0].Entries);
        Assert.Equal(257, snapshots[^1].Entries.Count);
    }

    [Fact]
    public void TranscriptSnapshotsRetainOnlyBoundedCopiedElementStorage()
    {
        CharacterTurnTranscriptEntry entry = new(new DialogueDelivery(
            new DialogueUtterance(new CharacterId("healer"), "Hello."), DialogueDeliveryMode.OrdinarySpeech));
        List<CharacterTurnTranscriptSnapshot> snapshots = [new([])];
        for (int index = 0; index < 257; index++)
        {
            snapshots.Add(CharacterTurnTranscriptAppendTransition.Append(entry, snapshots[^1]));
        }
        Assert.InRange(CountElementSlots<CharacterTurnTranscriptEntry>(snapshots.Select(snapshot => snapshot.Entries)), 0, 32 * 257);
        Assert.Empty(snapshots[0].Entries);
        Assert.All(snapshots[^1].Entries, retained => Assert.Same(entry, retained));
    }

    [Fact]
    public void PublicEventAppendBoundsAllStorageAndMembershipWorkAcrossHistoricalBranches()
    {
        AppendWork work = new();
        WorldEventLogSnapshot log = new([], work);
        List<WorldEventLogSnapshot> retained = [log];
        List<WorldEventId> ids = [];
        for (int index = 0; index < 4097; index++)
        {
            WorldEventId id = new($"herb-patch-examination-{index}");
            long visits = work.IndexVisits;
            long nodes = work.IndexNodes;
            long slots = work.ElementSlots;
            long symbols = work.KeySymbols;
            log = WorldEventLogAppendTransition.Append(id, log);
            Assert.InRange(work.IndexVisits - visits, 0, 1152);
            Assert.InRange(work.IndexNodes - nodes, 1, 385);
            Assert.InRange(work.ElementSlots - slots, 1, 32);
            Assert.InRange(work.KeySymbols - symbols, 0, 128);
            ids.Add(id);
            if (index < 129 || index % 127 == 0) { retained.Add(log); }
        }
        Assert.InRange(work.ElementSlots, 4097, 32L * 4097);
        Assert.InRange(work.Trees, 0, 4097);
        Assert.Equal(work.Trees, work.Links);
        Assert.InRange(work.IndexNodes, 4097, 385L * 4097);
        Assert.InRange(work.IndexVisits, 0, 1152L * 4097);
        AssertIndexShape(log, ids.Count);
        long emitted = work.EmittedElements;
        long trees = work.EnumeratedTrees;
        Assert.Equal(ids, log.Entries.ToArray());
        Assert.Equal(ids.Count, work.EmittedElements - emitted);
        Assert.InRange(work.EnumeratedTrees - trees, 0, ids.Count / 32);
        for (int index = 0; index < ids.Count; index++)
        {
            Assert.Same(ids[index], log.Entries[index]);
            Assert.Same(ids[index], WorldEventLogLookup.Find(new(ids[index].Value), log));
        }
        foreach (WorldEventLogSnapshot old in retained)
        {
            Assert.Equal(ids.Take(old.Entries.Count), old.Entries);
            WorldEventId branchId = new("branch-event");
            long visits = work.IndexVisits;
            long nodes = work.IndexNodes;
            long slots = work.ElementSlots;
            WorldEventLogSnapshot branch = WorldEventLogAppendTransition.Append(branchId, old);
            Assert.InRange(work.IndexVisits - visits, 0, 1152);
            Assert.InRange(work.IndexNodes - nodes, 1, 385);
            Assert.InRange(work.ElementSlots - slots, 1, 32);
            Assert.Same(branchId, branch.Entries[^1]);
            Assert.Null(WorldEventLogLookup.Find(branchId, old));
            Assert.Null(WorldEventLogLookup.Find(branchId, log));
            AssertIndexShape(branch, old.Entries.Count + 1);
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => log.Entries[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => log.Entries[log.Entries.Count]);
    }

    [Fact]
    public void CanonicalIndexDistinguishesPrefixesAllSymbolsAndMaximumLengthKeys()
    {
        string[] keys = ["a",
            "aa",
            "ab",
            "z",
            "za",
            "a-a",
            "a0",
            "a9",
            new string('a', 64),
            new string('a', 63) + "b",
            new string('a', 63) + "0"];
        keys = keys.Concat("abcdefghijklmnopqrstuvwxyz0123456789".Select(symbol => $"prefix-{symbol}"))
            .Distinct().ToArray();
        foreach (IEnumerable<string> order in new[] { keys, keys.Reverse().ToArray(), keys.Order().ToArray() })
        {
            WorldEventLogSnapshot log = new([]);
            foreach (string key in order)
            {
                WorldEventId canonical = new(key);
                WorldEventLogSnapshot prior = log;
                log = WorldEventLogAppendTransition.Append(canonical, log);
                Assert.Same(canonical, WorldEventLogLookup.Find(new(key), log));
                Assert.Null(WorldEventLogLookup.Find(new(key), prior));
                Assert.False(WorldEventLogAppendPolicy.CanAppend(new(key), log));
                Assert.Throws<InvalidOperationException>(() => WorldEventLogAppendTransition.Append(new(key), log));
                AssertIndexShape(log, log.Entries.Count);
            }
            foreach (string key in keys) { Assert.Equal(key, WorldEventLogLookup.Find(new(key), log)!.Value); }
            Assert.Null(WorldEventLogLookup.Find(new("absent"), log));
        }
    }

    [Fact]
    public void TranscriptPreservesOrderingAndBranchesWithBoundedAppendAndLinearEnumeration()
    {
        AppendWork work = new();
        CharacterTurnTranscriptSnapshot transcript = new([], work);
        List<CharacterTurnTranscriptEntry> expected = [];
        List<CharacterTurnTranscriptSnapshot> old = [];
        for (int index = 0; index < 4097; index++)
        {
            CharacterTurnTranscriptEntry entry = new(new DialogueDelivery(
                new DialogueUtterance(new CharacterId("healer"), $"Occurrence {index}."),
                DialogueDeliveryMode.OrdinarySpeech));
            expected.Add(entry);
            long slots = work.ElementSlots;
            long trees = work.Trees;
            transcript = CharacterTurnTranscriptAppendTransition.Append(entry, transcript);
            Assert.InRange(work.ElementSlots - slots, 1, 32);
            Assert.InRange(work.Trees - trees, 0, 1);
            if (index < 129 || index % 127 == 0) { old.Add(transcript); }
        }
        long emitted = work.EmittedElements;
        long traversed = work.EnumeratedTrees;
        Assert.Equal(expected, transcript.Entries.ToArray());
        Assert.Equal(4097, work.EmittedElements - emitted);
        Assert.InRange(work.EnumeratedTrees - traversed, 0, 4097 / 32);
        for (int index = 0; index < expected.Count; index++) { Assert.Same(expected[index], transcript.Entries[index]); }
        foreach (CharacterTurnTranscriptSnapshot snapshot in old)
        {
            Assert.Equal(expected.Take(snapshot.Entries.Count), snapshot.Entries);
            for (int attempt = 0; attempt < 3; attempt++)
            {
                long slots = work.ElementSlots;
                long trees = work.Trees;
                CharacterTurnTranscriptSnapshot branch = CharacterTurnTranscriptAppendTransition.Append(expected[0], snapshot);
                Assert.InRange(work.ElementSlots - slots, 1, 32);
                Assert.InRange(work.Trees - trees, 0, 1);
                Assert.Same(expected[0], branch.Entries[^1]);
                Assert.Equal(snapshot.Entries.Count + 1, branch.Entries.Count);
            }
        }
    }

    private static void AssertIndexShape(WorldEventLogSnapshot log, int count)
    {
        object index = typeof(WorldEventLogSnapshot).GetField("index", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(log)!;
        object root = index.GetType().GetField("root", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(index)!;
        Assert.Equal(2 * count - 1, Visit(root, -1));
        static int Visit(object node, int previousBit)
        {
            PropertyInfo? bit = node.GetType().GetProperty("Bit", BindingFlags.Instance | BindingFlags.NonPublic);
            if (bit is null) { return 1; }
            int position = (int)bit.GetValue(node)!;
            Assert.InRange(position, previousBit + 1, 383);
            object zero = node.GetType().GetProperty("Zero", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(node)!;
            object one = node.GetType().GetProperty("One", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(node)!;
            Assert.NotNull(zero);
            Assert.NotNull(one);
            return 1 + Visit(zero, position) + Visit(one, position);
        }
    }

    private static int CountElementSlots<T>(IEnumerable<object> roots)
    {
        HashSet<object> seen = new(ReferenceEqualityComparer.Instance);
        Queue<object> pending = new(roots);
        int slots = 0;
        while (pending.TryDequeue(out object? value))
        {
            if (!seen.Add(value) || value is T || value is string) { continue; }
            if (value is T[] elements) { slots += elements.Length; continue; }
            for (Type? type = value.GetType(); type is not null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (!field.FieldType.IsValueType && field.GetValue(value) is object child) { pending.Enqueue(child); }
                }
            }
        }
        return slots;
    }
}
