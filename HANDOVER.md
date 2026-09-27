# Allrpcs Handover

## Current state

`main` contains the Phase 8 opening-region playtest corrections found during the hands-on recovery
loop:

- P now reports station proximity, preparation start, every authored step, completion, and the ready
  batch. T accepts the authored 60-pixel sickhouse approach through its 64-pixel gate and reports
  missing batch, proximity, and successful administration.
- Patient testimony points to the sage; K reports missing testimony, proximity, and successful
  consultation. The previously invisible sage is now a labelled purple `VILLAGE SAGE · K` figure.
- The Seeker and following healer mutually ignore body collision, so the healer cannot pin the Seeker
  against the sickhouse walls. The existing 72-pixel follow stop remains unchanged.
- The large provider/settings and dialogue overlays start hidden behind physical Tab and release UI
  focus when hidden. The compact command, recovery, and save/load HUD remains visible.
- Explicit Q initializes the safe journal when map/route state already exists. If M has not initialized
  that state, Q now reports `PRESS M FIRST` and gives the exact M/M/Q recovery instruction instead of
  only `SAVE FAILED`. A successful save gives the close/reopen/L instruction.

## Validation and review

- Final host-local `./scripts/quality.sh` passed formatting, a zero-warning build, 2,839 Domain, 924
  Application, 1,261 Infrastructure, and 8 architecture tests, the five-evidence scenario validator,
  every credential-free Godot smoke, and a graphical Compatibility launch.
- Focused coverage proves the authored T approach, visible P/T/testimony/K guidance, visible sage,
  healer/Seeker pass-through with retained follow separation, default-hidden Tab overlays and focus
  release, explicit missing-M save guidance, safe journal initialization, exact save, and restoration.
- Session review checked the full diff, scene ownership and ready order, input mappings, collision and
  focus lifetime, save preflight, restoration status order, public surfaces, documentation, formatting,
  and whitespace. No known code issue remains in this slice.
- Restricted Godot execution still crashes at the known `user://logs` sandbox boundary; use only an
  identical authorized host-local retry.

## Direct manual evidence

- At full width, the old build reproduced silent P, an unreachable T range, healer pinning in the
  sickhouse, obscuring optional overlays, an invisible sage, and generic Q failure.
- With the corrections, the user completed preparation, administered the remedy, heard the patient
  testimony, left the sickhouse, found the labelled sage, consulted with K, and reached Q.
- That run's Q failed because the fresh process had not initialized map/route state with M. No
  `save-v1.json` existed when checked. Do not claim that Q, close/reopen/L, restored treatment state,
  or exactly-once restored journal entries have manual evidence yet.
- Earlier retained evidence covers ordinary Stream/laundry route completion and the keyboard-focused
  route acknowledgement, but not one uninterrupted route-through-recovery-and-reload run.

Keep the Phase 8 playtest and acceptance roadmap items open until the remaining direct evidence is
complete.

## Preserved boundaries

- There is no autosave, multiple slot, file selection, path/content display, settings/provider-state
  persistence, provider call, credential behavior, hidden disease exposure, conclusion inference,
  prescribed clue order, production art/audio, broad tuning, Phase 9 work, release, or signing.
- Route progress retains only canonical location/evidence identities. Map and journal persistence
  remains visibility-safe and exposes no coordinates, topology, authoritative hidden state, or
  provider transcript.

## Exact next bounded slice

Run one uninterrupted fresh graphical checkpoint on the current code. Complete one investigation
route, prepare with P, administer with T, hear testimony, leave the sickhouse, consult the labelled
sage with K, then press M twice to initialize and close the map before Q. Verify `SAVED` and the
close/reopen/L instruction, confirm `save-v1.json` exists, fully close, reopen, press L, and verify
`LOADED`, restored treatment/recovery state, and exactly one patient and sage journal entry.

Record route choices, dead ends, clue visibility, pacing, dialogue load, disease pressure, and the
checkpoint results. Fix only a concrete blocker with focused switch-gated coverage and the complete
quality spine. If the uninterrupted run passes, update the Phase 8 playtest and acceptance checkboxes
only to the extent the direct evidence supports, then leave one precise Phase 9 art-direction slice.
