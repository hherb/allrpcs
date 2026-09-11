using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;

namespace Allrpcs.Application.Diseases;

/// <summary>Projects hidden disease stages into bounded qualitative condition changes.</summary>
public static class DiseaseConditionProjection
{
    /// <summary>Returns an observable qualitative condition, or null for unobservable exposure.</summary>
    public static CharacterCondition? Project(
        CharacterDiseaseState infection,
        DiseaseTruthSnapshot diseaseTruth)
    {
        ArgumentNullException.ThrowIfNull(infection);
        ArgumentNullException.ThrowIfNull(diseaseTruth);

        if (!diseaseTruth.Infections.Any(candidate => ReferenceEquals(candidate, infection)))
        {
            throw new ArgumentException(
                "Condition projection requires an exact authoritative infection.",
                nameof(infection));
        }

        if (ReferenceEquals(infection.Stage, DiseaseStage.Exposed))
        {
            return null;
        }

        if (ReferenceEquals(infection.Stage, DiseaseStage.EarlyIllness))
        {
            return new CharacterCondition("appears-unwell", "Appears unwell.");
        }

        if (ReferenceEquals(infection.Stage, DiseaseStage.Drowsing))
        {
            return new CharacterCondition("appears-drowsy", "Appears increasingly drowsy.");
        }

        if (ReferenceEquals(infection.Stage, DiseaseStage.DeepSleep))
        {
            return new CharacterCondition(
                "deeply-asleep",
                "Is deeply asleep and cannot be roused.");
        }

        if (ReferenceEquals(infection.Stage, DiseaseStage.Wasting))
        {
            return new CharacterCondition("severely-weakened", "Appears severely weakened.");
        }

        if (ReferenceEquals(infection.Stage, DiseaseStage.FinalDecline))
        {
            return new CharacterCondition("near-death", "Appears close to death.");
        }

        throw new InvalidOperationException("Unknown disease stages cannot be projected.");
    }
}
