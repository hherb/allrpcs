# Audio buses and independent controls

The owner authorized this runtime scope with **“scope approved”** on 2026-10-07.
[Authority](buses-controls-authority.json) binds the exact prepared
[scope](buses-controls-scope.md); its historical pending language is superseded.
The approved [audio policy](owner-acceptance.json) and its provisional numerical
targets stay unchanged.

## Player controls

Press **Tab**, choose **Audio settings**, and use the six sliders/mutes for Master,
Music, Ambience, Effects, Dialogue and future Speech. Category buses are siblings
feeding Master. A slider at zero is silent. Explicit mute preserves its slider
value, and Master mute silences every category. Positive slider values represent
linear gain up to unity, without amplification.

Settings apply immediately with a short gain ramp. **Save** persists the complete
snapshot across restarts; its status reports success or failure. **Restore defaults**
restores all category gains/mutes, Default range, stereo and the caption preference.
Use arrow keys or controller direction controls to navigate, left/right on sliders
to adjust, and Enter/controller accept to activate buttons. Escape/Close returns
to the optional overlays and Audio entry; Tab closes all optional surfaces.

**Reduced range** enables output compression; **Mono output** folds the common
stereo output to equal channels. A sample-peak limiter remains enabled in either
preset. These are functioning presentation controls, without a production LRA or
true-peak compliance claim. The caption preference is explicitly reserved for future
meaningful non-speech cues; existing dialogue and consequence text always remain.
Future Speech has its own independent category and does not acquire a voice provider.

Preferences live in the separate versioned `user://audio-settings-v1.json` document.
Malformed, missing or unsupported documents fall back to a complete default snapshot;
failed saves retain current preferences and preserve the previous valid document.
Neither preferences nor ducking read or change world hearing, discovery, disease
time, action success, provider settings or world saves.

## Routing and foreground behavior

All five categories route directly to Master, once per sound. Master processing
provides Default/Reduced range, optional mono and peak limiting. The mixer installs
its dedicated layout at startup without changing `game/project.godot`.

Foreground leases use the approved speech, consequential-cue and typed-reading
envelopes. Overlap retains the strongest reduction, holds until eligible foreground
ends, and releases after its hold. Cancellation and failure end the exact lease;
teardown clears transient attenuation. Music and Ambience can duck globally.
The Effects bus never ducks globally: future optional emitters apply the envelope's
Effects attenuation individually so required action/result cues keep their levels.
There is no production foreground or world-event feed yet.

The implementation uses Godot's documented
[stereo-to-mono setting](https://docs.godotengine.org/en/4.6/classes/class_audioeffectstereoenhance.html)
and [compressor](https://docs.godotengine.org/en/4.6/classes/class_audioeffectcompressor.html).
Final output measurements use the engine's
[offline WAV recording](https://docs.godotengine.org/en/4.6/tutorials/animation/creating_movies.html).

## Diagnostic scene and evidence

Run the standalone laboratory:

```sh
godot --path game res://AudioBusesControlsProof.tscn
```

It starts stopped, uses disposable settings and creates sine probes in memory only
after explicit Start. Stop ends the probe and clears synthetic foreground leases.
The synthetic speech/cue switches exercise overlap without playing voices or
creating character events. No production audio assets or sound emitters are installed.

For automated rendered-output verification, explicitly start the diagnostic command:

```sh
mkdir -p /tmp/allrpcs-audio-output
ALLRPCS_AUDIO_CHECK=1 \
ALLRPCS_AUDIO_TIMELINE=/tmp/allrpcs-audio-output/timeline.json \
godot --path game res://AudioBusesControlsProof.tscn \
  --write-movie /tmp/allrpcs-audio-output/probes.png --fixed-fps 60 \
  --resolution 1280x720 --rendering-method gl_compatibility --quit-after 2500
python3 scripts/check-audio-output.py \
  --wave /tmp/allrpcs-audio-output/probes.wav \
  --timeline /tmp/allrpcs-audio-output/timeline.json \
  --output /tmp/allrpcs-audio-output/measurements.json
```

Use a graphical renderer for Movie Maker: this Godot build's dummy headless renderer
cannot supply its movie frames. The WAV contains the complete fixed-step audio
sequence even when unchanged video frames are skipped in the render statistics.
The analyzer independently checks duration and each probe's steady output.

Retained [verification](buses-controls-verification.json) links the measured final
stereo output, complete timeline, UI capture, commands and product checks. The 38
probes cover each category's gain/zero/mute, final Master zero/mute, normalized mono
and stereo restoration, actual compression, speech/reading overlap and release,
muted ducking, and optional versus required effects. They establish mixer behavior;
real assets still need collective listening and device/mix measurements.

## Product verification boundary

Run `python3 scripts/validate-audio-publication.py --quality` for this retained snapshot. It checks the exact
additive Main composition and retained predecessor files, workflows, imported
asset parity, restore/format/build, all test projects, scenarios, the complete
existing Godot smoke suite and the new Audio Main smoke. Three negative probes
reject moved physical owners, duplicate mixer composition and renamed resources.

The older `./scripts/quality.sh` retains exact historical artwork assumptions.
Its unchanged validators are preserved, and passing terminal art evidence is reused:
the new boundary validator proves existing scene/physical/art bytes are unchanged
apart from the explicitly added audio composition/entry. System Python's older Pillow
also fails a historical helper's `get_flattened_data` call; the bundled Python has
the compatible dependency. Neither condition calls for editing frozen validators.
No fresh listening, platform-device assessment, production audition, owner aesthetic
review of unchanged visuals or release is claimed. The owner separately authorized Git
publication on 2026-10-07; see [publication authority](publication-authority.json).

The publication runner preserves the recorded runtime comparator and quality script.
It checks predecessor membership separately from the reviewed additions, verifies
all recorded source/evidence hashes, and accepts either the clean committed project
or this checkout's retained editor rewrite. `--quality` runs the recorded product
commands after the replacement boundary check; without it, only the bounded snapshot
and continuation checks run. Frozen evidence is unchanged.
