namespace Allrpcs.Application.Map;

/// <summary>Contains stable plain text projected only from one safe map presentation.</summary>
public sealed class SeekerMapPresentationText
{
    /// <summary>Creates one immutable plain-text map presentation.</summary>
    internal SeekerMapPresentationText(string title, string content)
    {
        Title = title;
        Content = content;
    }

    /// <summary>Gets the fixed player-facing map title.</summary>
    public string Title { get; }

    /// <summary>Gets the safe known-location presentation content.</summary>
    public string Content { get; }
}
