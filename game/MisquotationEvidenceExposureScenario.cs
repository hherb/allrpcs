using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Builds isolated exact exposure and witness-view state for coordinator smoke checks.</summary>
internal sealed class MisquotationEvidenceExposureScenario
{
    private readonly CharacterId witnessCharacterId;

    /// <summary>Creates initialized exact exposure and witness-view scene nodes.</summary>
    public MisquotationEvidenceExposureScenario(
        CharacterDialogueMisquotationEvidenceExposureSnapshot initial,
        CharacterId witnessCharacterId)
    {
        this.witnessCharacterId = witnessCharacterId;
        Container = new Node();
        Exposure = new CharacterDialogueMisquotationEvidenceExposureController
        {
            Name = "CharacterDialogueMisquotationEvidenceExposureController",
        };
        View = new CharacterDialogueMisquotationEvidenceViewController
        {
            Name = "CharacterDialogueMisquotationEvidenceViewController",
        };
        Task = new CharacterDialogueMisquotationEvidenceTaskController
        {
            Name = "CharacterDialogueMisquotationEvidenceTaskController",
        };
        Coordinator = new CharacterDialogueMisquotationEvidenceExposureCoordinator();
        Container.AddChild(Exposure);
        Container.AddChild(View);
        Container.AddChild(Task);
        Container.AddChild(Coordinator);
        if (!Exposure.TryInitialize(initial)
            || !View.TryInitialize(initial, witnessCharacterId))
        {
            throw new InvalidOperationException("Smoke exposure and view initialization failed.");
        }
    }

    /// <summary>Gets the isolated parent containing all scenario nodes.</summary>
    public Node Container { get; }

    /// <summary>Gets the scene-owned exposure state.</summary>
    public CharacterDialogueMisquotationEvidenceExposureController Exposure { get; }

    /// <summary>Gets the scene-owned witness view.</summary>
    public CharacterDialogueMisquotationEvidenceViewController View { get; }

    /// <summary>Gets the scene-owned decision task.</summary>
    public CharacterDialogueMisquotationEvidenceTaskController Task { get; }

    /// <summary>Gets the scene-owned exposure coordinator.</summary>
    public CharacterDialogueMisquotationEvidenceExposureCoordinator Coordinator { get; }

    /// <summary>Starts one exact source-owned consideration through the scenario task.</summary>
    public bool Start(
        TemporaryCharacterDialogueMisquotationEvidenceSource source,
        ICharacterDialogueMisquotationEvidenceDecisionModel model)
    {
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            source.Actor,
            [source.Candidate]);
        return Task.TryStart(model, request);
    }

    /// <summary>Returns whether exposure retains the supplied exact entries.</summary>
    public bool HasExposure(CharacterDialogueMisquotationEvidence[] entries) =>
        Exposure.Exposure is { } current
        && current.Entries.SequenceEqual(entries, ReferenceEqualityComparer.Instance);

    /// <summary>Returns whether the view retains the exact exposure and supplied entries.</summary>
    public bool HasView(
        CharacterDialogueMisquotationEvidenceExposureSnapshot exposure,
        CharacterDialogueMisquotationEvidence[] entries) =>
        ReferenceEquals(View.SourceExposure, exposure)
        && View.View is { } current
        && ReferenceEquals(current.WitnessCharacterId, witnessCharacterId)
        && current.Entries.SequenceEqual(entries, ReferenceEqualityComparer.Instance);
}
