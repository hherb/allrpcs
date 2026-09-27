# Allrpcs Handover

## Current state

Phase 8 is complete on `main`. The authored opening region, background variants, journal, map,
investigation routes, first-patient recovery, sage consultation, and schema-8 persistence have
automated and graphical evidence. On 2026-09-27, a human tester used the ordinary graphical game,
saved, fully closed and reopened it, loaded, and confirmed that the expected state restored
correctly. After the acceptance follow-up, the tester explicitly directed that Phase 8 be marked
complete.

## Current evidence and boundaries

- The final human checkpoint verified the ordinary save/full-close/reopen/load path. Detailed route,
  false-start, and qualitative usability observations were not recorded, so do not present it as a
  granular usability study.
- Earlier Godot 4.6.3 Mono Compatibility evidence covers route completion, remedy readiness,
  testimony, sage consultation, `SAVED`, `LOADED`, and the restored journal.
- Host-local `./scripts/quality.sh` passed a zero-warning build, 2,839 Domain, 924 Application,
  1,261 Infrastructure, and 8 architecture tests, the scenario validator, every credential-free
  Godot smoke, and a graphical launch. Restricted Godot still hits the known `user://logs` boundary.
- Persistence remains visibility-safe: it stores canonical identities, not coordinates, topology,
  hidden authoritative state, or provider transcripts.
- Autosave, multiple slots, file selection, provider/settings persistence, credentials, provider
  calls, production art/audio, release, and signing remain out of scope.

## Exact next bounded slice

Create several preview-only visual directions for the same small representative scene containing
terrain, one building, two characters, plants, one interactable object, and dialogue UI. Keep the
existing greybox scale and camera framing visible enough to compare perspective, base resolution,
palette, lighting, texture, outlines, atmosphere, character proportions, and non-colour-only object
legibility. Record source files, prompts where applicable, provenance, and a concise comparison.

Do not import previews into Godot, replace greybox/runtime assets, modify gameplay or UI behavior,
produce a final asset set, select the final direction without review, use untracked third-party
assets, or begin production art, audio, release, or signing work.
