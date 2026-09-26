using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Discovery;

/// <summary>Creates presentation authority only from exact applied inscription discovery.</summary>
public static class OpeningRegionLayeredInscriptionPresentationTransition
{
    /// <summary>Returns a presentation token for the exact identified and consumed result.</summary>
    public static OpeningRegionLayeredInscriptionPresentation Create(
        OpeningRegionLayeredInscriptionDiscoveryState discoveryState)
    {
        ArgumentNullException.ThrowIfNull(discoveryState);

        OpeningRegionLayeredInscriptionResolution? resolution = discoveryState.Resolution;
        WorldObjectVisibility? visibility = resolution is null
            ? null
            : CharacterPerceptionLookup.FindByObject(
                resolution.CandidateBinding.Candidate.TargetObjectId,
                discoveryState.Perception);
        if (resolution is null
            || !ReferenceEquals(discoveryState.Perception, resolution.Result.Perception)
            || !ReferenceEquals(discoveryState.Candidates, resolution.Result.Candidates)
            || !ReferenceEquals(visibility?.VisibilityState, VisibilityState.Identified)
            || ObservationReinterpretationCandidateLookup.Find(
                resolution.CandidateBinding.Candidate,
                discoveryState.Candidates) is not null)
        {
            throw new InvalidOperationException(
                "Only the exact applied identified inscription result may be presented.");
        }

        return new OpeningRegionLayeredInscriptionPresentation(discoveryState, resolution);
    }
}
