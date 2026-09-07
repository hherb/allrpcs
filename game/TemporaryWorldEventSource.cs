using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one temporary snapshot containing the no-advance greybox-ready event.</summary>
public partial class TemporaryWorldEventSource : Node
{
    private const string GreyboxReadyEventIdText = "greybox-ready";

    /// <summary>Gets the retained temporary authoritative world-event snapshot.</summary>
    public WorldEventSnapshot WorldEvents { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        WorldEvents = new([CreateEvent()]);
    }

    private static WorldEvent CreateEvent() =>
        new(new WorldEventId(GreyboxReadyEventIdText), WorldEventTimeEffect.NoAdvance);
}
