namespace Allrpcs.Domain.Collections;

/// <summary>Optional per-history structural observations for internal tests; absent in normal snapshots.</summary>
internal sealed class AppendWork
{
    internal long ElementSlots { get; set; }
    internal long Trees { get; set; }
    internal long Links { get; set; }
    internal long IndexNodes { get; set; }
    internal long IndexVisits { get; set; }
    internal long KeySymbols { get; set; }
    internal long EnumeratedTrees { get; set; }
    internal long EmittedElements { get; set; }
}
