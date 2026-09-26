using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Discovery;

/// <summary>Retains the one-shot discovery state for the exact layered inscription.</summary>
public sealed class OpeningRegionLayeredInscriptionDiscoveryState
{
    internal OpeningRegionLayeredInscriptionDiscoveryState(
        OpeningRegionLayeredInscriptionBinding inscriptionBinding,
        CharacterPerceptionSnapshot perception,
        ObservationReinterpretationCandidateSnapshot candidates,
        OpeningRegionLayeredInscriptionResolution? resolution)
    {
        InscriptionBinding = inscriptionBinding;
        Perception = perception;
        Candidates = candidates;
        Resolution = resolution;
    }

    /// <summary>Gets the exact shrine inscription represented by this state.</summary>
    public OpeningRegionLayeredInscriptionBinding InscriptionBinding { get; }

    /// <summary>Gets the current immutable Seeker perception.</summary>
    public CharacterPerceptionSnapshot Perception { get; }

    /// <summary>Gets the current immutable reinterpretation candidates.</summary>
    public ObservationReinterpretationCandidateSnapshot Candidates { get; }

    /// <summary>Gets the exact applied resolution, or null before discovery.</summary>
    public OpeningRegionLayeredInscriptionResolution? Resolution { get; }
}
