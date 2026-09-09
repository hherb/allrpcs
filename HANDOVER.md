# Allrpcs Handover

## Current state

`main` contains merged PR #151 at `5efc71d`. The branch
`codex/phase-3-examination-flow` completes Phase 3 through Phase 3n8:

- A pure exact-identity selector returns a fresh `safe-to-collect` result only for the explicitly
  supplied target of an observed `examine-object` occurrence.
- The scene authors exact advancing herb-patch examination and collection events. A focused
  coordinator delegates the existing Domain gates and installs event-log, qualitative-time, and
  transcript results atomically.
- The ordinary deterministic loop records healer speech, preserves an interruptible speaking
  frame, records examination, selects the exact safe result, and records collection only while the
  examination remains latest. Intervening speech or cancellation fails closed.
- The dialogue label reveals only already complete response speech, one Unicode text element per
  frame. Displayed prefixes have no response or action authority.

Phase 3's roadmap checklist and acceptance path are complete. Examination and collection remain
recorded occurrences: inventory, world objects, discovery, and movement do not mutate. The loop
still does not classify dialogue, infer targets or general safety, stream provider output, apply
state proposals, progress live commitments, add model providers, or persist dialogue state.

## Validation

Every new boundary began with an expected focused failure. The final host-local
`./scripts/quality.sh` passes workflow and formatting validation, a zero-warning build, 1,381
Domain tests, 151 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 51 opt-in Godot smokes, and the bounded
graphical launch.

The restricted sandbox still reproduces the known VSTest loopback denial and Godot `user://logs`
crash, so executable evidence must run host-local. The GitHub Actions budget is exhausted for the
month: do not dispatch hosted workflows, and treat zero-step hosted failures as account
infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4a1: define one provider-neutral streamed text delta**.

Acceptance boundary:

1. Add one immutable Application dialogue value representing a non-empty model text delta. Retain
   exact text, including whitespace, without trimming, concatenating, interpreting, or logging it.
2. Bound one delta to `DialogueUtterance.MaximumTextLength` UTF-16 code units and reject null, empty,
   and oversized input while permitting whitespace-only deltas needed between provider tokens.
3. Prove exact retention, boundary lengths, unchanged input, validation, and a narrow immutable
   public surface.
4. Do not define `ICharacterModel` yet, assemble context, call a provider, wire the value into
   Godot, execute partial intentions, change the completed-response label, add retry/timeout/error
   behavior, persistence, credentials, logging, or alter world/gameplay state.
