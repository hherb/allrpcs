using Allrpcs.Domain.Collections;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Persistent exact-key crit-bit index. Nonzero six-bit canonical symbols and zero padding form
/// an injective 384-bit key. Only discriminating bits become branches; no hash collisions exist.
/// </summary>
internal sealed class WorldEventIdIndex
{
    private readonly Node? root;
    private readonly AppendWork? work;

    internal WorldEventIdIndex(AppendWork? work = null) : this(null, work) { }
    private WorldEventIdIndex(Node? root, AppendWork? work) { this.root = root; this.work = work; }

    internal WorldEventId? Find(WorldEventId query)
    {
        Leaf? leaf = FindLeaf(query.Value);
        return leaf is not null && FirstDifference(leaf.Id.Value, query.Value) < 0 ? leaf.Id : null;
    }

    internal WorldEventIdIndex Add(WorldEventId id)
    {
        Leaf? existing = FindLeaf(id.Value);
        if (existing is null) { return new(NewLeaf(id), work); }
        int difference = FirstDifference(existing.Id.Value, id.Value);
        if (difference < 0) { throw new ArgumentException("Duplicate world-event identifier.", nameof(id)); }

        Stack<(Branch Branch, bool One)> path = new();
        Node current = root!;
        while (current is Branch branch)
        {
            Visit();
            if (branch.Bit >= difference) { break; }
            bool one = IsOne(id.Value, branch.Bit);
            path.Push((branch, one));
            current = one ? branch.One : branch.Zero;
        }
        Leaf added = NewLeaf(id);
        Node updated = IsOne(id.Value, difference)
            ? NewBranch(difference, current, added)
            : NewBranch(difference, added, current);
        while (path.TryPop(out (Branch Branch, bool One) item))
        {
            updated = item.One
                ? NewBranch(item.Branch.Bit, item.Branch.Zero, updated)
                : NewBranch(item.Branch.Bit, updated, item.Branch.One);
        }
        return new(updated, work);
    }

    private Leaf? FindLeaf(string key)
    {
        Node? current = root;
        while (current is Branch branch)
        {
            Visit();
            current = IsOne(key, branch.Bit) ? branch.One : branch.Zero;
        }
        return current as Leaf;
    }

    private int FirstDifference(string left, string right)
    {
        for (int index = 0; index < Math.Max(left.Length, right.Length); index++)
        {
            if (work is not null) { work.KeySymbols++; }
            int difference = Symbol(left, index) ^ Symbol(right, index);
            if (difference == 0) { continue; }
            for (int bit = 0; bit < 6; bit++)
            {
                if ((difference & (1 << (5 - bit))) != 0) { return index * 6 + bit; }
            }
        }
        return -1;
    }

    private static bool IsOne(string key, int bit) => (Symbol(key, bit / 6) & (1 << (5 - bit % 6))) != 0;

    private static int Symbol(string key, int index)
    {
        if (index >= key.Length) { return 0; }
        char value = key[index];
        return value is >= 'a' and <= 'z' ? value - 'a' + 1
            : value is >= '0' and <= '9' ? value - '0' + 27 : 37;
    }

    private void Visit() { if (work is not null) { work.IndexVisits++; } }
    private Leaf NewLeaf(WorldEventId id) { if (work is not null) { work.IndexNodes++; } return new(id); }
    private Branch NewBranch(int bit, Node zero, Node one)
    {
        if (work is not null) { work.IndexNodes++; }
        return new(bit, zero, one);
    }

    private abstract class Node;
    private sealed class Leaf(WorldEventId id) : Node { internal WorldEventId Id { get; } = id; }
    private sealed class Branch(int bit, Node zero, Node one) : Node
    {
        internal int Bit { get; } = bit;
        internal Node Zero { get; } = zero;
        internal Node One { get; } = one;
    }
}
