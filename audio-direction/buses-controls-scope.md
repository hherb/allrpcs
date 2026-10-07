# Audio buses and independent controls: prepared implementation scope

2026-10-07 · Phase 9.3 · Runtime authority pending.

The owner approved the audio policy with the exact statement **“approved”**. The
[terminal acceptance](owner-acceptance.json) binds the reviewed policy files. Their
historical proposal/review fields remain unchanged; terminal acceptance supersedes them.
Numerical mix targets remain provisional. This document completes the authorized
scope preparation for the unfinished volume buses and independent controls item.

## Proposed deliverable

Add five sibling buses, Music, Ambience, Effects, Dialogue and Speech, all feeding
Master. Supply six independent volume sliders and mute controls in an Audio settings
panel, persist preferences across restarts, and demonstrate routing and mix behavior
in a disposable scene. Speech remains labeled as reserved for future playback.
The demonstration establishes control behavior; production sound selection and
real-world loudness measurements remain later deliverables.

The proposed runtime scope includes a Default/Reduced range selector, mono output,
and a stored sound-caption preference. Reduced range must apply a working master
dynamics configuration in the demonstration; mono must perform actual fold-down.
Caption preference is explicitly labeled for future meaningful non-speech cues.
Existing dialogue and consequence text is always available independently of it.
Do not present an inert preset or mono switch as working functionality. LRA bands
and true-peak targets from the policy need later representative mix measurements.

## Inspected integration points

- `game/Main.tscn` contains the optional provider `SettingsOverlay` and dialogue
  overlay. `OptionalOverlayVisibilityController.cs` hides/toggles both and releases
  their focus. Add an Audio entry and separate panel that do not obstruct those
  controls, journal, map, or the 1280×720 base viewport.
- `game/CharacterModelSettingsStorageController.cs` composes isolated `user://`
  storage and a disposable smoke-test path. Follow its separation of application
  state, infrastructure serialization/storage, and Godot composition. Audio
  preferences use their own document and never share provider settings or saves.
- The inspected C# and scene/resource files contain no `AudioServer`,
  `AudioStreamPlayer` or `AudioBusLayout` implementation. Introduce the category
  routing once; do not connect ambience, world actions or character utterances yet.
- Preserve the unrelated editor rewrite in `game/project.godot`. Runtime composition
  can install the dedicated layout at startup without replacing that file. Reconcile
  any later project-setting requirement with the actual diff before editing it.

## Settings and behavior

| Setting | Initial value and contract |
| --- | --- |
| Master and five category gains | 100%, range 0–100%, no gain above unity. Zero is exact silence. Positive values map to linear amplitude. Display readable percentages. |
| Six explicit mutes | Off. Mute does not erase the slider's stored value; unmute restores it. Master mute overrides all categories. |
| Mix preset | Default. Reduced range changes only presentation processing; never user gains or perception. |
| Mono output | Off. Fold down at the common output; preserve mute and avoid doubling amplitude. |
| Sound captions | On, stored for future meaningful cues. Always retain existing text. |
| Restore defaults | Restore all six gains, six mutes, preset, mono and caption preference together. |

Store a versioned `user://audio-settings-v1.json` containing the complete snapshot.
Load and validate it before applying the mix. Reject nonfinite/out-of-range gains,
missing or duplicate categories, invalid types and unknown schema/preset values.
Missing or invalid storage falls back to complete defaults with a readable status;
failed saves report failure without destroying the previous valid document. Use
atomic replacement and isolated paths for smoke tests. Apply changes immediately,
with short gain ramps, and expose an explicit Save action/status for persistence.
No setting changes world hearing, memory, perception, disease time or action success.

Keyboard/controller focus must reach every slider, toggle, Save, Restore defaults,
and Close. The Audio entry and panel must work with the existing overlay toggle.
Closing returns focus predictably and consumes the close event without triggering
world actions. Do not add disease-time pause behavior in this slice.

Each sound routes once to its category. Dialogue owns nonverbal conversation cues;
Speech owns future utterances, never a child of Dialogue. Decorative wagon creak
belongs to Ambience; general UI feedback belongs to Effects. These are routing
assignments for later emitters, not authorization to install them here.

