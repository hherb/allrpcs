using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Discovery;

/// <summary>Initializes and advances the exact layered-inscription discovery state once.</summary>
public static class OpeningRegionLayeredInscriptionDiscoveryStateTransition
{
    /// <summary>Creates undiscovered state from the binding's exact authored scenario.</summary>
    public static OpeningRegionLayeredInscriptionDiscoveryState Initialize(
        OpeningRegionLayeredInscriptionBinding inscriptionBinding)
    {
        ArgumentNullException.ThrowIfNull(inscriptionBinding);

        DiscoveryScenario scenario = inscriptionBinding.DiscoveryScenario;
        return new OpeningRegionLayeredInscriptionDiscoveryState(
            inscriptionBinding,
            scenario.InitialPerception,
            scenario.ReinterpretationCandidates,
            resolution: null);
    }

    /// <summary>Applies the exact first resolution without mutating the supplied state.</summary>
    public static OpeningRegionLayeredInscriptionDiscoveryState Apply(
        OpeningRegionLayeredInscriptionDiscoveryState state,
        OpeningRegionLayeredInscriptionResolution resolution)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resolution);

        DiscoveryScenario scenario = state.InscriptionBinding.DiscoveryScenario;
        if (state.Resolution is not null
            || !ReferenceEquals(state.Perception, scenario.InitialPerception)
            || !ReferenceEquals(state.Candidates, scenario.ReinterpretationCandidates)
            || !ReferenceEquals(
                resolution.CandidateBinding.Access.InscriptionBinding,
                state.InscriptionBinding))
        {
            throw new InvalidOperationException(
                "Only the exact first shrine resolution may advance undiscovered scene state.");
        }

        return new OpeningRegionLayeredInscriptionDiscoveryState(
            state.InscriptionBinding,
            resolution.Result.Perception,
            resolution.Result.Candidates,
            resolution);
    }
}
