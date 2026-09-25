# Allrpcs Handover

## Current state

`main` includes merged PR #367 at `520c7f0`. Branch
`codex/phase-7-acceptance-terminal-route` continues the switch-gated Phase 7 acceptance path without
extending the closed repeated Move-cycle chain.

The ordinary-turn Delay path now preserves the exact generic acknowledgement, typed candidate, applied
Delay, recovery request/result, fresh event log and world time, and recovered Delay authority. Four new
adjacent coordinators revalidate that complete graph, delegate the existing terminal-route retention and
reset owners, expose one exact completed-turn token, and admit only that token through the existing pure
scheduling gate. Null, rebuilt, foreign, wrong-kind, replaced-authority, already-retained, and repeated
inputs remain inert. The switch-gated proof keeps the generic, typed Delay, and recovery model calls at
one each and stops before binding the still-awaiting character turn or scheduling another action.

Ordinary startup remains inert. Broad Phase 7 outcomes and final acceptance remain unchecked because no
ordinary scheduler invokes the path, the character-turn lifecycle has not explicitly resolved after the
sabotage action, and no player-visible investigation surface exposes Delay evidence.

Do not add a sixth or later Move candidate, author, request, task, result, recovery, terminal route, turn
completion, scheduling authorization, cycle boundary, or admission.

## Validation and review

- Each of the four new coordinators was introduced test-first: the focused game build failed with
  `CS0246` for the intentionally absent type, then passed zero-warning after implementation.
- The first sandboxed focused Godot run hit the known `user://logs` crash before scene load; identical
  host-local focused runs passed after terminal retention, terminal reset, completed-turn exposure, and
  scheduling admission were added.
- Sandboxed `./scripts/quality.sh` reached a zero-warning build and then hit the known VSTest loopback
  `SocketException (13)`; the identical host-local command passed workflow validation, formatting, a
  zero-warning build, 2,731 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and Compatibility launch.
- Complete-diff review found no partial retention/reset/admission, rebuilt-token acceptance, authority
  mismatch, extra model consumption, premature scheduling, acceptance overstatement, Move-chain
  extension, line-length issue, or whitespace error.

## Exact next bounded slice

Resolve only the lifecycle of the exact sabotage-owning character turn after its admitted completion.

Acceptance boundary:

1. Start test-first by extending the switch-gated ordinary-turn main-scene proof.
2. Prove the existing ordinary-turn boundary rejects the current exact `AwaitingResponse` activity.
3. Define the smallest explicit transition that produces a deliberately terminal successor for only
   that exact sabotage-owning turn after the exact admitted completion and recovered authority remain
   current.
4. Do not fabricate speech, reuse player cancellation semantics, replace the exact turn request, or
   mutate the recovered event log/world time.
5. Bind the exact admitted sabotage completion to the ordinary-turn boundary only after that exact
   terminal successor is current.
6. Reject null, rebuilt, foreign, stale, already-resolved, replaced-authority, and repeated inputs while
   preserving all retained owners and all three model-call counts at one.
7. Stop before scheduling authorization, cadence, another sabotage action, provider work, persistence,
   UI, or presentation; add no sixth or later Move-cycle seam.
8. Update `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually added;
   leave broad acceptance items unchecked.
9. Run `./scripts/quality.sh`, review the complete diff, and leave one exact next acceptance slice.
