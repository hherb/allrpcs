using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one temporary no-advance greybox-ready world event.</summary>
public partial class TemporaryWorldEventSource : Node
{
    private const string GreyboxReadyEventIdText = "greybox-ready";

    /// <summary>Gets the single retained temporary world event.</summary>
    public WorldEvent Event { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Event = CreateEvent();
    }

    private static WorldEvent CreateEvent() =>
        new(new WorldEventId(GreyboxReadyEventIdText), WorldEventTimeEffect.NoAdvance);
}
