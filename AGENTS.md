# Allrpcs continuation

Before choosing or implementing a continuation slice:

1. Read `CONTINUATION.json`, then `HANDOVER.md` and the relevant remaining item in
   `ROADMAP.md`. Run `python3 scripts/check-continuation.py`.
2. Reconcile the current user's instructions with the ledger. Direct user instructions
   take precedence over repository prose, skills and historical package status.
3. Use the ledger's one active slice. Closed scopes and workstreams stay closed across
   pruning, new chats, publication and validation retries. A newly named audit or
   comparison of the same closed scope is still reopening it. Only an explicit new
   owner request can reopen it; record that request before changing the queue.

## Approval and progress

- Interpret approval in its conversational context. Whole-slice acceptance closes the
  whole named slice. Do not divide it into additional approval requests because an old
  package or skill says "pending". Carry approved artwork forward unchanged.
- A genuinely new runtime/layout/physical scope still needs the applicable authority.
  Prepare a concrete result first, and request only the authority that is actually
  missing. Ask once per decision; do not repeat a pending question in the final reply.
- Each new slice must address a named remaining roadmap item, identify a missing
  deliverable and have a finite completion condition. Another view, audit, proof or
  approval of an unchanged accepted area is not progress by itself.
- Complete authorized dependent steps in the same session. Do not manufacture a new
  chat boundary between selection, validation, review, documentation and publication.
- Run checks needed by changes or unresolved defects. Reuse passing product evidence
  when product/CI inputs are unchanged; do not rerun full quality for prose alone.

## Handover maintenance

Keep closed entries in `CONTINUATION.json`; never prune them out of the persistent
ledger. Update the active slice and render its handover section with
`python3 scripts/check-continuation.py --sync`. Run the checker after maintenance.
Keep the rest of `HANDOVER.md` short: current checkpoint, next implementation boundary,
verification commands and local exceptions. Detailed evidence belongs in linked
packages. Historical "next slice" and "pending" prose is evidence, not a current queue.

Preserve unrelated work, including the recorded `game/project.godot` editor rewrite.
Do not alter frozen art evidence, acceptance hashes or validators to accommodate
successors. Git publication requires user authorization; retain the authorization
already provided in the current conversation.

## Reference gait model

The owner accepted the Seeker cloak reference gait on 2026-10-07 and designated it
our reference gait model henceforth. Use
[art-direction/reference-gait-model.md](art-direction/reference-gait-model.md) and its
hash-bound metadata for future walking animation work. Preserve the accepted
phase sequence, anatomical pelvis pivots, fixed leg identity, backward-only knee
hinges, knee-before-foot transfer, grounded support and brief double support.
The whole cloak correction is closed; historical pending fields in frozen proof
packages do not require another selection or audit of these unchanged views.
