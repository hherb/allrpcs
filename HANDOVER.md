# Allrpcs Handover

## Current state

`main` contains merged PR #250 at `278da6c`. The current branch is
`codex/phase-7-substitute-sabotage` and advances four bounded Phase 7 Domain slices:

- Phase 7gc binds one exact substitute-sabotage occurrence to one exact authored replacement with
  a distinct world-object identifier;
- Phase 7gd binds that substitution to discoverable evidence only when both retain the same exact
  occurrence;
- Phase 7ge retains at most 64 exact active substitutions in stable caller order with value-unique
  target identifiers; and
- Phase 7gf applies an eligible exact substitution as a fresh overlay together with its exact fresh
  evidence, leaving the authoritative world-object snapshot and every source object unchanged.

Implementation commit `f6ec34c` is published in mergeable draft PR #251. Hosted run `34902576897`
was queued across all five credential-free jobs at the initial post-publication query; recheck its
current state rather than treating that queued snapshot as a result.

The implementation is pure Domain state and policy. It does not infer inventory, current placement,
physical similarity, plausibility, paths, reachability, critical-route safety, or replacement
eligibility. It does not replace a world object or scene body, recover or present a substitution,
choose sabotage or timing, persist state, or add Application, Infrastructure, scene, input, UI,
event, time, transcript, navigation, model, or provider behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing substitution, candidate, snapshot, policy, transition, and
  result compile failures.
- The focused host-local substitution suite passes 19 Domain cases. The first sandboxed VSTest run
  was blocked by local IPC `SocketException (13)`; the identical host-local test passed, and product
  code was not changed around the restriction.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,426 Domain tests, 515 Application tests, 682 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes, and manual review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7gg: require exact active substitution and visible evidence for recovery**.

Acceptance boundary:

1. Add one pure Domain policy that accepts one character evidence view, one current substitution
   snapshot, one authoritative evidence snapshot, and one exact substitution candidate.
2. Permit recovery only when the candidate's exact substitution instance remains in the current
   snapshot, its exact evidence definition remains in authoritative evidence, and that evidence
   identifier is visible in the supplied character view.
3. Reject nulls, absent or distinct value-equal substitution state, absent or distinct value-equal
   authoritative evidence, and unseen evidence while leaving every input unchanged; expose only one
   closed static query.
4. Do not remove a substitution, consume evidence, restore or replace a world object or body, infer
   inventory, placement, similarity, plausibility, route safety, perception, or discovery, or add
   transition, controller, source, selection, scheduling, model, provider, Application,
   Infrastructure, scene, persistence, input, UI, event, time, transcript, navigation, or
   presentation behavior.
