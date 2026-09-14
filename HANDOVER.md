# Allrpcs Handover

## Current state

`main` contains merged PR #237 at `d6d145d`. The current branch is
`codex/phase-7-incompatible-belief-goal-fixture` and advances one bounded Phase 7 slice:

- Phase 7eh adds `CharacterBeliefGoalIncompatibility`, an immutable Domain declaration binding two
  value-distinct exact characters to one exact belief and goal retained by each character plus one
  explicit non-blank rationale of at most 500 UTF-16 code units.
- `RepresentativeSageEnemyAgentIncompatibilitySource` composes one fresh declaration from the
  existing representative roles. Only the fixture characters' belief and goal sets are extended;
  their exact knowledge, capabilities, limitations, relationships, controller assignments, and
  public/private backgrounds are retained.

The declaration rejects nulls, same-character values, and value-equal replacement beliefs or goals.
It does not infer incompatibility from prose or keys, evaluate or resolve the conflict, change
allegiance, schedule dialogue or sabotage, call a provider, or add scene, persistence, input, UI,
event, time, transcript, movement, or sabotage-effect behavior.

## Validation and review

- The tests began with the expected `CS0246` compile failure for the absent declaration. Seven new
  contract/source tests and five adjacent representative-role tests pass.
- The first full run exposed six older dialogue tests whose single-goal assumption was broken by
  extending the global sage role. Review moved all new authored state into the fresh fixture only;
  the unchanged global representative roles and their complete dialogue regressions then passed.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,313 Domain tests, 487 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- Changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. No live-conformance switch was enabled and no provider was invoked.
- Unrelated untracked `scripts/code_statistics.py` appeared during the work and remains untouched and
  unstaged.

## Exact next bounded slice

Take **Phase 7ei: project the explicit incompatibility into exact per-character perspectives**.

Acceptance boundary:

1. Define one immutable Domain perspective that retains the exact incompatibility declaration, one
   exact subject character and its declared belief/goal, and the exact counterpart character and its
   declared belief/goal. Accept only the declaration's exact first or second character reference;
   reject value-equal replacements without inferring a side from prose or keys.
2. Project the representative fixture into exactly two perspectives in stable declaration order,
   retaining every exact nested reference without copying or mutating either character.
3. Prove null/replacement rejection, both exact orientations, bounded two-entry output, stable order,
   exact-instance retention, fresh independent fixture composition, and no mutation.
4. Do not evaluate a response, decide which position is correct, resolve the conflict, change
   allegiance, schedule dialogue or sabotage, call a provider, or add Application, Infrastructure,
   scene, persistence, input, UI, event, time, transcript, movement, or sabotage-effect behavior.
