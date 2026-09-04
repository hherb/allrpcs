# Allrpcs Handover

## Current state

Phase 2k23 is implemented and reviewed without findings on
`codex/phase-2k23-focused-search-result-presentation`, based directly on PR #70's merge commit
`e69ba0c`. Push and draft-PR evidence are not yet recorded here. Pull-request merge remains
separately unauthorized.

The temporary panel now includes one focused-search result label, initialized deterministically
to `Focused search: NO ATTEMPT`. A pressed physical F, C, or E updates it with START, CANCEL, or
COMPLETE plus CHANGED or UNCHANGED, using only the existing Boolean lifecycle result. It never
adds or infers a reason. Released focused-search actions and unrelated, movement, or healer input
preserve the exact prior text. Existing handled-input behavior, lifecycle transitions, snapshot
identity, exact retained-request forwarding, focus independence, context instances, and activity
indicator presentation remain unchanged.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six Godot smokes, and the bounded graphical launch.
The focused smoke failed test-first on the deliberately absent result node, then passed for the
initial text and successful/unchanged results from all three actions across both eligible targets.
It also passed exact prior-text preservation for released, unrelated, movement, and healer inputs
while retaining all earlier lifecycle, focus, context, request-instance, state-identity, and
indicator assertions. The graphical launch passed, but no separate manual visual acceptance was
performed. The first sandboxed engine attempt reproduced the known unavailable `user://logs`
failure; the host smoke and complete host quality spine passed. Hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k24: immutable focused-search outcome value**.

Acceptance boundary:

1. Add one sealed immutable `Allrpcs.Domain.Discovery.FocusedSearchOutcome` record that retains
   exactly one supplied `FocusedSearchRequest` and one supplied existing
   `Allrpcs.Domain.Perception.VisibilityState` as `Request` and `ResultingVisibilityState`.
2. Reject either null input. Preserve exact supplied instances and record value equality; do not
   validate, rank, advance, or otherwise interpret the visibility state.
3. Add focused Domain tests for exact instance retention, equal and distinct values, null inputs,
   and the intended get-only sealed API surface.
4. Do not create an outcome collection, apply the outcome to perception, connect it to completion,
   or alter the existing focused-search controller, temporary inputs, activity ring, or result
   label.
5. Document Phase 2k24 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add outcome production or application, discovery grants, visibility mutation or
  advancement policy, events, effects, reason/error taxonomies, progress, duration, automatic
  lifecycle behavior, authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
