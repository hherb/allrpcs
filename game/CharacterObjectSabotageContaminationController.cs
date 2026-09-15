using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns exact scene contamination and authoritative sabotage-evidence snapshots.</summary>
public partial class CharacterObjectSabotageContaminationController : Node
{
    private CharacterObjectSabotageContaminationConsiderationRequest? lastResolvedRequest;

    /// <summary>Gets the initialized exact active contamination snapshot, if any.</summary>
    public WorldObjectContaminationSnapshot? Contamination { get; private set; }

    /// <summary>Gets the initialized exact authoritative evidence snapshot, if any.</summary>
    public DiscoveryEvidenceSnapshot? Evidence { get; private set; }

    /// <summary>Atomically initializes exact contamination and evidence state once.</summary>
    public bool TryInitialize(
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence)
    {
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidence);

        if (Contamination is not null || Evidence is not null)
        {
            return false;
        }

        Contamination = contamination;
        Evidence = evidence;
        return true;
    }

    /// <summary>Applies one eligible exact contamination only from expected current scene state.</summary>
    public bool TryApply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot expectedContamination,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(expectedContamination);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Contamination, expectedContamination)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageContaminationResult result;
        try
        {
            result = CharacterObjectSabotageContaminationTransition.Apply(
                character,
                physicalAccess,
                worldObjects,
                expectedContamination,
                expectedEvidence,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Contamination = result.Contamination;
        Evidence = result.EvidenceResult.Evidence;
        return true;
    }

    /// <summary>Resolves one exact accepted consideration result from current scene state.</summary>
    public bool TryResolveConsideration(
        CharacterObjectSabotageContaminationConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterObjectSabotageContaminationConsiderationRequest request = considerationResult.Request;
        if (ReferenceEquals(lastResolvedRequest, request)
            || !ReferenceEquals(Contamination, request.Contamination)
            || !ReferenceEquals(Evidence, request.Evidence))
        {
            return false;
        }

        CharacterObjectSabotageContaminationResult? result =
            CharacterObjectSabotageContaminationConsiderationTransition.Apply(considerationResult);
        if (result is not null)
        {
            Contamination = result.Contamination;
            Evidence = result.EvidenceResult.Evidence;
        }

        lastResolvedRequest = request;
        return true;
    }

    /// <summary>Recovers one exact contamination only from expected current scene state.</summary>
    public bool TryRecover(
        WorldObjectContaminationSnapshot expectedContamination,
        DiscoveryEvidenceSnapshot expectedEvidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(expectedContamination);
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!ReferenceEquals(Contamination, expectedContamination)
            || !ReferenceEquals(Evidence, expectedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageContaminationRecoveryResult result;
        try
        {
            result = CharacterObjectSabotageContaminationRecoveryTransition.Recover(
                expectedContamination,
                expectedEvidence,
                evidenceView,
                candidate);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Contamination = result.Contamination;
        return true;
    }
}
