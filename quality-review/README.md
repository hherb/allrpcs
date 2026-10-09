# Current full quality

The owner requested full quality repair on 2026-10-09; the exact request is retained
in [owner-request.json](owner-request.json). This is check orchestration maintenance,
without new gameplay/audio/art authority or changes to the continuation queue.

Run `./quality.sh` after the dependency setup in the root README. The root entry
point exists because `scripts/quality.sh` is an exact hash-bound predecessor input.
It runs every one of its nine Python checks on the accepted Clearing tree
`ea63fcc46834e84ffab98aabd76530e60f760955`, using isolated Git indexes and temporary
archives. Each published audio boundary also runs at its own recorded implementation;
the five exact commits are listed in `quality.py`. Each archive has its own detached
Git HEAD and index, sharing only read-only objects; historical ledger comparisons cannot
see later live closures. The material publication validator
carries its already-authorized incidental Finder metadata exclusion forward, without
editing that historical verification or depending on unpublished `.DS_Store` files.

Current checks include continuation regressions, retained evidence hashes, exact
art imports, the current interior boundary, every original product command, all 293
retained Godot smoke launches, eight terrain checks, graphical Main, and all four
current interior modes with explicit completion and complete-save/transcript parity.
The current boundary runs again after runtime checks. Only newly generated valid
UID sidecars belonging to unchanged tracked GDScript sources are cleaned; existing
sidecars, edits, ignored caches and the recorded `game/project.godot` rewrite survive.

All attempts and log hashes are retained under ignored `artifacts/quality-current/`.
A nonzero command or incomplete fixture is fatal. An identical command may retry
only a native mutex shutdown abort, at most twice; all attempts remain recorded.
The report starts as running and is marked passed only after every stage succeeds.
`./quality.sh --self-test` exercises omitted-command rejection, failure reporting,
native retry restrictions, identical argv, incomplete fixtures, safe sidecar cleanup
and isolated historical HEADs with unchanged live Git state.

The completed 2026-10-09 run is recorded in [verification.json](verification.json),
with exact gzip archives of all 43 passing stage logs. It passed 5,173 unit tests,
all 293 retained smoke launches, eight terrain checks, graphical Main and four
fixed-60-FPS interior fixtures. Full-save bytes match SHA-256
`37bc768c19bbfaa21d680068bf08ed6136789a933bcd0f1eb17d632ba0be67e3` in every mode;
the final current boundary passed. The successful smoke attempt needed no retries.

Earlier sandbox permission failure, a native shutdown after a completed check, and
the initially omitted fixture `--fixed-fps 60` option remain in the separate failure
records and compressed logs. Run on the host where Godot can write its normal editor
settings and user logs. Retry classification examines only the failed launch, so
earlier successful launches' cleanup diagnostics cannot suppress a valid native retry.

The resumed run reused 26 verified stages after the native abort, then all 37 completed
stages after correcting the extra fixture invocation. Commands, product revision,
dependencies and log hashes were checked; a fresh current boundary ran before each
continuation. `--resume-smoke` and `--finish-interior` are restricted recovery modes for
those exact recorded failures, not general check-skipping options. Default
`./quality.sh` always runs the complete suite. Seven orchestration tests and eight
resume rejection probes pass. Frozen source/evidence and the local editor rewrite
remain unchanged; the next gameplay proposal stays queued in the continuation ledger.
