using Allrpcs.Domain.Persistence;

namespace Allrpcs.Game;

/// <summary>Names an owner and prepares its replacement without mutating installed state.</summary>
internal interface IRestoreParticipant
{
    string Owner { get; }
    PreparedRestore Prepare(SaveSnapshot snapshot);
}

/// <summary>Contains assignment-only installation and optional post-commit presentation.</summary>
internal sealed record PreparedRestore(Action Commit, Action? Publish = null);

/// <summary>Adapts an existing scene owner without adding persistence to its public API.</summary>
internal sealed record RestoreParticipant(
    string Owner,
    Func<SaveSnapshot, PreparedRestore> Preparation) : IRestoreParticipant
{
    public PreparedRestore Prepare(SaveSnapshot snapshot) => Preparation(snapshot);
}
