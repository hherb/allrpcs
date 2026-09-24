# Allrpcs Handover

## Current state

`main` includes merged PR #356 at `477a3a9`. The current branch advances the exact fifth Move cycle
through terminal-route retention/reset, completed-turn exposure, and scheduling admission.

The new path:

- retains only the exact fifth-cycle generic author request, canonical `Move` kind, and recovery
  coordinator's exact placement/evidence outputs after revalidating the complete acknowledged
  binding and current authoritative graph;
- resets only that exact retained route once while preserving recovery output and authoritative
  state;
- exposes one fresh completed-turn token only from that exact reset route; and
- admits only that exact token once through the existing pure scheduling eligibility policy.

Null, rebuilt-author, wrong-kind, substituted-authority, stale-graph, foreign/rebuilt completion,
and repeated inputs remain inert. No ordinary-turn boundary is bound or reserved, no scheduling
authorization is retained, no sixth cycle begins, and no extra model is invoked. No cadence,
provider transport, credentials, persistence, UI/input, navigation, perception, dialogue, timer, or
new Domain behavior was added. Ordinary startup remains inert. The unrelated untracked
`scripts/code_statistics.py` in the primary checkout remains untouched and must stay unstaged.

## Validation and review

- Each coordinator was introduced test-first. Focused game builds failed with `CS0246` for the
  absent terminal-route, turn-completion, and turn-admission coordinators before their minimum
  implementations were added.
- The focused switch-gated Move recovery proof passes host-local with Godot 4.6.3 Mono and .NET 8,
  including exact route retention/reset, completion, and admission while all seven model-call
  counts remain one and the ordinary turn stays unchanged. The sandbox cannot open `user://logs`
  and crashes before startup; the identical host-local command passes.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch. The sandboxed run reached the zero-warning build
  but VSTest could not bind its loopback socket (`SocketException (13): Permission denied`); the
  identical host-local run passed.
- Complete-diff review found one missing foreign-actor terminal-retention regression case; it was
  added and the focused proof plus full quality suite were rerun. No remaining provenance,
  stale-authority, route/reset, completion/admission, scene-ordering, public-surface, formatting,
  line-length, documentation, or whitespace issues were found.
- Hosted checks may fail with `steps: []` under the current Actions/account infrastructure state;
  such jobs are not product-test evidence.

## Exact next bounded slice

Bind the exact admitted fifth-cycle completion to the still-empty ordinary-turn reference without
reserving or replacing the turn.

Acceptance boundary:

1. Test-first add one ordinary-turn boundary coordinator over only the exact fifth-cycle admission
   and current ordinary-turn reference.
2. Revalidate the admitted completion's complete terminal-reset, recovery, and authoritative-state
   graph.
3. Reject null, rebuilt/foreign completion, non-empty or changed ordinary-turn references, stale
   authority, stale graph, and repeated binding without changing any owner.
4. Retain only the exact admitted completion and exact still-empty ordinary-turn reference.
5. Do not reserve or replace the ordinary turn, retain scheduling authorization, enter another
   cycle boundary, invoke an author/model, or begin a sixth sabotage cycle.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model-call
   count unchanged.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to retain one fresh scheduling authorization only from that exact still-current boundary.
