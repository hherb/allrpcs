# Allrpcs Handover

## Current state

Phase 8 implementation and repository-informed playtesting are complete on `main`. One acceptance
gate remains open in `ROADMAP.md`: a genuinely new player must complete the opening scenario without
developer intervention, objective arrows, a prescribed clue order, or unavailable character
knowledge.

The latest uninterrupted graphical checkpoint used ordinary mapped input and no character model.
It completed the Stream/laundry investigation route, prepared and administered the remedy, heard
the patient testimony, consulted the sage, saved, fully closed and reopened the game, and loaded the
same state. The schema-8 save retained stabilized care, `sage-consulted`, consumed route progress,
and patient-then-sage testimony; the reopened journal showed those two entries. This proves the flow
works, but its repository-informed operator does not satisfy the independent-new-player gate.

## Current evidence and boundaries

- Godot 4.6.3 Mono Compatibility screenshots cover route completion, remedy readiness, testimony,
  sage consultation, `SAVED`, `LOADED`, and the restored journal.
- Host-local `./scripts/quality.sh` passed a zero-warning build, 2,839 Domain, 924 Application,
  1,261 Infrastructure, and 8 architecture tests, the scenario validator, every credential-free
  Godot smoke, and a graphical launch. Restricted Godot still hits the known `user://logs` boundary.
- Persistence remains visibility-safe: it stores canonical identities, not coordinates, topology,
  hidden authoritative state, or provider transcripts.
- Autosave, multiple slots, file selection, provider/settings persistence, credentials, provider
  calls, production art/audio, Phase 9 implementation, release, and signing remain out of scope.

## Exact next bounded slice

Recruit one tester who has not used the repository or been taught the route. From a fresh graphical
start, ask only that they complete any investigation route, the recovery loop, and a save/full
close/reopen/load checkpoint using information visible in the game. Do not coach controls or route
order, reveal hidden state, edit state, or use developer controls. Record route choice, false
starts, help requested, clue/control discoverability, pacing, dialogue load, disease-pressure
interpretation, completion, and exact save/load outcomes.

If they finish without intervention or unavailable knowledge, check the final Phase 8 acceptance
item and hand over one Phase 9.1 preview-only art-direction slice. If they are blocked, implement
only the concrete observed blocker with focused switch-gated coverage plus `./scripts/quality.sh`,
then repeat the independent checkpoint.
