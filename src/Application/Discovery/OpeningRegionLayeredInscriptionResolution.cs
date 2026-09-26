using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Discovery;

/// <summary>Retains the exact provenance and pure result of one shrine reinterpretation.</summary>
public sealed class OpeningRegionLayeredInscriptionResolution
{
    internal OpeningRegionLayeredInscriptionResolution(
        OpeningRegionLayeredInscriptionCandidateBinding candidateBinding,
        Character seeker,
        ObservationReinterpretationCandidateResolutionResult result)
    {
        CandidateBinding = candidateBinding;
        Seeker = seeker;
        Result = result;
    }

    /// <summary>Gets the exact accessible candidate that was resolved.</summary>
    public OpeningRegionLayeredInscriptionCandidateBinding CandidateBinding { get; }

    /// <summary>Gets the exact authoritative Seeker used for resolution.</summary>
    public Character Seeker { get; }

    /// <summary>Gets the exact pure perception-and-candidate result.</summary>
    public ObservationReinterpretationCandidateResolutionResult Result { get; }
}
