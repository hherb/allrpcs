using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Discovery;

/// <summary>Resolves one exactly bound shrine inscription through existing pure discovery rules.</summary>
public static class OpeningRegionLayeredInscriptionResolutionTransition
{
    /// <summary>Returns an identified perception and consumed candidate with exact provenance.</summary>
    public static OpeningRegionLayeredInscriptionResolution Resolve(
        OpeningRegionLayeredInscriptionCandidateBinding candidateBinding,
        Character seeker)
    {
        ArgumentNullException.ThrowIfNull(candidateBinding);
        ArgumentNullException.ThrowIfNull(seeker);

        DiscoveryScenario scenario = candidateBinding.Access.InscriptionBinding.DiscoveryScenario;
        if (!ReferenceEquals(seeker.Identity.Id, candidateBinding.Access.SeekerCharacterId)
            || !ReferenceEquals(seeker.Knowledge, scenario.AvailableKnowledge)
            || !ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidateBinding.Candidate,
                seeker,
                scenario.InitialPerception,
                scenario.ReinterpretationCandidates))
        {
            throw new InvalidOperationException(
                "Only the exact knowledgeable Seeker may resolve the bound shrine candidate.");
        }

        ObservationReinterpretationCandidateResolutionResult result =
            ObservationReinterpretationCandidateResolutionTransition.Resolve(
                candidateBinding.Candidate,
                seeker,
                scenario.InitialPerception,
                scenario.ReinterpretationCandidates);
        return new OpeningRegionLayeredInscriptionResolution(
            candidateBinding,
            seeker,
            result);
    }
}
