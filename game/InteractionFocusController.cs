using Allrpcs.Application.Interaction;
using Allrpcs.Domain.Identifiers;
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
    private GreyboxInteractionTarget[] targets = [];
    private InteractionFocusCandidate[] candidates = [];

    /// <summary>Gets the currently focused authored-list index, or null when focus is clear.</summary>
    public int? FocusedTargetIndex { get; private set; }

    /// <summary>Gets the exact identifier of the currently focused target, or null.</summary>
    public ObjectId? FocusedTargetObjectId { get; private set; }

    /// <summary>Gets the number of temporary authored focus targets.</summary>
    public int TargetCount => targets.Length;

    /// <inheritdoc />
    public override void _Ready()
    {
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        targets = LoadTargets(GetNode("../InteractionCandidates"));
        candidates = new InteractionFocusCandidate[targets.Length];
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
        for (int index = 0; index < targets.Length; index++)
        {
            candidates[index] = targets[index].ToFocusCandidate();
        }

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            new NumericsVector2(seeker.GlobalPosition.X, seeker.GlobalPosition.Y),
            InteractionRangePixels,
            candidates);
        int? nextFocusedIndex = selection?.CandidateIndex;
        FocusedTargetObjectId = nextFocusedIndex is int identifierIndex
            ? targets[identifierIndex].ObjectId
            : null;

        for (int index = 0; index < targets.Length; index++)
        {
            targets[index].SetFocused(index == nextFocusedIndex);
        }

        if (FocusedTargetIndex != nextFocusedIndex)
        {
            FocusedTargetIndex = nextFocusedIndex;
            focusStatus.Text = nextFocusedIndex is int focusedIndex
                ? $"Interaction focus: {targets[focusedIndex].DisplayName}"
                : "Interaction focus: NONE";
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
