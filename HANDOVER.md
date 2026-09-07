# Allrpcs Handover

## Current state

PR #134 merged Phase 2o29 into `main` at `d71f135`. The branch
`codex/phase-2o30-plus-world-events` implements Phases 2o30–2o31.

The temporary source retains `greybox-ready`/`no-advance` first and
`herb-patch-identified`/`advance` second, with exact references for startup and herb identification.
Startup still records only `greybox-ready`. Eligible herb-patch focused-search resolution preflights
both controllers, then consumes the outcome, changes perception, records the exact authored event,
and advances qualitative time. Ineligible event occurrence preserves both controller states.

After code review, `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 1,020 Domain tests, 107 Application tests, six architecture tests, all 27 opt-in Godot
smokes, and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 2o32: ordinary input-path proof for the first meaningful-time event**.

Acceptance boundary:

1. Drive the existing start, complete, and resolve input actions against the authored herb patch in
   one opt-in Godot smoke; do not add another production path.
2. Prove no event occurs before successful resolution, then prove exact outcome consumption,
   perception identification, event recording, time advancement, and existing presentation.
3. Prove a repeated or ineligible sequence preserves focused-search and world-event state.
4. Update evidence, run `./scripts/quality.sh`, review intended paths, and prune this file again.

Explicit exclusions: no new event definitions, generic event mappings, payloads, clocks, disease
effects, save/load, replay, providers, credentials, or UI. Do not merge, release, distribute, sign,
notarize, or change repository visibility without explicit authorization.
