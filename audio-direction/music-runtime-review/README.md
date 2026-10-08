# Approved restrained music runtime

The owner's 2026-10-08 **approved** response authorizes the exact frozen
[music proposal](../music-runtime-scope.md). The resulting ordinary Main scene has
one optional non-positional Music player and the two unchanged accepted 25-second
M01/M02 WAVs. [Authority](../music-runtime-authority.json) and
[verification](../music-runtime-verification.json) bind the scope and evidence.

M01 follows a changed non-null regional bed, twelve seconds of stable dwell and
uninterrupted quiet, and 24 pixels of actual post-collision travel. M02 follows only
fresh exact committed treatment, after its eight-second action hold and twelve quiet
seconds. Opportunities expire at 45 seconds. Every actual finish/stop/start failure
requires 180 seconds of silence and a fresh later event. Reading, visible GUI focus,
search, generation, participation, dialogue and committed actions yield through a
0.6-second terminal fade. Mute/zero, load, discontinuity, missing owners/bus and focus
loss stop immediately. There is no loop, backlog, resume, initial cue or rest action.

The existing settings, ducking, captions/history, hearing and gameplay owners remain
unchanged. Music introduces no captions, saved events, perception or world-clock writes.
The Audio panel explains optional music and includes child reading views. Current
Yoiyami/gonzooka credits and the retained CC0 notice are included in all development
resource packages. The previous rejected M02 source is excluded.

## Verification

- 5,138 unit tests pass: Domain 2,839; Application 1,019 (21 new music cases);
  Infrastructure 1,272; Architecture 8. Solution/game builds have zero warnings/errors;
  both formatting checks, workflows and five ScenarioValidator evidence definitions pass.
- All 293 retained Godot launches pass. Four ordinary Main music modes pass: normal,
  muted, missing-resource and captions-off. Full committed saves and transcripts are
  byte-identical across modes; see [parity](main-parity.json). Fixtures use real region,
  physics displacement, committed preparation/treatment and exact recorded replies.
  They cover hidden turn/generation/participation, exact active search, all reading
  surfaces, failed/successful restoration, missing bus, cancellation and owned teardown.
  Waiting model fixtures have no credential or provider traffic.
- Twenty actual Movie Maker Master-output cases pass: both full motifs, Music/Master
  half/zero/mute, stereo/mono, Reduced range, unrelated Effects mute, missing resources/
  bus, both interruption fades, cancellation, unmute without replay, focus and long frames.
  Solo cases prime presentation quiet time to isolate output; ordinary Main checks the
  actual 12/20-second schedule. The five-minute case advances real presentation time,
  mixes existing B01, footsteps, A04/A05 and the admitted reply indication, and contains
  exactly two disjoint motifs. Subtracting the exact bed verifies long music-free gaps
  and no backlog. [Measurements](output/measurements.json), [timeline](output/timeline.json)
  and [lossless Master capture](output/master.flac) retain the output evidence.
- Native decoded streams exactly equal signed accepted PCM24 shifted right eight bits
  to PCM16. The two source WAVs remain byte-identical; no new audio edits or extraction.
  The FLAC archive decodes to the complete actual Master PCM exactly.
- Actual resource export retains 33 native streams (31 previous plus two music), fourteen
  notices and 80 exact entries in [inventory](export-inventory.json) and the audio-only
  subset ZIP. This verifies packaging, not an executable application.
- Graphical ordinary Main Audio/credits captures at 1280×720 and 1600×900 fit their
  panels, expose the explanation/current contributors and retain keyboard return.
  These are layout fixtures; playback gates are checked separately. A full graphical
  fixture run correctly yielded when another render window took focus.
- The independent successor validator checks all 82 accepted bindings, the complete
  4,772-file protected product snapshot, the editor exception, seven shipping files,
  exact imports and bounded Main/settings/credits/export edits. Eleven rejection probes
  cover changed samples, compressed/looping/normalized imports, wrong contributors,
  duplicate scene owner and physical placement drift. Frozen predecessor validators,
  audio selection, accepted art and their acceptance hashes remain unchanged.

## Measurements and limits

