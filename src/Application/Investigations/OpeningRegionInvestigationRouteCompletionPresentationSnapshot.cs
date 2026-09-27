namespace Allrpcs.Application.Investigations;

/// <summary>Retains fixed safe copy for one completed opening-region investigation route.</summary>
public sealed class OpeningRegionInvestigationRouteCompletionPresentationSnapshot
{
    internal OpeningRegionInvestigationRouteCompletionPresentationSnapshot(
        string title,
        string content)
    {
        Title = title;
        Content = content;
    }

    /// <summary>Gets the fixed authored completion heading.</summary>
    public string Title { get; }

    /// <summary>Gets the fixed authored route-specific completion copy.</summary>
    public string Content { get; }
}
