using Allrpcs.Application.Input;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Input;

public sealed class FocusedSearchInputCoordinatorTests
{
    [Fact]
    public void CancelTargetsOnlyTheSeekersActiveSearch()
    {
        FakePorts ports = new();
        FocusedSearchRequest other = new(new CharacterId("healer"), ports.Stone.Identity.Id);
        FocusedSearchRequest own = new(ports.SearcherCharacterId, ports.Stone.Identity.Id);
        ports.Activities = new([new(other), new(own)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Cancel);

        Assert.Same(own, ports.CancelledRequest);
        Assert.Equal("Focused search CANCEL: CHANGED", ports.Result);
    }

    [Fact]
    public void ActionsWithoutTheirSubjectAreUnchanged()
    {
        FakePorts ports = new();
        FocusedSearchInputCoordinator coordinator = new(ports);

        coordinator.Handle(FocusedSearchInputAction.Cancel);
        Assert.Equal("Focused search CANCEL: UNCHANGED", ports.Result);
        coordinator.Handle(FocusedSearchInputAction.Complete);
        Assert.Equal("Focused search COMPLETE: UNCHANGED", ports.Result);
        coordinator.Handle(FocusedSearchInputAction.Resolve);
        Assert.Equal("Focused search RESOLVE: UNCHANGED", ports.Result);
        coordinator.Handle(FocusedSearchInputAction.Start);
        Assert.Equal("Focused search START: UNCHANGED", ports.Result);
        Assert.Empty(ports.Calls);
    }

    [Fact]
    public void CompleteUsesTheAuthoredOutcomeForTheFoundObject()
    {
        FakePorts ports = new();
        FocusedSearchRequest own = new(ports.SearcherCharacterId, ports.Stone.Identity.Id);
        ports.Activities = new([new(own)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Complete);

        Assert.Equal(["create-outcome", "complete"], ports.Calls);
        Assert.Same(ports.Stone, ports.OutcomeWorldObject);
        Assert.Same(own, ports.CompletedOutcome!.Request);
        Assert.Equal("Focused search COMPLETE: CHANGED", ports.Result);
    }

    [Fact]
    public void StartSearchesTheFocusedWorldObject()
    {
        FakePorts ports = new() { FocusedTargetObjectId = new ObjectId("stone") };

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Start);

        Assert.Same(ports.Stone, ports.StartedWorldObject);
        Assert.Equal("Focused search START: CHANGED", ports.Result);
    }

    [Fact]
    public void ResolvePresentsAndRecoversOnlyMatchingSabotageEvidence()
    {
        FakePorts ports = new();
        ports.Outcomes = new([Outcome(ports, ports.Stone, VisibilityState.Understood)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve);

        Assert.Equal(["resolve", "refresh", "recover:stone", "success"], ports.Calls);
        Assert.Same(ports.Perception, ports.RecoveredPerception);
        Assert.Equal("Focused search RESOLVE: CHANGED", ports.Result);
    }

    [Fact]
    public void ResolvedWagonClueRoutesWithTheResolvedPerception()
    {
        FakePorts ports = new();
        ports.Outcomes = new([Outcome(ports, ports.Wagon, VisibilityState.Understood)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve);

        Assert.Equal(["resolve", "route-wagon", "refresh", "success"], ports.Calls);
        Assert.Same(ports.ResolvedPerception, ports.RoutedPerception);
    }

    [Fact]
    public void FailedResolutionChangesNothing()
    {
        FakePorts ports = new() { ResolveResult = false };
        ports.Outcomes = new([Outcome(ports, ports.Wagon, VisibilityState.Understood)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve);

        Assert.Equal(["resolve"], ports.Calls);
        Assert.Equal("Focused search RESOLVE: UNCHANGED", ports.Result);
    }

    [Fact]
    public void IdentifyingTheHerbPatchOccursItsWorldEventAtomically()
    {
        FakePorts ports = new();
        ports.Outcomes = new([Outcome(ports, ports.HerbPatch, VisibilityState.Identified)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve);

        Assert.Equal(["resolve", "occur:herb-patch-identified", "refresh", "success"], ports.Calls);
    }

    [Fact]
    public void HerbPatchIdentificationIsPrevalidatedAgainstWorldTime()
    {
        FakePorts ports = new();
        ports.Outcomes = new([Outcome(ports, ports.HerbPatch, VisibilityState.Identified)]);
        ports.EventLog = new([ports.HerbPatchIdentifiedWorldEvent.EventId]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve);

        Assert.Empty(ports.Calls);
        Assert.Equal("Focused search RESOLVE: UNCHANGED", ports.Result);
    }

    [Fact]
    public void PrevalidatedHerbPatchCommitFailureIsAnInvariantViolation()
    {
        FakePorts ports = new() { OccurResult = false };
        ports.Outcomes = new([Outcome(ports, ports.HerbPatch, VisibilityState.Identified)]);

        Assert.Throws<InvalidOperationException>(
            () => new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve));
    }

    [Fact]
    public void NonIdentifyingHerbPatchOutcomeResolvesWithoutTheWorldEvent()
    {
        FakePorts ports = new();
        ports.Outcomes = new([Outcome(ports, ports.HerbPatch, VisibilityState.Understood)]);

        new FocusedSearchInputCoordinator(ports).Handle(FocusedSearchInputAction.Resolve);

        Assert.Equal(["resolve", "refresh", "success"], ports.Calls);
    }

    private static FocusedSearchOutcome Outcome(
        FakePorts ports,
        WorldObject target,
        VisibilityState visibility) =>
        new(new FocusedSearchRequest(ports.SearcherCharacterId, target.Identity.Id), visibility);

    private static WorldObject WorldObject(string id) =>
        new(new WorldObjectIdentity(new ObjectId(id), id), new WorldObjectDescription(string.Empty));

    private sealed class FakePorts : IFocusedSearchInputPorts
    {
        public List<string> Calls { get; } = [];

        public WorldObject HerbPatch { get; } = WorldObject("herb-patch");

        public WorldObject Wagon { get; } = WorldObject("wagon-clue");

        public WorldObject Stone { get; } = WorldObject("stone");

        public bool ResolveResult { get; set; } = true;

        public bool OccurResult { get; set; } = true;

        public string? Result { get; private set; }

        public FocusedSearchRequest? CancelledRequest { get; private set; }

        public WorldObject? OutcomeWorldObject { get; private set; }

        public FocusedSearchOutcome? CompletedOutcome { get; private set; }

        public WorldObject? StartedWorldObject { get; private set; }

        public CharacterPerceptionSnapshot? RecoveredPerception { get; private set; }

        public CharacterPerceptionSnapshot? RoutedPerception { get; private set; }

        public CharacterPerceptionSnapshot ResolvedPerception { get; }

        public CharacterId SearcherCharacterId { get; } = new("seeker");

        public FocusedSearchActivitySnapshot Activities { get; set; } = new([]);

        public FocusedSearchOutcomeSnapshot Outcomes { get; set; } = new([]);

        public CharacterPerceptionSnapshot? Perception { get; private set; }

        public ObjectId? FocusedTargetObjectId { get; set; }

        public WorldObject AuthoredHerbPatch => HerbPatch;

        public WorldObject WagonClue => Wagon;

        public WorldEvent HerbPatchIdentifiedWorldEvent { get; } =
            new(new WorldEventId("herb-patch-identified"), WorldEventTimeEffect.Advance);

        public WorldEventSnapshot? WorldEvents => new([HerbPatchIdentifiedWorldEvent]);

        public WorldEventLogSnapshot? EventLog { get; set; } = new([]);

        public WorldTimeSnapshot? WorldTime { get; } = new(null);

        public IReadOnlyList<FocusedSearchEvidenceRecovery> EvidenceRecoveries =>
        [
            new(Stone.Identity.Id, Recover("stone")),
            new(new ObjectId("water-cask"), Recover("water-cask")),
        ];

        public FakePorts()
        {
            ResolvedPerception = new(
                SearcherCharacterId,
                [new WorldObjectVisibility(Wagon.Identity.Id, VisibilityState.Understood)]);
            Perception = new(
                SearcherCharacterId,
                [new WorldObjectVisibility(HerbPatch.Identity.Id, VisibilityState.Noticed)]);
        }

        public WorldObject? FindWorldObject(ObjectId objectId) =>
            new[] { HerbPatch, Wagon, Stone }.FirstOrDefault(worldObject => worldObject.Identity.Id == objectId);

        public bool TryCancelSearch(FocusedSearchRequest request)
        {
            Calls.Add("cancel");
            CancelledRequest = request;
            return true;
        }

        public FocusedSearchOutcome? CreateOutcome(FocusedSearchRequest request, WorldObject? worldObject)
        {
            Calls.Add("create-outcome");
            OutcomeWorldObject = worldObject;
            return new(request, VisibilityState.Noticed);
        }

        public bool TryCompleteSearch(FocusedSearchOutcome outcome)
        {
            Calls.Add("complete");
            CompletedOutcome = outcome;
            return true;
        }

        public bool TryResolveOutcome(FocusedSearchOutcome outcome, WorldObject worldObject)
        {
            Calls.Add("resolve");
            if (ResolveResult && ReferenceEquals(worldObject, Wagon))
            {
                Perception = ResolvedPerception;
            }

            return ResolveResult;
        }

        public bool TryRouteWagonClue(
            FocusedSearchOutcome outcome,
            WorldObject worldObject,
            CharacterPerceptionSnapshot resolvedPerception)
        {
            Calls.Add("route-wagon");
            RoutedPerception = resolvedPerception;
            return true;
        }

        public bool TryOccurWorldEvent(WorldEventId eventId)
        {
            Calls.Add("occur:" + eventId.Value);
            return OccurResult;
        }

        public bool TryStartFocusedSearch(WorldObject worldObject)
        {
            Calls.Add("start");
            StartedWorldObject = worldObject;
            return true;
        }

        public void RefreshResolvedPerception() => Calls.Add("refresh");

        public void PresentSuccessfulInteraction() => Calls.Add("success");

        public void PresentResult(string text) => Result = text;

        private Func<CharacterPerceptionSnapshot, bool> Recover(string name) => perception =>
        {
            Calls.Add("recover:" + name);
            RecoveredPerception = perception;
            return true;
        };
    }
}
