# Allrpcs Handover

## Current state

`main` includes merged PR #355 at `0666192`. The current branch advances the exact fifth Move cycle
through recovery-candidate authorship, exact eligibility admission, and stale-safe movement recovery.

The new path:

- authors one recovery candidate only from the exact acknowledged fifth-cycle applied result, its
  exact source-owned candidate, and its unique pre-move source placement;
- retains one exact caller-supplied evidence view only when the existing recovery policy accepts it
  against the retained authoritative placement and evidence snapshots;
- admits only that exact candidate/view pair; and
- delegates only that pair through the existing state owner, restoring the exact source placement
  in a fresh authoritative snapshot while retaining the exact evidence snapshot.

Null, rebuilt-result, rebuilt-candidate, substituted-snapshot, unseen-evidence, stale-graph, and
repeated inputs remain inert. No terminal route is retained or reset, turn completed or admitted,
ordinary turn reserved, provenance cleared, or extra model invoked. No cadence, provider transport,
credentials, persistence, UI/input, navigation, perception, dialogue, timer, or new Domain behavior
was added. Ordinary startup remains inert. The unrelated untracked `scripts/code_statistics.py`
remains untouched and must stay unstaged.

## Validation and review

- Recovery-candidate authorship was introduced test-first: the focused game build failed with
  `CS0246` for the absent coordinator, then passed after the minimum implementation was added.
- Eligibility admission and stale-safe recovery were each introduced with the same focused red/green
  pattern and distinct coordinators.
- The focused switch-gated Move recovery proof passes host-local with Godot 4.6.3 Mono and .NET 8,
  including exact candidate authorship, evidence admission, and restored movement while all seven
  model-call counts remain one. The sandbox cannot open `user://logs` and crashes before startup;
  the identical host-local command passes.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch. The sandboxed run reached the zero-warning build
  but VSTest could not bind its loopback socket (`SocketException (13): Permission denied`); the
  identical host-local run passed.
- Complete-diff review found no remaining provenance, stale-authority, recovery-scope,
  scene-ordering, public-surface, formatting, line-length, documentation, or whitespace issues.
- Hosted checks may fail with `steps: []` under the current Actions/account infrastructure state;
  such jobs are not product-test evidence.

## Exact next bounded slice

Retain the exact recovered fifth-cycle Move as one distinct terminal route and explicitly reset
only that route, stopping before turn completion or scheduling admission.

Acceptance boundary:

1. Test-first add one terminal-route coordinator over only the exact fifth-cycle generic author
   request, canonical Move kind, and recovery coordinator's exact placement/evidence outputs.
2. Revalidate the complete retained generic binding, applied-result acknowledgement, recovery
   candidate, eligibility, and current authoritative-state graph.
3. Reject null, rebuilt author request, wrong kind, substituted recovery snapshot, stale authority,
   stale graph, and repeated retention without changing any owner.
4. Retain one exact terminal route, then reset only that exact route once while preserving recovery
   output and authoritative placement/evidence state.
5. Do not expose completed-turn provenance, admit scheduling, bind or reserve the ordinary turn, or
   begin a sixth sabotage cycle.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model-call
   count unchanged.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to expose only the exact reset route as one completed-turn token.
