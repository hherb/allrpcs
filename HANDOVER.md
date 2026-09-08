# Allrpcs Handover

## Current state

PR #135 merged Phases 2o30–2o31 into `main` at `a2bf227`. The branch
`codex/phase-2o32-plus-world-events` implements Phase 2o32 and completes the bounded 2o
world-event sequence.

The existing opt-in focused-search world-event smoke now drives ordinary start, complete, and
resolve input against the authored herb patch. Start and completion preserve the exact startup
event log and qualitative time. Successful resolution consumes the exact outcome, identifies and
presents the herb, records `herb-patch-identified`, and advances qualitative time. Repeated resolve
and a newly completed but ineligible second resolution preserve the exact focused-search,
world-event, world-time, and presentation snapshots.

After code review, `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 1,020 Domain tests, 107 Application tests, six architecture tests, all 27 opt-in Godot
smokes, and the bounded graphical launch. The restricted run hit the known VSTest loopback denial;
the identical host run produced the authoritative passing result.

## Exact next bounded slice

Take **Phase 2p1: immutable save-schema version vocabulary**.

Acceptance boundary:

1. Add one engine-independent immutable save-schema version value with an explicit first supported
   version, strict construction/parsing, stable equality, and a minimal public surface.
2. Prove valid round-trip behavior, invalid-value rejection, and unchanged inputs with focused
   tests. Keep the value independent of JSON, files, paths, Godot, and storage providers.
3. Document only the version contract and the intended later use as the root discriminator for
   versioned saves; do not define a save aggregate or serializer yet.
4. Update evidence, run `./scripts/quality.sh`, review intended paths, and prune this file again.

Explicit exclusions: no save aggregate, serialization, files, paths, migration, load orchestration,
autosave, replay, providers, credentials, or UI. Do not merge, release, distribute, sign, notarize,
or change repository visibility without explicit authorization.
