using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one explicitly initialized immutable provider-settings selection.</summary>
public partial class CharacterModelSettingsController : Node
{
    private CharacterModelSettingsStorageController storageController = null!;

    /// <summary>Gets the exact current settings snapshot, if initialized.</summary>
    public CharacterModelSettingsSelectionSnapshot? Snapshot { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        storageController = GetNode<CharacterModelSettingsStorageController>(
            "../CharacterModelSettingsStorageController");
        TryLoadCurrent();
    }

    /// <summary>Loads and retains one valid current persisted selection when uninitialized.</summary>
    public bool TryLoadCurrent()
    {
        if (Snapshot is not null)
        {
            return false;
        }

        CharacterModelSettingsDocumentLoadResult result = storageController.LoadCurrent();
        return result.Status == CharacterModelSettingsDocumentLoadStatus.Loaded
            && TryInitialize(result.Selection!);
    }

    /// <summary>Initializes this controller once from one exact existing snapshot.</summary>
    public bool TryInitialize(CharacterModelSettingsSelectionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (Snapshot is not null)
        {
            return false;
        }

        Snapshot = snapshot;
        return true;
    }

    /// <summary>Replaces or appends one exact canonical provider setting.</summary>
    public bool TryUpdate(CharacterModelSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        CharacterModelSettingsSelectionSnapshot? snapshot = Snapshot;
        if (snapshot is null)
        {
            return false;
        }

        try
        {
            Snapshot = CharacterModelSettingsUpdateTransition.Update(settings, snapshot);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Selects one exact configured canonical provider.</summary>
    public bool TrySelect(CharacterModelProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        CharacterModelSettingsSelectionSnapshot? snapshot = Snapshot;
        if (snapshot is null)
        {
            return false;
        }

        try
        {
            Snapshot = CharacterModelSettingsSelectionTransition.Select(provider, snapshot);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Removes one exact configured unselected canonical provider.</summary>
    public bool TryRemove(CharacterModelProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        CharacterModelSettingsSelectionSnapshot? snapshot = Snapshot;
        if (snapshot is null)
        {
            return false;
        }

        try
        {
            Snapshot = CharacterModelSettingsRemovalTransition.Remove(provider, snapshot);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
