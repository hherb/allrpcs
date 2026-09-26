using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Copies authored experiment text without exposing its authoritative event.</summary>
public sealed class JournalExperimentAuthoredCopy
{
    internal JournalExperimentAuthoredCopy(
        ExperimentId experimentId,
        string procedure,
        string observedOutcome)
    {
        ExperimentId = experimentId;
        Procedure = procedure;
        ObservedOutcome = observedOutcome;
    }

    /// <summary>Gets the exact canonical experiment identifier.</summary>
    public ExperimentId ExperimentId { get; }

    /// <summary>Gets the exact authored procedure text.</summary>
    public string Procedure { get; }

    /// <summary>Gets the exact authored observed-outcome text.</summary>
    public string ObservedOutcome { get; }
}
