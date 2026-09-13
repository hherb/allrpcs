using Allrpcs.Application.Interaction;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Presents the pure deterministic focus selection over temporary authored greybox markers.
/// </summary>
public partial class InteractionFocusController : Node
{
    /// <summary>The inclusive greybox interaction-focus range in pixels.</summary>
    public const float InteractionRangePixels = 112.0f;

    private CharacterBody2D seeker = null!;
    private Label focusStatus = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private WorldObjectController worldObjectController = null!;
    private GreyboxInteractionTarget[] targets = [];
    private Label?[] targetLabels = [];
    private InteractionFocusCandidate[] candidates = [];
    private ObjectId?[] candidateWorldObjectIds = [];
    private string?[] candidateWorldObjectDisplayNames = [];

    /// <summary>Gets the currently focused authored-list index, or null when focus is clear.</summary>
    public int? FocusedTargetIndex { get; private set; }

    /// <summary>Gets the exact identifier of the currently focused target, or null.</summary>
    public ObjectId? FocusedTargetObjectId { get; private set; }

    /// <summary>Gets the number of temporary authored focus targets.</summary>
    public int TargetCount => targets.Length;

    /// <summary>Creates a focused-search request for the matching focused world object, or null.</summary>
    /// <param name="searcherCharacterId">The existing identifier of the searcher.</param>
    /// <param name="worldObject">The existing nullable world object for the current focus.</param>
    /// <returns>A new request retaining the supplied and world-object identifiers, or null.</returns>
    public FocusedSearchRequest? CreateFocusedSearchRequest(
        CharacterId searcherCharacterId,
        WorldObject? worldObject)
    {
        ArgumentNullException.ThrowIfNull(searcherCharacterId);

        return FocusedTargetObjectId is ObjectId focusedTargetObjectId
            && worldObject is not null
            && worldObject.Identity.Id == focusedTargetObjectId
            ? new FocusedSearchRequest(searcherCharacterId, worldObject.Identity.Id)
            : null;
    }

    /// <summary>
    /// Attempts to start a focused search for the current valid interaction focus.
    /// </summary>
    public bool TryStartFocusedSearch(
        WorldObject? worldObject,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory)
    {
        FocusedSearchRequest? request = CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId,
            worldObject);

        return request is not null
            && focusedSearchController.TryStartSearch(
                request,
                conditions,
                capabilities,
                inventory);
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        targets = LoadTargets(GetNode("../InteractionCandidates"));
        targetLabels = targets.Select(target => target.GetNodeOrNull<Label>("Label")).ToArray();
        candidates = new InteractionFocusCandidate[targets.Length];
        candidateWorldObjectIds = new ObjectId?[targets.Length];
        candidateWorldObjectDisplayNames = new string?[targets.Length];
        RefreshFocus();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        RefreshFocus();
    }

    /// <summary>Re-evaluates and presents interaction focus for the current Seeker position.</summary>
    public void RefreshFocus()
    {
        RefreshFocusFrom(new NumericsVector2(seeker.GlobalPosition.X, seeker.GlobalPosition.Y));
    }

    /// <summary>Re-evaluates and presents interaction focus from one supplied position.</summary>
    /// <param name="seekerPosition">The finite position to evaluate without moving the Seeker.</param>
    public void RefreshFocusFrom(NumericsVector2 seekerPosition)
    {
        for (int index = 0; index < targets.Length; index++)
        {
            GreyboxInteractionTarget target = targets[index];
            WorldObject? worldObject = worldObjectController.FindWorldObject(target.ObjectId);
            bool hasMatchingWorldObject = worldObject is not null
                && worldObject.Identity.Id == target.ObjectId;
            InteractionFocusCandidate candidate = target.ToFocusCandidate();
            candidates[index] = candidate with
            {
                IsInteractionEnabled = candidate.IsInteractionEnabled && hasMatchingWorldObject,
            };
            candidateWorldObjectIds[index] = hasMatchingWorldObject
                ? worldObject!.Identity.Id
                : null;
            candidateWorldObjectDisplayNames[index] = hasMatchingWorldObject
                ? worldObject!.Identity.DisplayName
                : null;
            Label? markerLabel = targetLabels[index];
            if (markerLabel is not null)
            {
                markerLabel.Text = candidateWorldObjectDisplayNames[index] ?? string.Empty;
            }
        }

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            seekerPosition,
            InteractionRangePixels,
            candidates);
        int? nextFocusedIndex = selection?.CandidateIndex;
        FocusedTargetObjectId = nextFocusedIndex is int identifierIndex
            ? candidateWorldObjectIds[identifierIndex]
            : null;

        for (int index = 0; index < targets.Length; index++)
        {
            targets[index].SetFocused(index == nextFocusedIndex);
        }

        string nextFocusStatusText = nextFocusedIndex is int focusedIndex
            ? $"Interaction focus: {candidateWorldObjectDisplayNames[focusedIndex]}"
            : "Interaction focus: NONE";
        if (FocusedTargetIndex != nextFocusedIndex
            || !string.Equals(focusStatus.Text, nextFocusStatusText, StringComparison.Ordinal))
        {
            FocusedTargetIndex = nextFocusedIndex;
            focusStatus.Text = nextFocusStatusText;
        }
    }

    /// <summary>Gets a temporary target by authored list index for credential-free checks.</summary>
    /// <param name="index">The zero-based authored list index.</param>
    /// <returns>The matching greybox target.</returns>
    public GreyboxInteractionTarget GetTarget(int index)
    {
        return targets[index];
    }

    private static GreyboxInteractionTarget[] LoadTargets(Node candidateContainer)
    {
        List<GreyboxInteractionTarget> authoredTargets = [];
        foreach (Node child in candidateContainer.GetChildren())
        {
            if (child is GreyboxInteractionTarget target)
            {
                authoredTargets.Add(target);
            }
        }

        if (authoredTargets.Count > InteractionFocusSelector.MaximumCandidateCount)
        {
            throw new InvalidOperationException(
                "Authored interaction targets exceed the pure selector's candidate bound.");
        }

        return authoredTargets.ToArray();
    }
}
