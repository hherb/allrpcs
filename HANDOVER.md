# Allrpcs Handover

## Current state

`main` includes merged PR #358 at `0f0585f`. The current branch advances the exact fifth Move cycle
through completion and pure scheduling admission after its terminal route reset. GitHub still shows
the older draft PR #357 open; do not treat it as the branch carrying this work.

The new completion coordinator accepts only the exact fifth-cycle reset author request, canonical
`Move` kind, actor, and still-current recovered placement/evidence outputs. It retains one fresh
`CharacterSabotageTurnCompletion`. A separate admission coordinator revalidates that entire graph
and accepts only the exact completion through the existing pure scheduling gate.

Null, rebuilt-completion, rebuilt-author, foreign-actor, wrong-kind, substituted-snapshot,
stale-graph, and repeated inputs remain inert. No ordinary turn is bound or reserved, no sixth
sabotage cycle is started, no prior provenance is cleared, and no extra model is invoked. Ordinary
startup remains inert. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

## Validation and review

- Both slices were introduced test-first. Each focused game build failed only with `CS0246` for its
  absent coordinator, then passed with zero warnings after the minimum implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8, including
  rejected substitute tokens, exact one-shot completion and admission, preserved recovered
  authority, and all seven model-call counts remaining one. The sandbox cannot open `user://logs`;
  the identical host-local command passes.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- Complete-diff review found no remaining exact-provenance, stale-authority, one-shot, scene-ordering,
  public-surface, formatting, line-length, documentation, or whitespace issues.
- Hosted checks may fail with `steps: []` under the current Actions/account infrastructure state;
  such jobs are not product-test evidence.

## Exact next bounded slice

Bind only the exact admitted fifth-cycle completion to the still-empty ordinary-turn reference,
stopping before scheduling authorization, turn reservation, or a sixth sabotage cycle.

Acceptance boundary:

1. Test-first add one ordinary-turn boundary coordinator over only the exact current fifth-cycle
   admission and the current empty `CharacterTurnActivityController` reference.
2. Revalidate the complete completion/admission provenance graph and use the existing pure ordinary
   turn boundary eligibility policy.
3. Reject null or rebuilt admission, a substituted non-empty activity, stale authority, stale graph,
   and repeated binding.
4. Retain the exact admitted completion and exact null activity reference without starting,
   reserving, replacing, or cancelling any ordinary turn.
5. Do not authorize scheduling or begin a sixth sabotage cycle.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every model-call
   count unchanged.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to retain one exact scheduling authorization from only that still-current boundary.
