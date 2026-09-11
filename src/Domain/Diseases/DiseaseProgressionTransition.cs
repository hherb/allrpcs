using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Advances one exact hidden infection by one canonical stage.</summary>
public static class DiseaseProgressionTransition
{
    /// <summary>Returns new disease truth after consuming one occurred meaningful-time event.</summary>
    public static DiseaseTruthSnapshot Advance(
        CharacterDiseaseState infection,
        DiseaseTruthSnapshot truth,
        WorldEvent worldEvent,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        if (!DiseaseProgressionPolicy.CanAdvance(
            infection,
            truth,
            worldEvent,
            worldEvents,
            eventLog,
            worldTime))
        {
            throw new InvalidOperationException("The supplied disease state cannot advance.");
        }

        CharacterDiseaseState advanced = new(
            infection.CharacterId,
            infection.Disease,
            NextStage(infection.Stage),
            worldEvent.EventId,
            infection.CareState);
        CharacterDiseaseState[] infections = truth.Infections
            .Select(candidate => ReferenceEquals(candidate, infection) ? advanced : candidate)
            .ToArray();

        return new DiseaseTruthSnapshot(truth.Definitions, infections);
    }

    private static DiseaseStage NextStage(DiseaseStage stage)
    {
        if (ReferenceEquals(stage, DiseaseStage.Exposed))
        {
            return DiseaseStage.EarlyIllness;
        }

        if (ReferenceEquals(stage, DiseaseStage.EarlyIllness))
        {
            return DiseaseStage.Drowsing;
        }

        if (ReferenceEquals(stage, DiseaseStage.Drowsing))
        {
            return DiseaseStage.DeepSleep;
        }

        if (ReferenceEquals(stage, DiseaseStage.DeepSleep))
        {
            return DiseaseStage.Wasting;
        }

        if (ReferenceEquals(stage, DiseaseStage.Wasting))
        {
            return DiseaseStage.FinalDecline;
        }

        throw new InvalidOperationException("The final disease stage cannot advance.");
    }
}
