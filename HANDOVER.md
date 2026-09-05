# Allrpcs Handover

## Current state

Phase 2k39 is implemented and reviewed without findings on
`codex/phase-2k39-completion-outcome-eligibility`, based directly on PR #86's verified merge
commit `8047478`. Slice commit `296e764` is pushed, and draft PR #87
(`https://github.com/hherb/allrpcs/pull/87`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome` is one
pure static operation accepting an existing outcome, activity snapshot, and outcome snapshot. It
rejects every null input before delegation, delegates active-request eligibility to
`FocusedSearchCompletionPolicy`, delegates duplicate-request rejection to
`FocusedSearchOutcomeRecordingPolicy`, and returns true only when both decisions succeed. Every
input and retained instance remains unchanged.

The 10 focused tests failed first with `CS0103`/`CS0246` while the policy was absent, then passed
after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning build, 620
Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and the bounded
graphical launch. The sandbox-only focused test attempt was inconclusive after the build because
VSTest could not use its required local socket facilities. No runtime Godot behavior changed, so
no separate manual visual acceptance was performed. Hosted desktop outputs remain unsigned
development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k40: immutable focused-search completion-with-outcome result**.

Acceptance boundary:

1. Add one sealed immutable
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionResult` record accepting an existing
   `FocusedSearchActivitySnapshot` and `FocusedSearchOutcomeSnapshot`.
2. Retain those exact supplied instances through get-only `Activities` and `Outcomes` properties,
   reject either null input, and let record equality use the retained snapshot instances. Do not
   copy, inspect, validate, or change either snapshot or any retained entry.
3. Add focused Domain tests for exact instance retention, equality using the retained snapshot
   instances, null rejection, and the intended sealed immutable two-property surface.
4. Document Phase 2k40 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add the combined completion transition, remove an activity, record or produce an outcome,
  change either snapshot, connect outcome completion to the Godot controller, resolve an outcome
  automatically, grant discovery, or add new Godot/Application behavior.
- Do not add outcome replacement, visibility ranking, complete-character inspection,
  ownership/capacity, world state, location, coordinates, range policy, passive noticing,
  reinterpretation, serialization, persistence, providers, plugins, credentials, or final
  interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
