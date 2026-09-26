namespace Allrpcs.Application.Map;

/// <summary>Projects one safe map presentation into stable plain text.</summary>
public static class SeekerMapPresentationTextProjection
{
    /// <summary>Gets the fixed player-facing title.</summary>
    public const string Title = "Opening Region Map";

    /// <summary>Gets the neutral text shown when the Seeker knows no map locations.</summary>
    public const string EmptyContent = "No known locations.";

    /// <summary>Returns stable text containing only known location status, label, and note text.</summary>
    public static SeekerMapPresentationText Project(SeekerMapPresentationSnapshot presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        string content = presentation.Locations.Count == 0
            ? EmptyContent
            : string.Join("\n\n", presentation.Locations.Select(Render));
        return new SeekerMapPresentationText(Title, content);
    }

    private static string Render(SeekerMapPresentationEntry entry)
    {
        string status = entry.IsVerified ? "Verified" : "Reported";
        string location = $"{status}: {entry.Location.DisplayLabel}";
        return entry.NoteText is null
            ? location
            : $"{location}\nNote: {entry.NoteText}";
    }
}
