using Allrpcs.Application.Map;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one explicit safe projection of the scene's exact current map state.</summary>
public partial class SeekerMapPresentationController : Node
{
    private SeekerMapController state = null!;
    private SeekerMapSnapshot? projectedMap;
    private SeekerMapNoteSnapshot? projectedNotes;

    /// <summary>Gets the exact safe map presentation, or null before explicit projection.</summary>
    public SeekerMapPresentationSnapshot? Presentation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        state = GetNode<SeekerMapController>("../SeekerMapController");
    }

    /// <summary>Projects exact current map and note state once without rendering UI.</summary>
    public bool TryProject(
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(expectedNotes);

        if (Presentation is not null
            || !ReferenceEquals(state.Map, expectedMap)
            || !ReferenceEquals(state.Notes, expectedNotes))
        {
            return false;
        }

        SeekerMapPresentationSnapshot result;
        try
        {
            result = SeekerMapPresentationProjection.Project(expectedMap, expectedNotes);
        }
        catch (ArgumentException)
        {
            return false;
        }

        projectedMap = expectedMap;
        projectedNotes = expectedNotes;
        Presentation = result;
        return true;
    }

    /// <summary>Replaces one exact current presentation only after its source state changes.</summary>
    public bool TryRefresh(
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes,
        SeekerMapPresentationSnapshot expectedPresentation)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(expectedNotes);
        ArgumentNullException.ThrowIfNull(expectedPresentation);

        if (!ReferenceEquals(Presentation, expectedPresentation)
            || !ReferenceEquals(state.Map, expectedMap)
            || !ReferenceEquals(state.Notes, expectedNotes)
            || ReferenceEquals(projectedMap, expectedMap)
                && ReferenceEquals(projectedNotes, expectedNotes))
        {
            return false;
        }

        SeekerMapPresentationSnapshot replacement;
        try
        {
            replacement = SeekerMapPresentationProjection.Project(expectedMap, expectedNotes);
        }
        catch (ArgumentException)
        {
            return false;
        }

        projectedMap = expectedMap;
        projectedNotes = expectedNotes;
        Presentation = replacement;
        return true;
    }
}
