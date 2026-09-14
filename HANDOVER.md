# Allrpcs Handover

## Current state

`main` contains merged PR #234 at `8177676`. The current branch is
`codex/phase-7-sabotage-model-binding` and advances three bounded Phase 7 slices:

- Phase 7ec binds one exact OpenAI concealment-decision model to its canonical
  `CharacterModelSettings` and composes it through a caller-owned factory with default or custom
  validated endpoints;
- Phase 7ed extends the temporary concealment-model source with exact configured binding
  initialization, compare-and-swap replacement and clearing, preserved one-shot precedence, and
  stable fail-closed offline fallback; and
- Phase 7ee composes, replaces, and clears the concealment binding with the existing selected-model
  settings flow: OpenAI activates it, an OpenAI setting change replaces it, and Ollama clears it.

Draft PR #235 is open from implementation commit `a4de97a`. Hosted run `34795879224`
completed with all five jobs at `steps: []`; it is Actions/account infrastructure state, not
product-test or unsigned-build evidence, and should not be retried unchanged.

Binding, factory, source, and settings-composition construction perform no credential or network
input/output and invoke no model. Nothing starts concealment automatically, chooses timing,
presents a completed model-selected concealment on the water-cask body, persists sabotage, adds
UI/input/events/time/transcripts/movement, adds another provider, or adds another sabotage effect.

## Validation and review

- Every slice began with the expected compile or smoke failure for its absent operation or wiring.
- Focused Application tests pass 3 binding cases; focused Infrastructure tests pass 5 factory
  cases; the focused game build passes with zero warnings; and isolated Godot smokes pass exact
  configured-source semantics and OpenAI activation/replacement plus Ollama clearing.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,306 Domain tests, 487 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters. No
  live-conformance switch was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7ef: present one exact completed model-selected concealment in the main scene**.

Acceptance boundary:

1. Add one scene coordinator that observes a completed concealment-decision task result once and
   presents only its exact selected concealment transition through the existing
   `CharacterObjectSabotageConcealmentPresentationController` compare-and-swap seam.
2. Treat accepted abstention as an exact no-op; failed, cancelled, stale, repeated, null, and
   non-selected terminal states must not change visibility, concealment, evidence, task state, or
   unrelated scene state.
3. Wire the coordinator after the existing state, task, and presentation nodes and prove the exact
   water-cask body hides only after an explicitly started selected result; do not start a model or
   concealment during scene startup.
4. Do not add timing or automatic triggers, new input/UI, persistence, events/time/transcripts,
   movement, another provider, another sabotage effect, or provider calls in standard validation.
