# Allrpcs Handover

## Current state

`main` contains the ordinary single-document save/load boundary needed to resume the fresh Phase 8
manual playtest:

- Physical Q and the visible `Q SAVE` button delegate only to
  `SaveDocumentStorageController.SaveCurrent()`. Physical L and the visible `L LOAD` button delegate
  only to `LoadCurrent()`.
- The panel exposes only `SAVE / LOAD READY`, `SAVED`, `SAVE FAILED`, `LOADED`, `NO SAVE FOUND`, or
  `LOAD FAILED`; it never exposes the fixed path or serialized content.
- Save still requires the existing complete schema-8 capture. Load still uses the existing strict
  deserialize/migration and complete atomic restoration path. Missing, malformed, incompatible, and
  failed documents do not change live state.
- Both storage smokes use a process-specific system-temporary file and delete it, so validation
  cannot overwrite the ordinary `user://save-v1.json` document.
- The top key guide now wraps at 960x540, lists M map, H healer move, and the P/T/K treatment flow,
  and leaves the Save/Load panel fully visible.

The existing automated continuity still covers schema-8 saves before preparation, with a complete
batch, after stabilization, after testimony, and after sage consultation. The first M-key map open
initializes route progress and routes retained stream-residue evidence; ordinary anchor visits record
exact verified locations.

## Validation and review

- Final host-local `./scripts/quality.sh` passed formatting, a zero-warning build, 2,839 Domain, 924
  Application, 1,261 Infrastructure, and 8 architecture tests, the five-evidence scenario validator,
  every credential-free Godot smoke, and graphical Compatibility launch.
- The new switch-gated main-scene smoke covers incomplete save, missing load, visible button paths,
  physical Q/L mappings, exact complete save, materially changed live state, successful load,
  malformed and future-version rejection, released/unrelated input, process-isolated storage, and
  unrelated scene-state isolation.
- A 960x540 two-frame graphical render confirmed that the wrapped instructions, both buttons, and
  bounded status are visible without clipping. A normal graphical Compatibility run also launched
  without runtime errors.
- Session review checked strict delegation, failure-state preservation, path/content non-exposure,
  input mapping, UI layout, smoke isolation, file sizes, line lengths, formatting, and the full diff.
  It found and fixed one stale interaction-focus assertion that still expected the old key guide. No
  known issue remains inside the save/load implementation.
- Restricted Godot execution still crashes at the known `user://logs` sandbox boundary; use only an
  identical authorized host-local retry.

## Manual evidence still required

- The preceding fresh profile was Aster, they/them, Travelling cloak, Apothecary's Assistant. Aster
  opened the empty map, crossed the stream, entered the laundry through its east opening, verified
  Stream and Abandoned Laundry, and reached the safe route-completion panel without developer state
  injection.
- This session had no native-app input surface, so it did not claim keyboard/button interaction or a
  real save/close/reopen checkpoint. Preparation, administration, testimony, sage consultation,
  duplicate-journal behavior, pacing, dialogue load, and disease pressure remain unproven manually.
- The route-completion panel's keyboard focus remains unproven. The customization panel also still
  says background route effects are inactive even though they are wired.
- Keep the Phase 8 playtest and acceptance roadmap items open until direct manual evidence satisfies
  them.

## Preserved boundaries

- There is no autosave, multiple slot, file selection, path/content display, settings/provider-state
  persistence, provider call, credential behavior, hidden disease exposure, conclusion inference,
  prescribed clue order, production art/audio, broad tuning, Phase 9 work, release, or signing.
- Route progress retains only canonical location/evidence identities. Map and journal persistence
  remains visibility-safe and exposes no coordinates, topology, authoritative hidden state, or
  provider transcript.

## Exact next bounded slice

Repeat the fresh graphical Phase 8 playtest using only ordinary controls. Complete one investigation
route, prepare and administer the remedy, hear the patient testimony, consult the sage, press Q to
save, fully close the game, reopen it, press L to load, and finish the loop. Stop before adding new
features unless a concrete playtest blocker prevents completion.

Record route choices, dead ends, clue visibility, pacing, dialogue load, disease pressure, Save/Load
status, restored treatment/recovery state, and whether safe journal/dialogue entries occur exactly
once. Check keyboard-only route-completion acknowledgement and the stale customization copy. If a
blocker is found, fix only that smallest ordinary-play boundary, add focused switch-gated coverage,
rerun the complete quality spine, and repeat the failed manual step. Mark Phase 8 playtest or
acceptance complete only if the direct evidence actually supports it.
