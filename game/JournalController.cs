using Allrpcs.Application.Journal;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's exact complete visibility-safe journal snapshot.</summary>
public partial class JournalController : Node
{
    /// <summary>Gets the exact initialized journal, or null before explicit initialization.</summary>
    public JournalSnapshot? Journal { get; private set; }

    /// <summary>Initializes the scene journal once from one already-complete safe snapshot.</summary>
    public bool TryInitialize(JournalSnapshot journal)
    {
        ArgumentNullException.ThrowIfNull(journal);

        if (Journal is not null
            || journal.Testimony is null
            || journal.Hypotheses is null
            || journal.Experiments is null
            || journal.OpenQuestions is null
            || journal.PendingSections.Count != 0)
        {
            return false;
        }

        Journal = journal;
        return true;
    }

    /// <summary>Installs one preflighted complete safe journal during root restoration.</summary>
    internal void InstallRestored(JournalSnapshot journal)
    {
        ArgumentNullException.ThrowIfNull(journal);

        Journal = journal;
    }
}
