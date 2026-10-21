using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors the temporary ordered greybox world-event definitions.</summary>
public partial class TemporaryWorldEventSource : Node
{
    private const string GreyboxReadyEventIdText = "greybox-ready";
    private const string HerbPatchIdentifiedEventIdText = "herb-patch-identified";
    private const string HerbPatchExaminedEventIdText = "healer-examined-herb-patch";
    private const string HerbPatchCollectedEventIdText = "healer-collected-herb-patch";
    private const string FirstPatientRemedyAdministeredEventIdText =
        "healer-administered-first-patient-remedy";
    private const string FirstPatientRemedySecondDoseEventIdText =
        "healer-administered-first-patient-second-dose";

    /// <summary>Gets the exact event selected for ordinary scene startup.</summary>
    public WorldEvent InitialWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for identifying the herb patch.</summary>
    public WorldEvent HerbPatchIdentifiedWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for the healer examining the herb patch.</summary>
    public WorldEvent HerbPatchExaminedWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for the healer collecting the herb patch.</summary>
    public WorldEvent HerbPatchCollectedWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for administering the first-patient remedy.</summary>
    public WorldEvent FirstPatientRemedyAdministeredWorldEvent { get; private set; } = null!;

    /// <summary>Gets the event authored for a second dose, after a first that changed nothing.</summary>
    public WorldEvent FirstPatientRemedySecondDoseWorldEvent { get; private set; } = null!;

    /// <summary>
    /// Gets the authored doses in order. shortcut: two doses; Phase 4 item 5 (stakes and ending)
    /// decides what follows a second wrong dose.
    /// </summary>
    public IReadOnlyList<WorldEvent> FirstPatientRemedyDoseWorldEvents =>
        [FirstPatientRemedyAdministeredWorldEvent, FirstPatientRemedySecondDoseWorldEvent];

    /// <summary>Gets the retained temporary authoritative world-event snapshot.</summary>
    public WorldEventSnapshot WorldEvents { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        InitialWorldEvent = new WorldEvent(
            new WorldEventId(GreyboxReadyEventIdText),
            WorldEventTimeEffect.NoAdvance);
        HerbPatchIdentifiedWorldEvent = new WorldEvent(
            new WorldEventId(HerbPatchIdentifiedEventIdText),
            WorldEventTimeEffect.Advance);
        HerbPatchExaminedWorldEvent = new WorldEvent(
            new WorldEventId(HerbPatchExaminedEventIdText),
            WorldEventTimeEffect.Advance);
        HerbPatchCollectedWorldEvent = new WorldEvent(
            new WorldEventId(HerbPatchCollectedEventIdText),
            WorldEventTimeEffect.Advance);
        FirstPatientRemedyAdministeredWorldEvent = new WorldEvent(
            new WorldEventId(FirstPatientRemedyAdministeredEventIdText),
            WorldEventTimeEffect.Advance);
        FirstPatientRemedySecondDoseWorldEvent = new WorldEvent(
            new WorldEventId(FirstPatientRemedySecondDoseEventIdText),
            WorldEventTimeEffect.Advance);
        WorldEvents = new(
        [
            InitialWorldEvent,
            HerbPatchIdentifiedWorldEvent,
            HerbPatchExaminedWorldEvent,
            HerbPatchCollectedWorldEvent,
            FirstPatientRemedyAdministeredWorldEvent,
            FirstPatientRemedySecondDoseWorldEvent,
        ]);
    }
}
