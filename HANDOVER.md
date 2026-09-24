# Allrpcs Handover

## Current state

`main` includes merged PR #356 at `477a3a9`. The current branch advances the exact fifth Move cycle
through terminal-route retention and explicit reset.

The new coordinator accepts only the exact fifth-cycle generic author request, canonical `Move`
kind, and fresh recovered placement/evidence snapshots. It revalidates the task-owned author and
acknowledged result/candidate provenance plus the current recovery output, retains that route once,
then resets only that exact route once while preserving reset provenance and authoritative state.

Null, rebuilt-author, wrong-kind, substituted-snapshot, stale-graph, and repeated inputs remain
inert. No turn is completed or admitted, no ordinary turn is reserved, no sixth sabotage cycle is
started, no prior provenance is cleared, and no extra model is invoked. Ordinary startup remains
inert. The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay
unstaged.

## Validation and review

- The terminal route was introduced test-first: the focused game build failed only with `CS0246`
  for the absent coordinator, then passed with zero warnings after the minimum implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8, including
  rejected substitute tokens, exact retention, exact reset, preserved recovered authority, and all
  seven model-call counts remaining one. The sandbox cannot open `user://logs` and crashes before
  startup; the identical host-local command passes.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch. The sandboxed run reached the zero-warning build
  but VSTest could not bind its loopback socket (`SocketException (13): Permission denied`); the
  identical host-local run passed.
- Complete-diff review found no remaining provenance, stale-authority, reset-scope, scene-ordering,
  public-surface, formatting, line-length, documentation, or whitespace issues.
- Hosted checks may fail with `steps: []` under the current Actions/account infrastructure state;
  such jobs are not product-test evidence.

## Exact next bounded slice

Expose only the exact reset fifth-cycle terminal route as one completed-turn token, stopping before
scheduling admission, ordinary-turn binding, or a sixth sabotage cycle.

Acceptance boundary:

1. Test-first add one turn-completion coordinator over only the exact fifth-cycle reset author
   request, canonical `Move` kind, and still-current recovered placement/evidence outputs.
2. Revalidate the complete retained generic binding, acknowledgement, recovery, and reset-route
   provenance graph.
3. Reject null actor/request/kind/snapshots, rebuilt author request, foreign actor, wrong kind,
   substituted recovery snapshot, stale authority, stale graph, and repeated completion.
4. Retain one exact `CharacterSabotageTurnCompletion` without clearing any earlier owner or changing
   placement/evidence authority.
5. Do not evaluate or admit scheduling, bind or reserve the ordinary turn, or begin a sixth cycle.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model-call
   count unchanged.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to gate pure scheduling eligibility for only that exact completion token.
