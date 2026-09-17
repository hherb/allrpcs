# Allrpcs Handover

## Current state

`main` includes merged PR #296 at `b922ad6`. Branch
`codex/phase-7-influence-recovery-candidate-source` advances recovery input through two bounded
game slices:

- Phase 7lt adds `CharacterTargetSabotageInfluenceRecoveryCandidateController`, a one-shot scene
  owner that publishes one fresh candidate only from the influence controller's exact current
  applied-result and relationship authority and an accepted caller-authored inverse change.
- Phase 7lu dependency-orders that source after influence authority in `Main.tscn`. Ordinary
  startup stays inert; the existing switch-gated influence smoke alone creates one exact recovery
  candidate after explicit influence application and proves a later offline failure leaves it and
  the relationship authority unchanged.

The branch changes only the bounded controller, isolated and main-scene smokes, main-scene
dependency order, smoke script, README, ROADMAP, and this handover. It does not create a recovery
consideration request, start recovery work automatically, author evidence, persist relationships,
or add UI/input, elapsed time, navigation, movement, perception, dialogue, or other world behavior.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first builds failed only on the absent candidate controller, and the main-scene smoke failed
  only on the absent dependency-ordered node before their smallest implementations were added.
- Focused zero-warning builds, the isolated candidate-source smoke, and the switch-gated
  main-scene influence smoke pass host-locally.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, all Domain, Application, Infrastructure, and architecture tests, scenario
  validation, every credential-free Godot smoke, and the final Compatibility renderer launch.
- Session-wide review covers exact latest-result and relationship authority, one-shot publication,
  unchanged inverse-policy delegation, stale/rebuilt/misdirected/wrong-dimension/wrong-delta/null/
  repeated failure, inert startup, main-scene dependency order, unchanged source graphs, closed
  public surface, documentation, file size, line length, formatting, and whitespace. One smoke
  setup/reporting gap was fixed; no unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lv: retain one exact recovery consideration request from the current candidate**.

Acceptance boundary:

1. Test-first add one isolated scene controller that accepts the recovery-candidate source's exact
   current candidate and obtains its exact relationship-owning target character from the retained
   influence occurrence.
2. Publish one fresh `CharacterTargetSabotageInfluenceRecoveryConsiderationRequest` retaining that
   exact character, the influence result's exact current relationship, and only that exact
   candidate while unchanged Domain request and recovery policies accept the graph.
3. Reject unavailable, stale, value-equal rebuilt, foreign, repeated, and null input without
   replacing an already current request or changing the candidate, influence result, relationship,
   change, reason, occurrence, or character graph.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   this request source into `Main.tscn`, start recovery consideration automatically, author
   discoverable evidence, persist relationships, or add UI/input, timing, navigation, movement,
   perception, dialogue, or other world behavior.
