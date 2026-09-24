# Allrpcs Handover

## Current state

`main` includes merged PR #354 at `869b859`. The current branch advances the exact fifth Move cycle
through two bounded seams after its typed start: applied-result observation, then binding and
acknowledgement.

The new path:

- observes only the exact completed applied fifth-cycle typed result and the fresh authoritative
  placement and evidence snapshots installed by its task;
- revalidates the full retained fifth-cycle request/start graph and canonical exact fifth candidate;
- binds only those exact observed result/placement/evidence tokens; and
- explicitly acknowledges them, resetting only the completed typed task while retaining the exact
  result and authoritative snapshots.

Null, running, rebuilt-request, rebuilt-result, foreign-actor, wrong-candidate, stale-authority,
stale-graph, substituted-snapshot, and repeated inputs remain inert. No recovery candidate is
authored, movement recovered, turn reserved, provenance cleared, or extra model invoked. No cadence,
provider transport, credentials, persistence, UI/input, navigation, perception, dialogue, timer, or
new Domain behavior was added. Ordinary startup remains inert. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Applied-result observation was introduced test-first: the focused game build failed with `CS0246`
  for the absent coordinator, then passed after the minimum implementation was added.
- The focused switch-gated Move recovery proof passes host-local with Godot 4.6.3 Mono and .NET 8,
  including exact observation, binding, and acknowledgement while all model-call counts remain one.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch. The sandboxed run reached the zero-warning build
  but VSTest could not bind its loopback socket (`SocketException (13): Permission denied`); the
  identical host-local run passed.
- Complete-diff review found no remaining provenance, reset-scope, scene-ordering, public-surface,
  formatting, line-length, documentation, or whitespace issues.
- Hosted checks may fail with `steps: []` under the current Actions/account infrastructure state;
  such jobs are not product-test evidence.

## Exact next bounded slice

Author only one recovery candidate from the exact acknowledged fifth-cycle applied result and its
retained authoritative placement/evidence snapshots, stopping before recovery eligibility or
movement recovery.

Acceptance boundary:

1. Test-first add one distinct recovery-candidate coordinator over only the exact acknowledged
   result, exact fifth source-owned Move candidate, and retained authoritative snapshots.
2. Revalidate the complete retained acknowledgement, typed request, start, result, candidate, and
   authoritative-state graph.
3. Require the existing recovery-candidate Domain policy to accept the exact pair and require
   evidence visibility from the retained authoritative evidence snapshot.
4. Reject null, rebuilt-result, rebuilt-candidate, wrong-candidate, stale-authority, stale-graph,
   unseen-evidence, and repeated inputs without changing any owner.
5. Retain one exact recovery candidate and evidence view; do not admit recovery eligibility,
   recover movement, reset recovery authority, reserve the ordinary turn, or clear provenance.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model-call
   count unchanged.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to admit only that exact recovery candidate through the existing recovery policy.
