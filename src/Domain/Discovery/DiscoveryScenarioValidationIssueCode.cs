namespace Allrpcs.Domain.Discovery;

/// <summary>Identifies one closed kind of authored discovery-scenario structural defect.</summary>
public enum DiscoveryScenarioValidationIssueCode
{
    /// <summary>The perception snapshot belongs to another observer.</summary>
    ObserverPerceptionMismatch,

    /// <summary>A known world object has no initial perception entry.</summary>
    MissingPerception,

    /// <summary>An initial perception entry references an unknown world object.</summary>
    UnknownPerceptionTarget,

    /// <summary>Evidence references an unknown world object.</summary>
    UnknownEvidenceTarget,

    /// <summary>A capability requirement references an unknown target.</summary>
    UnknownCapabilityTarget,

    /// <summary>A required capability is unavailable in the scenario.</summary>
    UnavailableCapability,

    /// <summary>A tool requirement references an unknown target.</summary>
    UnknownToolTarget,

    /// <summary>A tool requirement references an unknown tool object.</summary>
    UnknownToolObject,

    /// <summary>A required tool is absent from the scenario inventory.</summary>
    UnavailableTool,

    /// <summary>An inventory reference is not a known world object.</summary>
    UnknownInventoryObject,

    /// <summary>A passive-noticing candidate belongs to another observer.</summary>
    PassiveObserverMismatch,

    /// <summary>A passive-noticing candidate references an unknown target.</summary>
    UnknownPassiveTarget,

    /// <summary>An observation-reinterpretation candidate belongs to another observer.</summary>
    ReinterpretationObserverMismatch,

    /// <summary>An observation-reinterpretation candidate references an unknown target.</summary>
    UnknownReinterpretationTarget,

    /// <summary>A reinterpretation candidate's trigger knowledge is unavailable.</summary>
    UnavailableTriggerKnowledge,

    /// <summary>Non-critical evidence cannot reach its required visibility.</summary>
    UnreachableEvidence,

    /// <summary>Critical evidence cannot reach its required visibility.</summary>
    UnreachableCriticalEvidence,
}
