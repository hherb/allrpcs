using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Diseases;

namespace Allrpcs.Application.Diseases;

/// <summary>Selects an effective controller without replacing the authored configuration.</summary>
public static class EffectiveCharacterControllerSelector
{
    /// <summary>Returns dormant for deep disease, or the exact configured assignment otherwise.</summary>
    public static CharacterControllerAssignment Select(
        CharacterControllerAssignment configuredAssignment,
        DiseaseTruthSnapshot diseaseTruth)
    {
        ArgumentNullException.ThrowIfNull(configuredAssignment);
        ArgumentNullException.ThrowIfNull(diseaseTruth);

        bool requiresDormantController = diseaseTruth.Infections.Any(infection =>
            infection.CharacterId == configuredAssignment.CharacterId
            && IsDeepStage(infection.Stage));
        if (!requiresDormantController
            || ReferenceEquals(configuredAssignment.ControllerType, ControllerType.Dormant))
        {
            return configuredAssignment;
        }

        return new CharacterControllerAssignment(
            configuredAssignment.CharacterId,
            ControllerType.Dormant);
    }

    private static bool IsDeepStage(DiseaseStage stage) =>
        ReferenceEquals(stage, DiseaseStage.DeepSleep)
        || ReferenceEquals(stage, DiseaseStage.Wasting)
        || ReferenceEquals(stage, DiseaseStage.FinalDecline);
}
