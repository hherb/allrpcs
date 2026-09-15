# Allrpcs Handover

## Current state

`main` is verified at merged PR #256 (`bd08f6b`). Branch
`codex/phase-7-substitution-configured-source` completes the configured substitution-decision
scene chain through four bounded slices:

- the existing temporary source owns one exact configured
  `CharacterObjectSabotageSubstitutionModelBinding` through exact-reference initialization,
  replacement, and clearing while preserving one-shot precedence and one stable offline fallback;
- the selected-settings composition controller activates and replaces that exact future OpenAI
  substitution model, and clears it back to offline behavior for Ollama or unsupported providers,
  without construction-time credential access, HTTP send, or model invocation;
- one main-scene coordinator observes each fresh completed substitution result at most once and
  presents only an exact selected result through the existing stale-safe body projection; and
- explicit acknowledgement resets only an already-observed completed result whose selected
  presentation or abstention request snapshots remain exact current scene state.

Implementation commit `e7b9b13` is published in mergeable draft PR #257. Hosted run
`34939423963` failed before executing any steps in all five credential-free jobs (`steps: []`), so
it is Actions/account infrastructure state rather than product-test evidence and was not retried.

No provider was invoked. The work adds no automatic model invocation, sabotage selection, timing,
scheduling, persistence, input, UI, events, time, transcripts, navigation, actor movement, or new
sabotage effect. Authoritative world objects and every supplied request, candidate, substitution,
evidence, and scene snapshot remain unchanged. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing source operations, then the expected focused composition
  failure, then the expected missing presentation-coordinator compile failure.
- Focused host-local Godot smokes pass configured/one-shot/offline model-source ownership,
  selected-settings composition and clearing, and exact main-scene selection, one-time
  presentation, stale-safe acknowledgement, abstention, and offline failure.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,455 Domain tests, 524 Application tests, 757 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Every changed authored C# file remains below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. The implementation mirrors the established concealment and move
  contracts; complete changed-file review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7hf: define bounded immutable world-object contamination state**.

Acceptance boundary:

1. Add one Domain `WorldObjectContaminationSnapshot` that defensively retains at most 64 exact
   contaminated `ObjectId` instances in stable caller order and exposes a pure value-based
   `IsContaminated(ObjectId)` lookup.
2. Reject null input, null entries, value-equal duplicate identifiers, and excess entries before
   retaining state; expose only the bounded read-only entries and lookup surface.
3. Prove exact identifier-instance retention, defensive collection copying, stable order, absent
   lookup, null rejection, boundary counts, unchanged caller input, and a closed get-only surface.
4. Do not reuse or mutate ingredient contamination; define a contamination candidate, policy,
   transition, evidence application, recovery, consideration/model/provider path, scene ownership,
   persistence, input, UI, timing, scheduling, events, time, transcripts, navigation, presentation,
   or actor movement.
