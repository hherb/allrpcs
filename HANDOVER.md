# Allrpcs Handover

## Current state

`main` contains merged PR #207 at `b7b9ffb`. The current branch is
`codex/phase-7-participation-consideration`.

This branch adds four bounded Phase 7 Domain slices:

- `CharacterTurnParticipationConsiderationRequest` retains one exact scheduled `Character`, one
  exact heard `DialogueEvent`, and an explicit direct-address flag only when the character is a
  value-equal listener;
- `CharacterTurnParticipationConsiderationPolicy` validates a returned decision against that exact
  request through the existing exact-character, direct-address, and authored-motive gate;
- `CharacterTurnParticipationConsiderationRoundTransition` preflights at most 32 distinct scheduled
  listeners and 32 distinct direct addresses, then returns a stable independent read-only request
  collection retaining every exact character and the exact event;
- `CharacterTurnParticipationConsiderationResult` binds one accepted exact decision to its exact
  request and rejects incompatible, value-equal-but-distinct decision identity.

The Phase 7 parent items remain open. This branch does not assemble participation model context,
define or invoke a participation model/provider interface, deserialize a decision, obtain a
decision, collect a complete decision round, validate or select such a round in ordinary play,
construct or start its follow-up, initialize an ordinary autonomous budget, loop, change hearing or
roles, add whispering/private conversations, add independent tasks, implement sabotage, start Phase
8, release, sign, or distribute.

Draft PR #208 contains this work.

## Validation and review

- test-first compile failures were observed before each missing request, policy, round-transition,
  and result implementation;
- 32 focused Domain cases pass across the four new surfaces, covering addressed and unaddressed
  choices, response, interjection, meaningful silence, exact and distinct value-equal identity,
  authored motives, audience absence, complete preflight, stable order, empty and maximum rounds,
  null, duplicate, and oversized inputs, unchanged inputs, collection immutability, and smallest
  public surfaces;
- final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, the
  zero-warning solution build, 2,081 Domain, 402 Application, 447 Infrastructure, and 8 architecture
  tests, all 5 representative scenario definitions, the complete headless smoke matrix, and the
  bounded Compatibility-renderer launch;
- `git diff --check`, staged-diff review, practical 500-line/120-character limits, exact-identity and
  value-membership boundaries, complete preflight, immutable collections, network-free behavior,
  and scope review pass.

Restricted VSTest reproduced the known localhost-bind denial; the identical focused commands and
full quality spine passed host-local. The most recent known hosted jobs still had `steps: []` under
the Actions account-budget condition. Treat any unchanged empty-step jobs as infrastructure rather
than test evidence and do not retry or dispatch them.

## Exact next bounded slice

Take **Phase 7w: complete participation-consideration result round**.

Acceptance boundary:

1. Add one immutable bounded Domain round result retaining a copied read-only sequence of the exact
   consideration requests and one exact accepted result per request in the same order.
2. Require exact request-instance alignment, one result for every request, at most 32 entries,
   distinct scheduled character identifiers, and one shared exact heard dialogue event.
3. Add focused Domain tests for empty, single, and maximum rounds; stable order; exact versus
   distinct value-equal request identity; missing, extra, reordered, duplicate-character, and
   mixed-event rejection; nulls; unchanged inputs; immutability; and smallest public surface.
4. Do not define/invoke a model or provider interface, assemble context, serialize/deserialize,
   retry or time out, mutate a budget, select/start/complete a turn, loop, or wire the scene.

Leave both Phase 7 parent items unticked until later slices obtain one decision per scheduled
listener and drive every ordinary autonomous chain through the existing bounded scene seams.