The complete controlled capture measures **−25.17 LUFS, −6.45 dBTP and 10.10 LU LRA**.
Its integrated level is 0.17 LU below the provisional Default audition band. The
five-minute mix measures **−24.86 LUFS, −6.45 dBTP and 4.00 LU LRA**; its range is below
the provisional Default 8–12 LU range. These fixtures establish output behavior and
report deviations honestly. They establish no device listening, ordinary-play loudness
compliance or release certification. No normalization output is written.

`./scripts/quality.sh` stops in the unchanged historical garden validator because the
ordinary Python/Pillow lacks `Image.get_flattened_data`. Its frozen validators also have
predecessor path boundaries. The relevant product checks above were run independently;
unchanged accepted art evidence is reused. This is not a full quality-wrapper pass.
Existing Godot ObjectDB exit warnings remain visible in logs. Full macOS executable
export is still unavailable because the mono export template is missing and the existing
export lookup expects `game/Allrpcs.Game.sln`. Resource packaging succeeds. Graphical
Godot uses host execution; sandbox log-directory failures need the identical host retry.

## Reproduce

Run from the repository root. NumPy analysis uses the bundled Python:

```sh
PYTHON=/Users/hherb/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/bin/python3
python3 scripts/check-continuation.py --self-test
"$PYTHON" scripts/validate-music-audio-runtime.py --reviewed
./scripts/validate-workflows.sh
dotnet format Allrpcs.sln --no-restore --verify-no-changes
dotnet format game/Allrpcs.Game.csproj --no-restore --verify-no-changes
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
dotnet build game/Allrpcs.Game.csproj --no-restore
# Run dotnet test --no-build for all four tests/*/*.csproj projects.
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
./scripts/smoke-godot.sh
# Repeat for muted, missing, captions-off; OUTPUT must be an absolute path.
ALLRPCS_MUSIC_MAIN_CHECK=1 ALLRPCS_MUSIC_MODE=normal \
ALLRPCS_MUSIC_CHECK_OUTPUT="$PWD/audio-direction/music-runtime-review/main-normal.json" \
godot --headless --path game
ALLRPCS_MUSIC_MAIN_CHECK=1 ALLRPCS_MUSIC_MODE=views \
ALLRPCS_MUSIC_VIEWS="$PWD/audio-direction/music-runtime-review/views" godot --path game
godot --headless --path game res://MusicAudioRuntimeProof.tscn --quit-after 5
ALLRPCS_MUSIC_OUTPUT_CHECK=1 \
ALLRPCS_MUSIC_TIMELINE="$PWD/audio-direction/music-runtime-review/output/timeline.json" \
ALLRPCS_MUSIC_DECODE_PATH="$PWD/audio-direction/music-runtime-review/output" \
godot --path game res://MusicAudioRuntimeProof.tscn \
--write-movie /private/tmp/allrpcs-music-master.avi --fixed-fps 60
ffmpeg -i /private/tmp/allrpcs-music-master.avi -vn -c:a pcm_s16le /private/tmp/allrpcs-music-master.wav
"$PYTHON" scripts/check-music-audio-output.py --wave /private/tmp/allrpcs-music-master.wav \
--timeline audio-direction/music-runtime-review/output/timeline.json \
--output audio-direction/music-runtime-review/output/measurements.json
godot --headless --path game --export-pack 'macOS Development' /private/tmp/allrpcs-music-resources.zip
python3 audio-direction/music-runtime-review/checks/verify-export.py /private/tmp/allrpcs-music-resources.zip
```

The output render's playback sources stayed unchanged during subsequent additions to
Main fixture coverage and analysis. A physics-frame traversal arrangement removes the
fixture's dependence on idle-frame timing. No accepted product inputs were changed to
resolve environment or diagnostic failures. Generated untracked `.gd.uid` sidecars are
removed; the recorded unrelated `game/project.godot` rewrite is preserved and excluded.

This whole named music integration is complete. Standing task commit/push authority is
recorded in [the publication grant](../footstep-runtime-review/publication/authority.json).
Wider material/positional audio, physical water/tool/interior scope, voice and the overall
opening-region identity remain unfinished; none is accepted or implemented by this slice.
