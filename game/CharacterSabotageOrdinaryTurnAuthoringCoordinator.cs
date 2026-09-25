using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors and composes eligible sabotage kinds for one exact current ordinary turn.</summary>
public partial class CharacterSabotageOrdinaryTurnAuthoringCoordinator : Node
{
    private CharacterTurnActivityController turns = null!;
    private TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source = null!;
    private TemporaryCharacterObjectSabotageSource concealSource = null!;
    private TemporaryCharacterObjectSabotageMoveSource moveSource = null!;
    private TemporaryCharacterObjectSabotageSubstitutionSource substitutionSource = null!;
    private TemporaryCharacterObjectSabotageContaminationSource contaminationSource = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceSource misquotationSource = null!;
    private CharacterObjectSabotageConcealmentController concealState = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterSabotageActionConcealmentRequestController concealAuthor = null!;
    private CharacterSabotageActionMoveRequestController moveAuthor = null!;
    private CharacterSabotageActionSubstitutionRequestController substitutionAuthor = null!;
    private CharacterSabotageActionContaminationRequestController contaminationAuthor = null!;
    private CharacterSabotageActionMisquotationRequestController misquotationAuthor = null!;
    private CharacterSabotageActionDelayRequestController delayAuthor = null!;
    private CharacterSabotageActionInfluenceRequestController influenceAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;

    /// <summary>Gets the exact ordinary-turn token retained by successful authoring.</summary>
    public CharacterTurnActivity? Turn { get; private set; }

    /// <summary>Gets the exact enemy actor retained by successful authoring.</summary>
    public Character? Actor { get; private set; }

