# Approved sickhouse and laundry interior audio

The owner approved the exact published [proposal](../interior-runtime-scope.md) on
2026-10-09 with “approved”. The [authority](../interior-runtime-authority.json) binds
both proposal files. The two existing structures now have a shared, transient audio
model: sickhouse timber contacts, laundry earth contacts and softer exterior ambience.
No new PCM, world material, physical room, artwork, collision or saved indoor state is added.

The source observes exact live layouts, their projection identities/transforms and current
navigation support. Doorway positions come from the live layouts, including the laundry's
route-derived openings. Conservative inner floors and open-width portal strips override
only the old silent interior exclusions; exterior material selection and contact cadence
remain unchanged. The new nodes precede the accepted channel owner, which remains the
final root child. Removing the two nodes/references reproduces the prior Main scene exactly.

Distance from the nearest actual opening controls a 32-pixel enclosure blend. At full depth,
B02/B03/B04 are attenuated by 9 dB in the sickhouse and 3 dB in the laundry. B01 is unchanged.
One shared four-gain source advances before consumers and applies a bounded amplitude slew.
Each existing bed player applies its multiplier once. B02 remains a single spatial player;
its range/pan/listener, recorded reply, hearing and history are unchanged. Actions, contact
volume, music opportunities, buses and ducking retain their existing owners.

The fifth caption, “Outdoor sounds softened”, reports only the local audio presentation.
It remains available with muted output and clears independently. Successful load, pause,
disabling, teleport, invalid/replaced owners/transforms/navigation, focus loss, invalid
presentation time and removal reset attenuation to neutral. Invalid sources restore the
original silent interior contact behavior. Missing contact streams retain safe captions.

## Verification

See [the bound verification](../interior-runtime-verification.json), [output matrix](output/timeline.json),
[measurements](output/measurements.json), [packaging](output/packaging.json), ordinary Main
fixtures and `logs/`. The implementation records product checks, all retained Godot smoke
commands and any identical native-shutdown retries. Frozen validators and accepted evidence
remain unchanged. The historical quality wrapper still stops because ordinary Pillow lacks
`Image.get_flattened_data`; product checks are run independently.

The four ordinary Main modes (`normal`, `muted`, `zero`, `mono-reduced`) use the same actual
post-collision traversal, existing preparation/treatment and recorded-reply sequence. Full
save bytes and transcript text match exactly. Fixtures cover live floors/portals, standing
silence, both laundry returns, wall/topology preservation, no enclosure-created music,
failed/successful restore, pause, disable, teleport, foreign transform, missing bus/source,
missing resources, cancellation and teardown.

Thirty-nine **actual engine Master-output cases** cover all four beds at neutral/half/full
depth in both profiles, doorway/jitter and reset/owner loss, two contacts and an unsupported
wall, overlap, category mutes, mono and both range presets. Steady measured attenuation is
within 0.25 dB of the proposed value; B01 is exactly unchanged in the retained captures.
The aggregate controlled recording measures −28.28 LUFS, −10.74 dB true peak and 13.50 LU
LRA. This is a mixed diagnostic matrix, not ordinary-play loudness compliance. No source
normalization or PCM edit was written.

All sixteen reused installed WAVs match their native PCM24-to-PCM16 engine conversion.
Actual resource packaging matches all 33 existing native streams and 14 notices (80 audio
entries). The full package and lossless audio subset are temporary verification artifacts;
exact inventories/hashes are retained in `output/packaging.json`. No executable release is
claimed: the recorded mono-template/C# export lookup limitations remain.

[Caption capture at 1280 × 720](views/captions-1280.png) and
[1920 × 1080](views/captions-1920.png) show all five lines together. Muted output,
caption disabling, credits/history and keyboard return are checked at both viewports.
The graphical fixture also checks a real native-window focus-loss reset. These are layout
and lifecycle fixtures, without new visual selection or device listening.

## Reproduction

Run from the repository root. Python output measurement/native parity needs the retained
bundled NumPy runtime; the boundary/continuation checks use ordinary Python.

```sh
python3 scripts/check-continuation.py --self-test
python3 scripts/validate-interior-audio-runtime.py --reviewed
dotnet format Allrpcs.sln --no-restore --verify-no-changes
dotnet format game/Allrpcs.Game.csproj --no-restore --verify-no-changes
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false
dotnet build game/Allrpcs.Game.csproj --no-restore
# dotnet test --no-build for each of the four tests/*/*.csproj projects.
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
./scripts/smoke-godot.sh
# Review-only equivalent, retrying only diagnosed native mutex-lock shutdown aborts:
python3 audio-direction/interior-runtime-review/checks/run-retained-smoke.py

# Repeat the ordinary fixture for all four named modes, using an absolute output path.
ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1 ALLRPCS_INTERIOR_MAIN_CHECK=1 \
ALLRPCS_INTERIOR_MODE=normal \
ALLRPCS_INTERIOR_CHECK_OUTPUT="$PWD/audio-direction/interior-runtime-review/main-normal.json" \
godot --headless --fixed-fps 60 --path game
ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1 ALLRPCS_INTERIOR_MAIN_CHECK=1 \
ALLRPCS_INTERIOR_MODE=views \
ALLRPCS_INTERIOR_VIEWS="$PWD/audio-direction/interior-runtime-review/views" godot --path game

godot --headless --path game \
--script "$PWD/audio-direction/interior-runtime-review/checks/check-stopped.gd"
ALLRPCS_INTERIOR_OUTPUT_CHECK=1 \
ALLRPCS_INTERIOR_TIMELINE="$PWD/audio-direction/interior-runtime-review/output/timeline.json" \
ALLRPCS_INTERIOR_DECODE_PATH=/private/tmp/allrpcs-interior-decode \
godot --path game res://InteriorAudioRuntimeProof.tscn \
--write-movie /private/tmp/allrpcs-interior-master.avi --fixed-fps 60
ffmpeg -i /private/tmp/allrpcs-interior-master.avi -vn -c:a pcm_s16le /private/tmp/allrpcs-interior-master.wav
/Users/hherb/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/bin/python3 \
scripts/check-interior-audio-output.py --wave /private/tmp/allrpcs-interior-master.wav \
--timeline audio-direction/interior-runtime-review/output/timeline.json \
--output audio-direction/interior-runtime-review/output/measurements.json \
--decode /private/tmp/allrpcs-interior-decode
godot --headless --path game --export-pack 'macOS Development' /private/tmp/allrpcs-interior-resources.zip
python3 audio-direction/interior-runtime-review/checks/verify-export.py /private/tmp/allrpcs-interior-resources.zip
```

This is the complete named interior slice. Wider material/positional audio, new water/tool
owners, NPC footsteps, full room reverb/transmission/filtering, voice and the whole regional
identity remain unfinished. Standing commit/push authority applies to this task.
