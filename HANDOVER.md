# Allrpcs Handover

## Current state

`main` contains merged PR #216 at `a4e70da`. The current branch is
`codex/phase-7-whispering-hearing`.

This branch advances four bounded Phase 7 slices:

- Phase 7ay defines the closed provider-neutral `DialogueDeliveryMode` values
  `ordinary-speech` and `whispering`, plus immutable `DialogueDelivery` exact retention.
- Phase 7az adds `DialogueDeliveryHearingSelector`, which validates both named finite positive
  ranges and delegates stable bounded candidate filtering through the existing hearing selector.
- Phase 7ba-7bb make `DialogueEvent` and `DialogueEventController` retain one exact delivery and
  audience, migrate every existing event construction to explicit ordinary speech, and prove an
  exact whisper uses the shorter inclusive range.
- Phase 7bc adds a temporary explicit `WHISPER` panel toggle, retains the exact player-authored
  delivery in `DialogueInputController`, and carries that same delivery through ordinary scene
  occurrence. Existing utterance consumers continue through exact read-only projections.

The Phase 7 whisper/private-conversation roadmap parent remains open. Transcript entries,
character-model context, and version-3 saves still retain only utterance content, so they do not
yet preserve delivery mode. Character response speech remains explicitly ordinary.

This branch does not infer delivery, privacy, or direct address from text; define addressee
authorization, whisper occlusion, visibility, obstacles, acoustic materials, or eavesdropping;
change provider response schemas; persist delivery mode; add local participation dialects,
diagnostics, retries, independent tasks, party separation, off-screen simulation, sabotage, or
Phase 8 work; release; sign; or distribute.

## Validation and review

- The Phase 7ay tests were authored against absent delivery types and failed compilation before
  implementation; 14 focused Domain tests now prove both exact closed values, parsing, equality,
  null rejection, exact retention, and closed public surfaces.
- The Phase 7az tests failed compilation before the selector existed; nine focused Application
  tests now prove ordinary/whisper range selection, inclusive boundaries, stable exact identifiers,
  complete range preflight, and null rejection.
- The Phase 7ba event tests failed against the old utterance-only event and now prove exact delivery,
  utterance projection, audience retention, speaker validation, null rejection, and public surface.
- The Phase 7bb-7bc Godot checks failed compilation against the old controller/input signatures.
  Focused host-local dialogue input and event-controller smokes pass, including explicit ordinary
  and whisper input, exact delivery retention, shorter-range whisper hearing, invalid-query state
  preservation, and unchanged unrelated state.
- `./scripts/quality.sh` passes host-local: restore and formatting; zero-warning solution build;
  2,112 Domain, 435 Application, 522 Infrastructure, and 8 architecture tests; all 5 scenario
  definitions through `dotnet exec`; the complete headless Godot smoke matrix; and the bounded
  Compatibility-renderer launch.
- Restricted Godot reproduced the known `user://logs` crash. The identical focused and full checks
  passed host-local. No hosted workflow was retried or dispatched.
- Review found the 75-file breadth is the required mechanical `DialogueEvent` constructor migration:
  every existing event now wraps its exact utterance in explicit `ordinary-speech`; authored behavior
  changes remain confined to the new delivery/hearing contracts and player scene path.

## Exact next bounded slice

Take **Phase 7bd: delivery-aware spoken transcript entries**.

Acceptance boundary:

1. Change the spoken `CharacterTurnTranscriptEntry` path to retain one exact existing
   `DialogueDelivery`, while preserving `SpokenStatement` as an exact read-only projection of its
   utterance and leaving physical-action entries unchanged.
2. Migrate transcript construction and `CharacterTurnTranscriptController` to require explicit
   delivery, then carry the exact occurred player delivery and explicit ordinary character-response
   delivery into transcript append exactly once.
3. Add focused Domain tests and update the existing transcript scene smoke for exact mode/delivery
   retention, repeated equal occurrences, closed surface, and unchanged physical-action behavior.
4. Do not yet change recent-conversation save JSON, provider context/request schemas, response
   delivery selection, `DialogueAct`, event audience selection, UI beyond the existing toggle, or
   the Phase 7 roadmap parent; do not infer privacy or direct address from text.
