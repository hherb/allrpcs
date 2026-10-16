using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Movement;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Executes one accepted examination at a time, outside the completed dialogue turn.</summary>
public partial class HerbExaminationActionController : Node
{
    private const double MaximumTravelSeconds = 45;
    private const double MaximumStationarySeconds = 3;
    private Healer healer = null!;
    private GreyboxInteractionTarget target = null!;
    private CharacterId actor = null!;
    private ObjectId objectId = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterTurnTranscriptController transcript = null!;
    private WorldEventController world = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private Label status = null!;
    private CharacterTurnRequest? invalidatedRequest;
    private CharacterTurnResponse? lastOffered;
    private CharacterTurnActivity? pending;
    private AcceptedCharacterExamination? execution;
    private ulong commandRevision;
    private Vector2 destination;
    private Vector2 waypoint;
    private readonly Queue<Vector2> approach = new();
    private Vector2 lastPosition;
    private double elapsed;
    private double stationary;

    internal bool Running => pending is not null || execution is not null;
    internal string Status => status.Text;
    internal CharacterTurnWorldEventOccurrence? LastOccurrence { get; private set; }
    internal CharacterExaminationResult? LastResult { get; private set; }
    internal int CompletedCount { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        healer = GetNode<Healer>("../Healer");
        target = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch");
        actor = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id;
        objectId = GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource")
            .WorldObjects.Entries[0].Identity.Id;
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        world = GetNode<WorldEventController>("../WorldEventController");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        restoration.PresentationReset += OnRestored;
        // Separate action feedback from both the committed reply and transient generation text.
        status = new() { Name = "ActionStatus", Position = new(124, 178), Size = new(800, 23),
            Visible = false, MouseFilter = Control.MouseFilterEnum.Ignore, AccessibilityName = "Healer action",
            AccessibilityLive = DisplayServer.AccessibilityLiveMode.Polite };
        status.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new(0.055f, 0.067f, 0.082f, 0.96f),
            ContentMarginLeft = 4, ContentMarginRight = 4, ContentMarginTop = 2, ContentMarginBottom = 2 });
        status.AddThemeColorOverride("font_outline_color", Colors.Black);
        status.AddThemeConstantOverride("outline_size", 6);
        GetNode<Control>("../DialogueOverlay/Panel").AddChild(status);
    }

    /// <summary>Receives only the exact newly installed ordinary generated response from Main.</summary>
    internal void Offer(CharacterTurnActivity activity)
    {
        if (activity.State != CharacterTurnActivityState.Speaking || !ReferenceEquals(turns.Activity, activity)
            || activity.Response?.Disposition != CharacterTurnDisposition.Accept
            || activity.Response.ImmediateIntention.Type != CharacterIntentionType.ExamineObject
            || ReferenceEquals(activity.Response, lastOffered)) { return; }
        lastOffered = activity.Response;
        if (ReferenceEquals(activity.Request, invalidatedRequest)) { return; }
        Cancel();
        pending = activity;
        elapsed = 0;
        SetStatus("Healer: preparing to examine the herb patch…");
    }

    /// <summary>Cancels pending action ownership without stopping a replacement manual command.</summary>
    internal void Cancel()
    {
        bool running = Running;
        pending = null;
        if (execution is not null && healer.CommandRevision == commandRevision)
        { healer.CommandWait(); }
        execution = null;
        approach.Clear();
        if (running) { SetStatus("Healer: examination cancelled."); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Running) { return; }
        elapsed += delta;
        if (pending is { } offered)
        {
            if (!ReferenceEquals(turns.Activity?.Response, offered.Response)
                || turns.Activity?.State == CharacterTurnActivityState.Cancelled)
            { Fail("examination cancelled before it started."); return; }
            AcceptedCharacterExamination? accepted = AcceptedCharacterExamination.TryAccept(
                offered, actor, objectId, transcript.Transcript!);
            if (accepted is null)
            {
                // Exact spoken occurrence is recorded later in the same scene frame.
                if (elapsed > 3 || offered.Response?.Speech is null)
                { Fail("could not accept that examination."); }
                return;
            }
            pending = null;
            if (!healer.Visible) { Fail("is away and cannot examine the herb patch yet."); return; }
            if (!ValidTarget()) { Fail("examination target is no longer available."); return; }
            destination = target.GlobalPosition;
            PlanApproach();
            if (!StartNextLeg()) { Fail("cannot reach the herb patch."); return; }
            execution = accepted;
            lastPosition = healer.GlobalPosition;
            stationary = 0;
            elapsed = 0;
            SetStatus("Healer: walking to examine the herb patch…");
        }
        if (execution is null) { return; }
        if (!ValidTarget() || target.GlobalPosition != destination || !healer.Visible)
        { Fail("examination target is no longer available."); return; }
        if (healer.CommandRevision != commandRevision)
        { Fail("examination interrupted by a new movement command."); return; }
        if (ReferenceEquals(turns.Activity?.Response, execution.Response)
            && turns.Activity?.State == CharacterTurnActivityState.Cancelled)
        { Cancel(); return; }
        if (healer.GlobalPosition.DistanceTo(waypoint) <= healer.DestinationStopDistancePixels)
        {
            if (approach.Count > 0)
            {
                if (!StartNextLeg()) { Fail("cannot reach the herb patch."); }
                return;
            }
            CharacterTurnWorldEventOccurrence? occurred = world.TryOccurExamination(execution, transcript);
            if (occurred is null) { Fail("examination could not be recorded."); return; }
            LastOccurrence = occurred;
            LastResult = GetNode<TemporaryCharacterExaminationResultSource>(
                "../TemporaryCharacterExaminationResultSource").CreateResult(occurred, objectId);
            CompletedCount++;
            healer.CommandWait();
            execution = null;
            SetStatus(LastResult?.Safety == CharacterExaminationSafety.SafeToCollect
                ? "Healer: examined the herb patch. Safe to collect; herbs left in place."
                : "Healer: examination finished; safety not established. Herbs left in place.");
            return;
        }
        stationary = healer.GlobalPosition.DistanceTo(lastPosition) < 0.1f ? stationary + delta : 0;
        lastPosition = healer.GlobalPosition;
        if (elapsed > MaximumTravelSeconds || stationary > MaximumStationarySeconds
            || healer.CommandMode != HealerCommandMode.MoveToDestination)
        { Fail("cannot reach the herb patch; examination stopped."); }
    }

    // The navigation corridor includes the clearing wall endpoints. Pass through the authored
    // opening center with body clearance instead of clipping a tree boundary at a corridor edge.
    private void PlanApproach()
    {
        approach.Clear();
        var clearing = GetNode<OpeningRegionWoodlandClearingSceneSource>(
            "../OpeningRegionWoodlandClearingSceneSource");
        var bounds = clearing.Binding.WoodlandClearingBounds;
        bool Inside(Vector2 p) => p.X >= bounds.Left && p.X <= bounds.Right
            && p.Y >= bounds.Top && p.Y <= bounds.Bottom;
        if (!Inside(healer.GlobalPosition) && Inside(destination))
        {
            var path = NavigationServer2D.MapGetPath(healer.GetWorld2D().NavigationMap,
                healer.GlobalPosition, destination, true);
            Vector2? entry = path.Where(Inside).Select(p => (Vector2?)p).FirstOrDefault();
            if (entry is { } crossing)
            {
                var layout = clearing.Layout;
                (System.Numerics.Vector2 Gate, string Outside)[] gates =
                [ (layout.VillageEdgeOpening, "village-edge"), (layout.LaundryOpening, "abandoned-laundry"),
                  (layout.ShrineOpening, "old-shrine"), (layout.ClosedForestPathOpening, "closed-deeper-forest-path") ];
                var selected = gates.MinBy(g => new Vector2(g.Gate.X, g.Gate.Y).DistanceSquaredTo(crossing));
                Vector2 gate = new(selected.Gate.X, selected.Gate.Y);
                Vector2 outside = OpeningRegionLayoutGeometry.Anchors[selected.Outside];
                Vector2 center = OpeningRegionLayoutGeometry.Anchors["woodland-clearing"];
                const float clearance = 40;
                approach.Enqueue(gate + (outside - gate).Normalized() * clearance);
                approach.Enqueue(gate + (center - gate).Normalized() * clearance);
            }
        }
        approach.Enqueue(destination);
    }

    private bool StartNextLeg()
    {
        waypoint = approach.Dequeue();
        if (!healer.TryCommandExaminationMove(waypoint)) { return false; }
        commandRevision = healer.CommandRevision;
        stationary = 0;
        lastPosition = healer.GlobalPosition;
        return true;
    }

    private bool ValidTarget() => IsInstanceValid(target) && target.IsInsideTree()
        && target.ObjectId == objectId && target.ObjectIdText == objectId.Value
        && target.IsInteractionEnabled && target.IsInteractionVisible && target.Visible
        && target.GlobalPosition.IsFinite();

    private void Fail(string message) { Cancel(); SetStatus("Healer: " + message); }
    private void SetStatus(string message) { if (IsInstanceValid(status)) { status.Text = message; status.Visible = true; } }
    private void OnRestored()
    {
        invalidatedRequest = turns.Activity?.Request;
        Cancel(); LastOccurrence = null; LastResult = null;
        SetStatus("Healer: examination reset after load.");
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        restoration.PresentationReset -= OnRestored;
        Cancel();
    }
}
