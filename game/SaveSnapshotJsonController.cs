using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes in-memory snapshot capture and restoration with the JSON format boundary.</summary>
public partial class SaveSnapshotJsonController : Node
{
    private SaveSnapshotCaptureController captureController = null!;
    private SaveSnapshotRestorationController restorationController = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        captureController = GetNode<SaveSnapshotCaptureController>(
            "../SaveSnapshotCaptureController");
        restorationController = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
        firstPatientContextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
    }

    /// <summary>Captures canonical JSON only when the complete live state is initialized.</summary>
    public string? CaptureSerialized()
    {
        SaveSnapshot? snapshot = captureController.Capture();
        return snapshot is null
            ? null
            : SaveSnapshotJsonSerializer.Serialize(snapshot);
    }

    /// <summary>
    /// Restores one strict, supported JSON document after complete scenario preflight.
    /// </summary>
    public bool TryRestoreSerialized(string serialized)
    {
        ArgumentNullException.ThrowIfNull(serialized);

        SaveSnapshot snapshot;
        try
        {
            snapshot = SaveSnapshotJsonDeserializer.DeserializeCurrent(
                serialized,
                InitialFirstPatientState());
        }
        catch (ArgumentException)
        {
            return false;
        }

        return restorationController.TryRestore(snapshot);
    }

    private FirstPatientSaveState InitialFirstPatientState() => new(
        firstPatientContextSource.Context.Scenario.DiseaseTruth,
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));
}