Implement the accepted ducking envelope in the disposable demonstration. Use the
strongest reduction per bus, retain user attenuation/mute, and release correctly
after overlap, cancellation, failure and teardown. Synthetic foreground triggers
exercise speech, consequential cues and typed-reading focus. No production speech
or off-screen perception event is synthesized. Reduced range must not amplify a
muted channel or change event eligibility.

## Intended file boundaries

| Area | Proposed additions / bounded changes |
| --- | --- |
| Application | `src/Application/Audio/`: validated preference snapshot, category/mix settings and storage contract; pure gain/ducking transitions as needed. |
| Infrastructure | `src/Infrastructure/Audio/`: strict versioned serializer/deserializer and atomic local document store. |
| Godot | `game/AudioBusLayout.tres`, `AudioSettingsController.cs`, `AudioSettingsStorageController.cs`, `AudioSettingsPanel.tscn` and its script; category mixer/ducking adapters as needed. |
| Integration | Small composition/UI additions to `game/Main.tscn` and `OptionalOverlayVisibilityController.cs`; keep provider/dialogue controls intact. |
| Disposable proof | `game/AudioBusesControlsProof.tscn`, proof/check scripts and temporary diagnostic signal buffers. No production test-clip assets. |
| Tests and evidence | Application/Infrastructure audio tests, Godot smoke checks, `audio-direction/buses-controls-verification.json`, and updated handover/roadmap. |

Exact helper names may follow repository conventions during implementation. Keep
audio presentation state outside Domain and avoid modifying existing world or
provider contracts. Product inputs will change, so run the applicable quality
checks after implementation; do not weaken frozen visual validators to make it pass.

## Disposable routing demonstration and validation

The later authorized proof uses short, low-level, procedurally constructed test
signals in memory, one per category, played only after explicit Start. It opens
stopped and has immediate Stop/mute. No acquired/generated production sound, voice
provider, microphone, environmental loop or main-scene emitter is involved.
Headless checks inspect the actual Godot bus state and captured signal behavior;
the visible proof supplies playback, controls and focus for manual verification.

| Case | Required evidence |
| --- | --- |
| Category isolation | Each probe reaches only its named bus and Master. Changing one category leaves all other gains and routes unchanged. |
| Zero and explicit mute | Zero, category mute and Master mute produce silence; unmute restores the saved value. Verify captured output as well as configuration. |
| Persistence | Save/restart restores the whole snapshot; malformed input and failed writes leave coherent defaults/current state and preserve prior valid storage. |
| Ducking | Speech/cue/reading triggers use policy timing and strongest overlap, preserve sliders/mutes, and release after cancellation/failure/teardown. |
| Preset | Reduced range demonstrably reduces test-signal contrast; Default restores its processing settings. No production LRA claim. |
| Mono | A panned probe folds to equal output channels without doubling level; returning to stereo restores the original routing. |
| Interface | Six readable values/mutes, defaults and save status; keyboard/controller focus and overlay open/close work at base and expanded viewport. |
| Information authority | Toggle captions/mutes/presets without changing existing text, world perception, action results or disease time. No new event feed is introduced. |

Runtime implementation completes when these checks pass, appropriate product
quality and Main smoke checks pass, and the visible settings/proof result is
available for any actual runtime-view confirmation. Retain commands/results and
limitations in verification evidence. Mark the volume-control roadmap item complete
only after that implementation, not after this scope preparation.

## Authority boundary

Authorized now: record the approved policy, close its slice, and prepare this concrete
scope with documentation/checks. The single remaining decision is authority to
implement the runtime scope described above, including its disposable signals and
settings UI. Policy approval alone did not authorize runtime buses/settings: the
review explicitly covered the policy only, and the retained policy requires a
named implementation scope.

Production footsteps/ambience/music/voice, sound acquisition or audition, perception
mapping, accepted artwork/layout, save-game changes, provider/capture work, release
and Git publication remain outside this proposed implementation. Retain this scope
while its authority decision is pending; do not recreate or request it repeatedly.
