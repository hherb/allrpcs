# Allrpcs Handover

## Current state

`main` includes merged PR #370 at `6e9dba3`. Branch
`codex/phase-7-reserved-sabotage-lifecycle` advances the genuine Phase 7 ordinary-play acceptance path
without extending the closed repeated Move-cycle chain. Draft PR #371 carries this branch.

The exact reserved ordinary-play path now binds and acknowledges its generic Delay selection through
the existing author-provenance owner, composes and explicitly starts the typed request, applies and
acknowledges its exact result, and binds the fresh authoritative event-log and world-time outcome. It
then publishes the exact recovery candidate and eligibility token, composes the provider-neutral
recovery request, and explicitly starts it. All three queued models are one-shot, the shared reservation
remains current, and normal dialogue cannot overlap it.

The path stops before observing or acknowledging recovery. It has not retained or reset a terminal
route, completed the reserved sabotage turn, released the shared slot, or added player-visible
investigation. The older switch-gated proof covers those reusable downstream owners. Do not add a sixth
or later Move chain.

## Validation and review

- Test-first focused builds failed on each absent reserved overload before implementation. The focused
  host-local main-scene smoke now proves exact reservation retention, generic binding and acknowledgement,
  typed Delay composition/start/application/acknowledgement, outcome binding, recovery-candidate and
  eligibility publication, recovery-request composition, and explicit recovery start. Null, rebuilt,
  stale, and repeated inputs stay inert without extra model calls or slot release.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,780 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests, representative
  scenario validation, every credential-free Godot smoke, and the Compatibility launch.
- The sandboxed quality run reached a zero-warning build, then hit the known VSTest loopback
  `SocketException (13)`; the identical host-local run passed completely.
- Complete-diff review added an exact one-call recovery-model assertion and split smoke-only models so
  every changed C# file remains below 500 lines. No automatic lifecycle progression, fabricated
  dialogue, sixth Move chain, provider/persistence/UI work, whitespace error, or release claim remains.
- Draft PR #371 targets `main`. Hosted run `36115308443` failed all five jobs with `steps: []`; no
  workflow step executed, so this is Actions/account infrastructure rather than product-test evidence.

## Exact next bounded slice

Retain and acknowledge only the exact completed reserved ordinary-play Delay recovery through the
existing recovery-result owner.

Acceptance boundary:

1. Retain the exact reservation, author request, recovery request, task-installed result, applied recovery,
   and fresh authoritative event-log and world-time snapshots.
2. Reuse the existing recovery observation, resolution, and acknowledgement owners; reset only the
   completed recovery task.
3. Reject null, rebuilt reservation/request/result/candidate, stale provenance or authority, and repeated
   input without consuming another model or releasing the slot.
4. Stop before terminal-route retention or reset, turn completion, or reservation release.
5. Add no sixth or later Move-cycle seam and no provider, persistence, UI, presentation, or investigation
   work.
6. Keep ordinary startup inert without an eligible meaningful event; update acceptance evidence only for
   behavior actually proved.
7. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next acceptance slice.
