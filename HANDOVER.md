# Allrpcs Handover

## Current state

`main` includes merged PR #369 at `fa3b2c5`. Branch
`codex/phase-7-ordinary-sabotage-scheduling` advances the genuine Phase 7 ordinary-play acceptance
path without extending the closed repeated Move-cycle chain.

The scene now owns a bounded sabotage cadence measured only in exact authoritative
meaningful-world-event occurrences. At the due boundary it retains one inert opportunity for the
existing scene-owned enemy actor. An explicit continuation reserves the shared character-turn slot
without fabricating dialogue, invokes all seven existing authors, composes eligible Conceal, Delay, and
Influence outputs, starts the existing provider-neutral generic task, and observes its exact completed
selection once. Normal dialogue cannot overlap that explicit reservation. Ordinary startup remains
inert until an eligible event occurs and does not advance the incomplete chain beyond the opportunity.

The new ordinary path stops before binding or acknowledging that observed selection. It has not composed
typed Delay work, applied or recovered an action, released the reservation, or added player-visible
investigation. The older switch-gated proof still covers the complete Delay lifecycle. Do not add a
sixth or later Move chain.

## Validation and review

- Test-first focused Domain runs proved cadence, exact signal derivation, opportunity, and reservation
  policy/transition surfaces. The sandboxed VSTest run hit the known loopback `SocketException (13)`;
  identical host-local runs passed 24 opportunity tests and 8 reservation tests.
- The focused game build passed with zero warnings. Host-local main-scene smokes proved ordinary
  startup inertness, early-event rejection, exact due-event scheduling, exclusive shared-slot
  reservation, canonical authoring, one-shot model preservation, exact generic start, and one exact
  completed-result observation. The existing complete switch-gated Delay lifecycle and character-turn
  controller smokes also passed after the shared-slot change.
- Host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning build,
  2,780 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests, representative
  scenario validation, every credential-free Godot smoke, and the Compatibility launch.
- Complete-diff review removed automatic downstream reservation/authoring/start/observation from the
  incomplete ordinary path so offline failure or the deliberate post-observation stop cannot hold the
  shared turn slot indefinitely. Automatic ordinary behavior now ends at one inert opportunity; the
  focused proof invokes later seams explicitly. No fabricated dialogue, independent-task cadence reuse,
  sixth Move chain, provider/persistence/UI work, whitespace error, or changed C# file over 500 lines
  remains.

## Exact next bounded slice

Bind and acknowledge only the exact observed reserved ordinary-play generic selection through the
existing author-provenance owner.

Acceptance boundary:

1. Retain the exact reservation, selected author request, candidate binding, and observed result.
2. Reuse the existing selection/binding/acknowledgement owners; reset only the completed generic task.
3. Reject null, rebuilt reservation, opportunity, request, result, selected kind, stale author output,
   changed scene authority, and repeated input without consuming another model or releasing the slot.
4. Stop before composing or starting typed action work, applying an action, or releasing the reservation.
5. Add no sixth or later Move-cycle seam and no provider, persistence, UI, presentation, or investigation
   work.
6. Keep ordinary startup inert without an eligible meaningful event; update acceptance evidence only for
   behavior actually proved.
7. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next acceptance slice.
