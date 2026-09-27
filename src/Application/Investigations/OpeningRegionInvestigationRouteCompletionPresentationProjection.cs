namespace Allrpcs.Application.Investigations;

/// <summary>Projects exact canonical route completions into closed authored safe copy.</summary>
public static class OpeningRegionInvestigationRouteCompletionPresentationProjection
{
    /// <summary>Gets the shared fixed completion heading.</summary>
    public const string Title = "Investigation route complete";

    /// <summary>Projects only fixed safe copy selected by the exact completion route key.</summary>
    public static OpeningRegionInvestigationRouteCompletionPresentationSnapshot Project(
        OpeningRegionInvestigationRouteCompletion completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        string content = completion.Route.Key switch
        {
            "stream-laundry" => "Stream and abandoned laundry route complete.",
            "shrine" => "Old shrine route complete.",
            "road-wagon" => "Southern road and abandoned wagon route complete.",
            _ => throw new InvalidOperationException(
                "Completion presentation requires one closed authored route key."),
        };
        return new(Title, content);
    }
}
