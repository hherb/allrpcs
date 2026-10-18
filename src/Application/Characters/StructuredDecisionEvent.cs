namespace Allrpcs.Application.Characters;

/// <summary>Provider-neutral semantic events for one bounded structured decision.</summary>
public enum StructuredDecisionEvent
{
    /// <summary>The provider accepted a new response.</summary>
    Started,
    /// <summary>The response entered generation.</summary>
    InProgress,
    /// <summary>Non-output metadata was observed.</summary>
    Metadata,
    /// <summary>A bounded output-text fragment was observed.</summary>
    TextDelta,
    /// <summary>The authoritative final decision document was received.</summary>
    TextDone,
    /// <summary>The provider completed the response successfully.</summary>
    Completed,
    /// <summary>The response failed or contained an unsupported event.</summary>
    Failed,
}