    /// <summary>Gets the exact canonical request produced by successful composition.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <summary>Gets the exact canonical author-output snapshot retained by composition.</summary>
    internal IReadOnlyList<CharacterSabotageActionConsiderationRequest?>? AuthorOutputs
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        source = GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
            "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        concealSource = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        moveSource = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        substitutionSource = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        contaminationSource = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        misquotationSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        concealState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        concealAuthor = GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController");
        moveAuthor = GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController");
        substitutionAuthor = GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController");
        contaminationAuthor = GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController");
        misquotationAuthor = GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController");
        delayAuthor = GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController");
        influenceAuthor = GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController");
        authors =
        [
            concealAuthor,
            moveAuthor,
            substitutionAuthor,
            contaminationAuthor,
            misquotationAuthor,
            delayAuthor,
            influenceAuthor,
        ];
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
    }

    /// <summary>Invokes all exact current authors once and composes their eligible outputs.</summary>
    public bool TryAuthor(CharacterTurnActivity? expectedTurn, Character? expectedActor)
    {
        CharacterTurnActivity? currentTurn = turns.Activity;
        if (Turn is not null
            || Actor is not null
            || Request is not null
            || AuthorOutputs is not null
            || currentTurn is null
            || expectedActor is null
            || !ReferenceEquals(currentTurn, expectedTurn)
            || !ReferenceEquals(expectedActor, source.Actor)
            || !ReferenceEquals(currentTurn.Request.RespondingCharacterId, expectedActor.Identity.Id)
            || !ReferenceEquals(currentTurn.State, CharacterTurnActivityState.AwaitingResponse)
            || !HasFreshCompositionGraph()
            || !HasCurrentSceneAuthority())
        {
            return false;
        }

        bool[] eligible = EvaluateEligibility(expectedActor);
        if (!eligible.Any(value => value))
        {
            return false;
        }

        bool[] authored = InvokeAuthors(expectedActor);
        if (!authored.SequenceEqual(eligible))
        {
            throw new InvalidOperationException(
                "Preflighted sabotage authors must publish exactly their eligible outputs. "
                + $"Eligible={string.Join(',', eligible)}; authored={string.Join(',', authored)}.");
        }

        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        if (!composer.TryCompose(expectedActor, outputs)
            || composer.Request is not { } request
            || composer.ComposedAuthorOutputs is not { } composedOutputs)
        {
            throw new InvalidOperationException(
                "Preflighted sabotage author outputs must compose exactly once.");
        }

        Turn = currentTurn;
        Actor = expectedActor;
        Request = request;
        AuthorOutputs = composedOutputs;
        return true;
    }

    /// <summary>Checks the complete retained authoring graph for a downstream exact request.</summary>
    internal bool IsCurrent(
        CharacterTurnActivity? expectedTurn,
        CharacterSabotageActionConsiderationRequest? expectedRequest) =>
        IsRetained(expectedTurn, expectedRequest)
        && HasCurrentSceneAuthority();

    /// <summary>Checks retained authoring provenance after action authority has advanced.</summary>
    internal bool IsRetained(
        CharacterTurnActivity? expectedTurn,
        CharacterSabotageActionConsiderationRequest? expectedRequest) =>
        Turn is not null
        && Actor is not null
        && Request is not null
        && AuthorOutputs is not null
        && ReferenceEquals(Turn, expectedTurn)
        && ReferenceEquals(Request, expectedRequest)
        && ReferenceEquals(turns.Activity, Turn)
        && ReferenceEquals(Turn.Request.RespondingCharacterId, Actor.Identity.Id)
        && ReferenceEquals(Turn.State, CharacterTurnActivityState.AwaitingResponse)
        && ReferenceEquals(composer.Request, Request)
        && ReferenceEquals(composer.ComposedAuthorOutputs, AuthorOutputs)
        && AuthorOutputs.Count == authors.Length
        && AuthorOutputs.Select((output, index) => ReferenceEquals(output, authors[index].Request))
            .All(value => value);

    private bool HasFreshCompositionGraph() =>
        composer.Request is null
        && composer.ComposedAuthorOutputs is null
        && authors.All(author => author.Request is null && author.CandidateBinding is null);

    private bool HasCurrentSceneAuthority()
    {
        CharacterTargetSabotageDelayConsiderationRequest delay = source.DelayRequest;
        CharacterTargetSabotageInfluenceConsiderationRequest influence = source.InfluenceRequest;
        bool delayUnavailable = delayState.EventLog is null && delayState.WorldTime is null;
        bool delayCurrent = ReferenceEquals(delayState.EventLog, delay.EventLog)
            && ReferenceEquals(delayState.WorldTime, delay.WorldTime);
        bool influenceCurrent = influenceState.Relationship is null
            || ReferenceEquals(influenceState.Relationship, influence.Relationship);
        return ReferenceEquals(concealState.Concealment, concealSource.Concealment)
            && ReferenceEquals(concealState.Evidence, concealSource.Evidence)
            && ReferenceEquals(moveState.Placements, moveSource.Placements)
            && ReferenceEquals(moveState.Evidence, moveSource.Evidence)
            && ReferenceEquals(substitutionState.Substitutions, substitutionSource.Substitutions)
            && ReferenceEquals(substitutionState.Evidence, substitutionSource.Evidence)
            && ReferenceEquals(contaminationState.Contamination, contaminationSource.Contamination)
            && ReferenceEquals(contaminationState.Evidence, contaminationSource.Evidence)
            && (delayUnavailable || delayCurrent)
            && influenceCurrent
            && ReferenceEquals(delay.Actor, source.Actor)
            && ReferenceEquals(influence.Actor, source.Actor)
            && delay.Candidates.Count > 0
            && influence.Candidates.Count > 0;
    }

    private bool[] EvaluateEligibility(Character actor)
    {
        CharacterTargetSabotageDelayConsiderationRequest delay = source.DelayRequest;
        CharacterTargetSabotageInfluenceConsiderationRequest influence = source.InfluenceRequest;
        return
        [
            ReferenceEquals(concealSource.Character, actor)
                && CharacterObjectSabotageConcealmentPolicy.CanApply(
                actor,
                concealSource.PhysicalAccess,
                concealSource.WorldObjects,
                concealSource.Concealment,
                concealSource.Evidence,
                concealSource.Candidate),
            ReferenceEquals(moveSource.Character, actor)
                && CharacterObjectSabotageMovePolicy.CanApply(
                actor,
                moveSource.PhysicalAccess,
                moveSource.WorldObjects,
                moveSource.Placements,
                moveSource.Evidence,
                moveSource.Candidate),
            ReferenceEquals(substitutionSource.Character, actor)
                && CharacterObjectSabotageSubstitutionPolicy.CanApply(
                actor,
                substitutionSource.PhysicalAccess,
                substitutionSource.WorldObjects,
                substitutionSource.Substitutions,
                substitutionSource.Evidence,
                substitutionSource.Candidate),
            ReferenceEquals(contaminationSource.Character, actor)
                && CharacterObjectSabotageContaminationPolicy.CanApply(
                actor,
                contaminationSource.PhysicalAccess,
                contaminationSource.WorldObjects,
                contaminationSource.Contamination,
                contaminationSource.Evidence,
                contaminationSource.Candidate),
            ReferenceEquals(misquotationSource.Actor, actor)
                && CharacterDialogueMisquotationEvidencePolicy.CanObserve(
                actor,
                misquotationSource.Candidate),
            delayState.EventLog is not null
                && CharacterTargetSabotageDelayPolicy.CanApply(
                    actor,
                    delay.InteractionAccess,
                    delay.WorldEvents,
                    delay.EventLog,
                    delay.WorldTime,
                    delay.Candidates[0]),
            influenceState.Relationship is not null
                && CharacterTargetSabotageInfluencePolicy.CanApply(
                    actor,
                    influence.InteractionAccess,
                    influence.Relationship,
                    influence.Candidates[0]),
        ];
    }

    private bool[] InvokeAuthors(Character actor)
    {
        CharacterTargetSabotageDelayConsiderationRequest delay = source.DelayRequest;
        CharacterTargetSabotageInfluenceConsiderationRequest influence = source.InfluenceRequest;
        return
        [
            concealAuthor.TryCreate(
                actor,
                concealSource.PhysicalAccess,
                concealSource.WorldObjects,
                concealSource.Concealment,
                concealSource.Evidence,
                concealSource.Candidate),
            moveAuthor.TryCreate(
                actor,
                moveSource.PhysicalAccess,
                moveSource.WorldObjects,
                moveSource.Placements,
                moveSource.Evidence,
                moveSource.Destination,
                moveSource.Candidate),
            substitutionAuthor.TryCreate(
                actor,
                substitutionSource.PhysicalAccess,
                substitutionSource.WorldObjects,
                substitutionSource.Substitutions,
                substitutionSource.Evidence,
                substitutionSource.Candidate),
            contaminationAuthor.TryCreate(
                actor,
                contaminationSource.PhysicalAccess,
                contaminationSource.WorldObjects,
                contaminationSource.Contamination,
                contaminationSource.Evidence,
                contaminationSource.Candidate),
            misquotationAuthor.TryCreate(
                actor,
                misquotationSource.SourceEvent,
                misquotationSource.DeliveredEvent,
                misquotationSource.Occurrence,
                misquotationSource.WitnessCharacterId,
                misquotationSource.Candidate),
            delayAuthor.TryCreate(
                delay,
                actor,
                delay.InteractionAccess,
                delay.WorldEvents,
                delay.EventLog,
                delay.WorldTime,
                delay.Candidates[0]),
            influenceAuthor.TryCreate(
                influence,
                actor,
                influence.InteractionAccess,
                influence.Relationship,
                influence.Candidates[0]),
        ];
    }
}
