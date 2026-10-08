# Six-sound runtime implementation and retained evidence

The owner approved the prepared scope on 2026-10-08. The exact grant is in
[material-runtime-authority.json](../material-runtime-authority.json); the scope
and its inventory remain frozen. B01–B04 supply the four existing regional rain,
water, canopy and road/wind beds. A04 is emitted after the exact successful
Grinding step, and A05 after successful first-patient administration. Their
accepted WAVs are byte-identical. New captions and source credits use the existing
Audio controls; dialogue and consequence text remain available independently.

This completes that named runtime boundary. Footsteps, material classification,
water/tool handling, positional/off-screen cues, music and voice stay unfinished.
No accepted art, physical owner, gait, source edit or prior acceptance is changed.
The unrelated project editor rewrite stays local. Git publication was not authorized.

## Runtime behavior

Main starts live ambience after its sources are ready. The nearest authored anchor
selects its exact current environmental region, with ordinal-ID ties; this is only
an audio presentation approximation. Linear 0.75-second ramps remain bounded and
retarget from current values. The existing revealed woodland state reduces B03
by 3 dB without any audio-owned discovery trigger. Unknown source keys fade quiet.
The four bed players retain phase while gain/mute changes. Whole 30-second loops
and eight-second action passages keep the accepted samples and timing.

Successful grinding, treatment and restoration deliver isolated transient
notifications. A failing listener cannot invalidate committed gameplay or prevent
another listener receiving it. Failed/repeated/remote actions emit no new cue.
Successful restoration cancels actions/captions and the exact A05 foreground lease,
without replaying saved history; failed restoration emits no reset. Captions are
independent of output mute. Muted or missing A05 does not acquire a consequential
lease, and mute/missing/reset/finish/teardown releases an owned lease without restarting it on unmute.

Godot's default QOA compression was caught and disabled in all six WAV sidecars.
Actual loaded streams are stereo PCM16 at 48 kHz, retaining the known engine
precision conversion from accepted PCM24. Trimming, normalization, rate conversion
and mono conversion are disabled. Source bytes are unchanged. Exact import-resource
paths, stream bytes and notices were checked in the resource-only export.

## Reproducing checks

```sh
python3 scripts/validate-material-audio-runtime.py --reviewed
python3 scripts/check-continuation.py --self-test
git diff --check
```

The successor validator checks actual approval, accepted source bytes, the precise
additive Main scene, notice-only export filters, protected predecessor inputs and
the local editor rewrite. Its three negative probes reject replaced audio,
duplicate material composition and changed physical placement. It preserves the
old audio/art validators and never changes their acceptance assumptions.

For product checks, run the commands retained in `scripts/quality-audio.sh` after
the successor boundary check, starting at `./scripts/validate-workflows.sh` rather
than its old snapshot validator. The record retains restore, format verification,
build, four unit-test projects, ScenarioValidator and all existing Godot smokes.
The initial channel-order failure was corrected by placing only the new audio nodes
before WholeChannelPresentation; the whole channel remains the final root child.
Checks before that failure were reused, and the suite resumed from the unchanged
channel check through its end. Final build/format and material checks followed. A final missing-playback lease
cleanup branch and explicit teardown/restoration-listener probes were then verified
by rerunning material Main modes, affected retained action/audio smokes and the
complete rendered output; unchanged Domain/Application/Infrastructure and unrelated
smoke evidence is retained.

Ordinary Main checks use disposable results and no model network:

```sh
ALLRPCS_MATERIAL_MAIN_CHECK=1 ALLRPCS_AUDIO_MAIN_CHECK=0 \
ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1 \
ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=1 \
godot --headless --path game --quit-after 1000
```

Repeat with `ALLRPCS_MATERIAL_ACTION_MODE=muted`, `missing` or `captions-off`, and
`ALLRPCS_MATERIAL_RESULT_PATH=/tmp/material-result.txt` to compare committed disease,
batch, administration and event/time results. [The four retained results](actions/)
are identical, including complete committed save-root JSON with perception/memory,
discovery, journal/map/routes, disease, batch, administration and event/time state.
Persistence tests initialize their existing journal/map/route
fixtures; those arrangements belong only to the enabled smoke check.

Open the stopped laboratory with:

```sh
godot --path game res://MaterialAudioRuntimeProof.tscn
```

Select a clip to play its accepted passage and use Stop immediately. Automated
Movie Maker mode exercises the same controller/player class and existing mixer:

```sh
mkdir -p /tmp/material-output
ALLRPCS_MATERIAL_OUTPUT_CHECK=1 \
ALLRPCS_MATERIAL_TIMELINE=/tmp/material-output/timeline.json \
godot --path game res://MaterialAudioRuntimeProof.tscn \
  --write-movie /tmp/material-output/probes.png --fixed-fps 60 \
  --disable-vsync --rendering-method gl_compatibility --quit-after 28000
```

For the retained run, a disposable 320×180 project reduced video transport only.
Its clips, imported streams, scripts, mixer and compiled assemblies were copied
byte-for-byte from the implementation. The archive retains those actual source
inputs. The full Master WAV duration is measured independently of Godot's count
of changed video frames; the frame statistic is not the audio duration.

The complete Master capture is losslessly gzip-archived in
[output/master.wav.gz](output/master.wav.gz). Unpack and analyze it with the bundled
Python (NumPy) and ffmpeg:

```sh
gzip -dc audio-direction/material-runtime-review/output/master.wav.gz > /tmp/material-master.wav
/Users/hherb/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/bin/python3 \
  scripts/check-material-audio-output.py --wave /tmp/material-master.wav \
  --timeline audio-direction/material-runtime-review/output/timeline.json \
  --output /tmp/material-measurements.json
```

The analyzer uses captured output for gain/zero/mute, mono, existing compression,
loop continuity, revealed attenuation, crossfades/retargets, cue cancellation,
missing playback and mute/unmute phase. Runtime proof assertions cover A05 lease
release. Measurements are technical evidence, with actual peak/LUFS/LRA and seam
deltas reported. They are not a new listening selection, production-play loudness
qualification or target-compliance claim.

## Views and packaging limits

The [views directory](views/) retains inspected Main settings and credits at
1280×720 and 1600×900 plus ordinary treatment captions. The captions fit at the
bottom left, do not take focus and clear under the preference/reset. Credits scroll,
expose working source links and return focus to their Audio entry. Existing world
views are reused without another acceptance gate.

[Export inventory](export-inventory.json) verifies six import remaps, six exact
imported streams and five source/provenance/licence notices in the actual ZIP.
The exporter normally strips editor parameters from packaged import maps; their
UID/cache mappings and packed stream bytes were independently compared. These
notices include both attribution sources, all eight contributing source authors,
the existing edit notices and supplied Bashar credit.

A full unsigned macOS application export could not run: this host lacks the
4.6.3 mono macOS template, and the existing C# export configuration looks for
`game/Allrpcs.Game.sln` rather than the repository solution. Resource-only export
produced its archive while reporting the C# configuration errors; only the scoped
audio/notice inventory is claimed. No template install, export-system repair,
signing, release or device listening was added to this scope.

Sandbox Godot failed before execution during log/settings access; approved host
retries supplied the evidence. Retained inherited Godot smokes report ObjectDB
cleanup warnings, and editor shutdown can report its existing EditorSettings error.
Those are distinguished from the passing assertions and changed runtime views.
