using Allrpcs.Application.Map;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns explicit bounded note editing for only the map's safely presented locations.</summary>
public partial class SeekerMapNoteEditingController : Node
{
    private const string EmptyLocationsText = "No known locations";

    private OptionButton locations = null!;
    private LineEdit note = null!;
    private Button remove = null!;
    private Button save = null!;
    private SeekerMapPanel panel = null!;
    private SeekerMapPresentationController presentations = null!;
    private SeekerMapController state = null!;
    private SeekerMapSnapshot? presentedMap;
    private SeekerMapNoteSnapshot? presentedNotes;
    private SeekerMapPresentationSnapshot? presented;

    /// <inheritdoc />
    public override void _Ready()
    {
        panel = GetNode<SeekerMapPanel>("../SeekerMapPanel");
        locations = panel.GetNode<OptionButton>("Panel/Locations");
        note = panel.GetNode<LineEdit>("Panel/Note");
        save = panel.GetNode<Button>("Panel/SaveNote");
        remove = panel.GetNode<Button>("Panel/RemoveNote");
        presentations = GetNode<SeekerMapPresentationController>(
            "../SeekerMapPresentationController");
        state = GetNode<SeekerMapController>("../SeekerMapController");
        locations.ItemSelected += OnLocationSelected;
        save.Pressed += OnSavePressed;
        remove.Pressed += OnRemovePressed;
        ShowEmptyLocations();
    }

    /// <summary>Displays only exact current safe presentation entries as editable note targets.</summary>
    public bool TryPresent(
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes,
        SeekerMapPresentationSnapshot expectedPresentation)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(expectedNotes);
        ArgumentNullException.ThrowIfNull(expectedPresentation);

        if (!ReferenceEquals(state.Map, expectedMap)
            || !ReferenceEquals(state.Notes, expectedNotes)
            || !presentations.HasExactSources(
                expectedMap,
                expectedNotes,
                expectedPresentation))
        {
            return false;
        }

        MapLocationDefinition? selectedLocation = SelectedLocation();
        presentedMap = expectedMap;
        presentedNotes = expectedNotes;
        presented = expectedPresentation;
        ApplyPresentation(selectedLocation);
        return true;
    }

    private void OnLocationSelected(long index)
    {
        if (presented is null || index < 0 || index >= presented.Locations.Count)
        {
            note.Text = string.Empty;
            save.Disabled = true;
            remove.Disabled = true;
            return;
        }

        LoadEntry(presented.Locations[(int)index]);
    }

    private void OnSavePressed()
    {
        if (!TryGetCurrent(out SeekerMapSnapshot map, out SeekerMapNoteSnapshot notes)
            || SelectedEntry() is not { } entry
            || !state.TrySetNote(map, notes, entry.Location, note.Text))
        {
            return;
        }

        RefreshAfterMutation(entry.Location);
    }

    private void OnRemovePressed()
    {
        if (!TryGetCurrent(out SeekerMapSnapshot map, out SeekerMapNoteSnapshot notes)
            || SelectedEntry() is not { NoteText: not null } entry
            || !state.TryRemoveNote(map, notes, entry.Location))
        {
            return;
        }

        RefreshAfterMutation(entry.Location);
    }

    private bool TryGetCurrent(
        out SeekerMapSnapshot map,
        out SeekerMapNoteSnapshot notes)
    {
        map = presentedMap!;
        notes = presentedNotes!;
        return panel.Visible
            && presented is not null
            && map is not null
            && notes is not null
            && ReferenceEquals(state.Map, map)
            && ReferenceEquals(state.Notes, notes)
            && ReferenceEquals(presentations.Presentation, presented)
            && ReferenceEquals(panel.Presentation, presented);
    }

    private void RefreshAfterMutation(MapLocationDefinition selectedLocation)
    {
        SeekerMapSnapshot map = state.Map!;
        SeekerMapNoteSnapshot notes = state.Notes!;
        SeekerMapPresentationSnapshot previous = presented!;
        if (!presentations.TryRefresh(map, notes, previous)
            || presentations.Presentation is not { } replacement
            || !panel.TryPresent(replacement))
        {
            throw new InvalidOperationException(
                "An accepted exact map-note mutation could not refresh its safe presentation.");
        }

        presentedMap = map;
        presentedNotes = notes;
        presented = replacement;
        ApplyPresentation(selectedLocation);
    }

    private void ApplyPresentation(MapLocationDefinition? preferredLocation)
    {
        locations.Clear();
        if (presented is null || presented.Locations.Count == 0)
        {
            ShowEmptyLocations();
            return;
        }

        int selectedIndex = 0;
        for (int index = 0; index < presented.Locations.Count; index++)
        {
            SeekerMapPresentationEntry entry = presented.Locations[index];
            locations.AddItem(entry.Location.DisplayLabel);
            if (ReferenceEquals(entry.Location, preferredLocation))
            {
                selectedIndex = index;
            }
        }

        locations.Select(selectedIndex);
        LoadEntry(presented.Locations[selectedIndex]);
    }

    private void ShowEmptyLocations()
    {
        locations.Clear();
        locations.AddItem(EmptyLocationsText);
        locations.SetItemDisabled(0, true);
        locations.Select(0);
        note.Text = string.Empty;
        save.Disabled = true;
        remove.Disabled = true;
    }

    private void LoadEntry(SeekerMapPresentationEntry entry)
    {
        note.Text = entry.NoteText ?? string.Empty;
        save.Disabled = false;
        remove.Disabled = entry.NoteText is null;
    }

    private SeekerMapPresentationEntry? SelectedEntry()
    {
        int selectedIndex = locations.Selected;
        return presented is not null
            && selectedIndex >= 0
            && selectedIndex < presented.Locations.Count
                ? presented.Locations[selectedIndex]
                : null;
    }

    private MapLocationDefinition? SelectedLocation() => SelectedEntry()?.Location;
}
