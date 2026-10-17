using System.Collections;

namespace Allrpcs.Domain.Collections;

/// <summary>
/// Immutable chunks and a newest-first skew forest. Append copies at most 32 element slots and
/// creates at most one tree/link, including branches from any historical snapshot.
/// </summary>
internal sealed class AppendSequence<T> : IReadOnlyList<T>, IList<T>
{
    private const int ChunkSize = 32;
    private readonly T[] tail;
    private readonly Link? forest;
    private readonly AppendWork? work;

    internal AppendSequence(AppendWork? work = null) : this([], null, 0, work) { }

    private AppendSequence(T[] tail, Link? forest, int count, AppendWork? work)
    {
        this.tail = tail;
        this.forest = forest;
        this.work = work;
        Count = count;
    }

    public int Count { get; }

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count) { throw new ArgumentOutOfRangeException(nameof(index)); }
            int completedCount = Count - tail.Length;
            if (index >= completedCount) { return tail[index - completedCount]; }
            int chunk = completedCount / ChunkSize - 1 - index / ChunkSize;
            Link link = forest!;
            while (chunk >= link.Tree.Weight) { chunk -= link.Tree.Weight; link = link.Next!; }
            Tree tree = link.Tree;
            while (chunk != 0)
            {
                chunk--;
                if (chunk < tree.Left!.Weight) { tree = tree.Left; }
                else { chunk -= tree.Left.Weight; tree = tree.Right!; }
            }
            return tree.Chunk[index % ChunkSize];
        }
    }

    bool ICollection<T>.IsReadOnly => true;
    T IList<T>.this[int index] { get => this[index]; set => throw new NotSupportedException(); }
    void ICollection<T>.Add(T item) => throw new NotSupportedException();
    void ICollection<T>.Clear() => throw new NotSupportedException();
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
    void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
    void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
    bool ICollection<T>.Contains(T item) => ((IList<T>)this).IndexOf(item) >= 0;
    int IList<T>.IndexOf(T item)
    {
        int index = 0;
        foreach (T value in this)
        {
            if (EqualityComparer<T>.Default.Equals(value, item)) { return index; }
            index++;
        }
        return -1;
    }
    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        if (arrayIndex > array.Length || array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("The destination cannot hold every entry.", nameof(array));
        }
        foreach (T value in this) { array[arrayIndex++] = value; }
    }

    internal AppendSequence<T> Append(T value)
    {
        int count = checked(Count + 1);
        Link? nextForest = forest;
        T[] nextTail;
        if (tail.Length == ChunkSize)
        {
            Tree tree = forest is not null && forest.Next is not null
                && forest.Tree.Weight == forest.Next.Tree.Weight
                ? new(tail, forest.Tree, forest.Next.Tree)
                : new(tail, null, null);
            nextForest = new(tree, tree.Left is null ? forest : forest!.Next!.Next);
            nextTail = [value];
            if (work is not null) { work.Trees++; work.Links++; work.ElementSlots++; }
        }
        else
        {
            nextTail = new T[tail.Length + 1];
            Array.Copy(tail, nextTail, tail.Length);
            nextTail[^1] = value;
            if (work is not null) { work.ElementSlots += nextTail.Length; }
        }
        return new(nextTail, nextForest, count, work);
    }

    public IEnumerator<T> GetEnumerator()
    {
        Stack<Tree> trees = new();
        for (Link? link = forest; link is not null; link = link.Next) { trees.Push(link.Tree); }
        while (trees.TryPop(out Tree? tree))
        {
            // A chunk is newer than its left subtree, which is newer than its right subtree.
            Stack<(Tree Tree, bool Emit)> pending = new();
            pending.Push((tree, false));
            while (pending.TryPop(out (Tree Tree, bool Emit) item))
            {
                if (item.Emit)
                {
                    foreach (T value in item.Tree.Chunk)
                    {
                        if (work is not null) { work.EmittedElements++; }
                        yield return value;
                    }
                    continue;
                }
                if (work is not null) { work.EnumeratedTrees++; }
                pending.Push((item.Tree, true));
                if (item.Tree.Left is not null)
                {
                    pending.Push((item.Tree.Left, false));
                    pending.Push((item.Tree.Right!, false));
                }
            }
        }
        foreach (T value in tail)
        {
            if (work is not null) { work.EmittedElements++; }
            yield return value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Link(Tree tree, Link? next)
    {
        internal Tree Tree { get; } = tree;
        internal Link? Next { get; } = next;
    }

    private sealed class Tree(T[] chunk, Tree? left, Tree? right)
    {
        internal T[] Chunk { get; } = chunk;
        internal Tree? Left { get; } = left;
        internal Tree? Right { get; } = right;
        internal int Weight { get; } = checked(1 + (left?.Weight ?? 0) + (right?.Weight ?? 0));
    }
}
