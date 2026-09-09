using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one current immutable provider-neutral character-model stream prefix.</summary>
public partial class CharacterModelStreamController : Node
{
    /// <summary>Gets the exact initialized stream snapshot, if any.</summary>
    public CharacterModelStreamSnapshot? Snapshot { get; private set; }

    /// <summary>Initializes this controller once from one exact existing stream snapshot.</summary>
    public bool TryInitialize(CharacterModelStreamSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (Snapshot is not null)
        {
            return false;
        }

        Snapshot = snapshot;
        return true;
    }

    /// <summary>Appends one exact update when a stream has begun and remains non-terminal.</summary>
    public bool TryAppend(CharacterModelStreamUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        CharacterModelStreamSnapshot? snapshot = Snapshot;
        if (snapshot is null)
        {
            return false;
        }

        try
        {
            Snapshot = CharacterModelStreamAppendTransition.Append(update, snapshot);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
