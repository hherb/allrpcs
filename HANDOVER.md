# Allrpcs Handover

## Current state

`main` includes merged PR #361 at `803b77d`. Branch
`codex/phase-7-acceptance-audit` completes the Phase 7 acceptance traceability audit after the
deliberately final repeated Move-cycle proof.

`PHASE_7_ACCEPTANCE_AUDIT.md` maps all seven canonical kinds to their current Domain, Application,
Infrastructure, and Godot owners and evidence. It distinguishes reusable contracts and switch-gated
smokes from ordinary player-visible behavior. `ROADMAP.md` records the audit as complete, while the three
broad Phase 7 outcomes and final acceptance remain unchecked.

The audit found one shared first gap: `Main.tscn` owns the seven exact request authors and canonical
composer, but no ordinary runtime owner evaluates current enemy-agent turn authority, invokes the seven
authors, and composes their exact eligible outputs. Complete sabotage paths currently start only from
smoke environment variables or isolated test scenes.

The repeated Move boundary is closed. Do not add a sixth or later Move candidate, author, request, task,
result, recovery, terminal route, turn completion, scheduling authorization, cycle boundary, or admission.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- The audit cross-checks all seven canonical kinds against production owners and Domain, Application,
  Infrastructure, and Godot evidence without treating switch-gated smokes as ordinary-play acceptance.
- Truth, placement, history, evidence, and recovery claims were checked against the current transition and
  policy implementations. No broad acceptance checkbox was changed.
- Final host-local `./scripts/quality.sh` passes workflow validation, formatting, a zero-warning build,
  2,731 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests, scenario validation,
  every credential-free Godot smoke, and the Compatibility launch. The sandbox-only run reached the
  zero-warning build and then hit the known VSTest loopback `SocketException (13)`; the identical
  host-local command passed.
- Complete-diff review found no unsupported acceptance claim, missed canonical kind, mistaken
  ordinary-play classification, stale next-slice boundary, line-length issue, or whitespace error.
  `git diff --check` passes, and all changed Markdown lines remain within 120 characters.

## Exact next bounded slice

Add one provider-neutral, explicit, one-shot ordinary-turn sabotage authoring coordinator. It must accept
the exact current enemy-agent ordinary-turn token and current scene authority required by the seven
existing request authors, invoke those authors in canonical order, and compose only their exact current
outputs through `CharacterSabotageActionRequestComposer`.

Acceptance boundary:

1. Start test-first with a focused switch-gated Godot smoke that proves both eligible and ineligible
   canonical kinds from one exact current enemy-agent turn.
2. Retain and require the exact ordinary-turn token and exact actor. Reject null, rebuilt, foreign,
   replaced, stale, repeated, and already-composed graphs without partial publication.
3. Reuse the existing seven `CharacterSabotageAction*RequestController` owners and their current scene
   sources/states. Do not duplicate typed eligibility policy or construct generic requests directly.
4. Invoke authors in canonical order: Conceal, Move, Substitute, Contaminate, Misquote, Delay, Influence.
   Pass their exact current outputs to `CharacterSabotageActionRequestComposer` only after complete
   preflight proves the graph is current.
5. Preserve ineligible kinds as absent author outputs. Require at least one eligible exact output; an empty
   set stays inert.
6. Retain only the exact successfully composed request and turn provenance needed for the next slice.
   Keep the public surface closed to scene-owned implementation tokens where practical.
7. Wire the coordinator inertly into `Main.tscn`; ordinary startup must perform no authoring, model work,
   state mutation, turn reservation, or presentation.
8. Stop before supplying a model, starting generic or typed consideration, selecting or applying an
   outcome, recovering it, completing a turn, scheduling another action, or adding cadence.
9. Add no provider transport, credentials, persistence, UI/input, navigation, or unrelated Domain behavior.
10. Do not extend the repeated Move-cycle chain. The slice is acceptance orchestration across the existing
    seven-kind boundary, not another Move proof.
11. Update `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually added;
    do not check a broad acceptance item until ordinary player-visible behavior satisfies it.
12. Run `./scripts/quality.sh`, review the complete diff, preserve `scripts/code_statistics.py`, and leave
    one exact next acceptance slice.
