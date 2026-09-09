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

    /// <summary>Gets the exact event selected for ordinary scene startup.</summary>
    public WorldEvent InitialWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for identifying the herb patch.</summary>
    public WorldEvent HerbPatchIdentifiedWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for the healer examining the herb patch.</summary>
    public WorldEvent HerbPatchExaminedWorldEvent { get; private set; } = null!;

    /// <summary>Gets the exact event authored for the healer collecting the herb patch.</summary>
    public WorldEvent HerbPatchCollectedWorldEvent { get; private set; } = null!;

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
        WorldEvents = new(
        [
            InitialWorldEvent,
            HerbPatchIdentifiedWorldEvent,
            HerbPatchExaminedWorldEvent,
            HerbPatchCollectedWorldEvent,
        ]);
    }
}
