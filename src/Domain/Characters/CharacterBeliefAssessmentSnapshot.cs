using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains immutable ordered belief assessments for one character.</summary>
public sealed class CharacterBeliefAssessmentSnapshot
{
    /// <summary>Creates a per-character assessment snapshot in stable supplied order.</summary>
    public CharacterBeliefAssessmentSnapshot(
        CharacterId characterId,
        IEnumerable<CharacterBeliefAssessment> assessments)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(assessments);

        List<CharacterBeliefAssessment> retainedAssessments = new();
        HashSet<EpistemicMemoryId> assessedMemoryIds = new();
        foreach (CharacterBeliefAssessment assessment in assessments)
        {
            if (assessment is null)
            {
                throw new ArgumentException(
                    "Belief-assessment snapshots cannot contain null values.",
                    nameof(assessments));
            }

            if (assessment.Belief.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every assessed belief must belong to the snapshot character.",
                    nameof(assessments));
            }

            if (!assessedMemoryIds.Add(assessment.Belief.MemoryId))
            {
                throw new ArgumentException(
                    "A belief can have only one assessment in a character snapshot.",
                    nameof(assessments));
            }

            retainedAssessments.Add(assessment);
            if (retainedAssessments.Count > CharacterEpistemicMemorySnapshot.MaximumMemoryCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(assessments),
                    retainedAssessments.Count,
                    "A belief-assessment snapshot cannot exceed the epistemic-memory bound.");
            }
        }

        CharacterId = characterId;
        Assessments = retainedAssessments.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact assessment instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterBeliefAssessment> Assessments { get; }
}
