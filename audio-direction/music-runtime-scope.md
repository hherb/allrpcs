# Restrained music runtime proposal

Prepared 2026-10-08 for the remaining Phase 9.3 music item. This is completed
planning; the new runtime grant is pending. The [structured inventory](music-runtime-scope.json)
binds the inspected product, accepted material, owners and exact implementation paths.

The proposed result is occasional optional music: one unchanged 25-second exploration
passage M01, or the accepted 25-second shakuhachi M02 after committed treatment.
One player, twelve quiet seconds before admission and at least three music-free minutes
after a passage keep silence the usual traversal experience. Both yield to reading,
dialogue and physical actions. No continuous loop, proximity clue or treatment-success
fanfare is added. Music mute/zero continues to provide a fully music-free game.

## Current owners and missing implementation

| Inspected owner | Existing behavior and proposed use |
| --- | --- |
| `OpeningRegionMaterialAudioController.Current` | Existing exact nearest-anchor B01–B04 selection. Observe a change of clip identity for M01; never change its selection, ramp, gain or captions. |
| `OpeningRegionFirstPatientAdministrationInputCoordinator.TreatmentCompleted` | Exception-isolated notification after exact administration commits. Admit one temporary M02 opportunity from this live event; do not inspect restored treatment history. |
| `OpeningRegionRemedyPreparationInputCoordinator.GrindingCompleted` | Existing committed grinding notification. Reserve eight presentation seconds for its A04 passage and reset music quiet timing. |
| `TemporaryCharacterTurnResponseSpeechRecorder.ReplyRecorded` | Existing post-commit reply notification. Use only the already admitted Seeker audience to interrupt music; retain positional click/text/hearing/history ownership. |
| `CharacterTurnActivityController.Activity`, `CharacterModelGenerationFlowController.IsGenerating`, `CharacterTurnParticipationRoundTaskController.Status` | Awaiting response, speaking, generation or Running participation blocks admission and interrupts an active passage. Completed, resolved and cancelled turn snapshots alone do not remain blockers. |
| `FocusedSearchActivityController` and `FocusedSearchActivityIndicatorPolicy.ShouldShow` | Existing exact Seeker search activity blocks music; no music-driven search completion or perception change. |
| `DialogueOverlay`, `SettingsOverlay`, `JournalSectionPanel`, `SeekerMapPanel`, `AudioSettingsPanel` | Visible reading/settings surfaces block admission and interrupt music. Credits/history children count even while the Audio parent is hidden. Existing GUI focus also blocks. |
| `Seeker.MotionCompleted` | Observe actual displacement to distinguish traversal from initial/loading projection and detect teleport. Preserve movement, footsteps and animation. |
| `SaveSnapshotRestorationController.PresentationReset` | Stop and forget presentation only after successful load. Failed load leaves ordinary presentation intact. |
| `AudioSettingsController` | Already owns Music/Master gain/mute, mono, presets, persistence and ducking. The new player uses these unchanged. |

There is **no gameplay rest action or rest-state controller** in the inspected product.
M02 therefore follows the existing treatment presentation only. Its filename and source
title grant no rest interaction, disease pause, cure judgment or gameplay clock authority.
Other healing/rest events and future Speech playback require their own owners and scope.

## Exact admission and silence rules

These are proposed named constants, not measurements of implemented behavior. A new pure
`RestrainedMusicSchedule` owns presentation time, one pending opportunity and one active
passage. `OpeningRegionMusicAudioController` translates existing scene observations into
that schedule; it never writes a world/save/provider object.

**M01 exploration:** a transition between two different, non-null B01–B04 clip IDs
creates an authored presentation opportunity, provided no passage is active or cooling
down. The first observation after scene startup/reset creates no opportunity. Changes
only to caption or gain, including the revealed woodland state, create none. Transition
through null clears the baseline and pending M01; returning from null only establishes
a new baseline. No hidden object, discovery, patient diagnosis, camera boundary or
same-bed anchor change is a trigger.

The opportunity expires at 45 presentation seconds after that transition. Before play,
the new clip ID must stay selected for twelve seconds, the Seeker must accumulate at
least 24 pixels of ordinary post-collision displacement after the event, and all blockers
must remain clear for twelve consecutive seconds. Travel itself is not a blocker.
Unmapped/foreign material inputs cancel M01. A changed bed replaces a pending M01 with
a new event/deadline and zero dwell/displacement; it cannot replace pending M02.
No periodic timer creates a new opportunity while standing still in the same bed.

**M02 treatment:** accept only a fresh live `TreatmentCompleted` notification carrying
the exact current `OpeningRegionFirstPatientSceneSource.Binding`. One opportunity per
live binding per reset epoch; duplicate/foreign bindings and restored experiments are
inert. Protect the existing eight-second A05 passage, then require twelve consecutive
quiet seconds. Thus the earliest M02 is twenty seconds after that event. It expires
45 seconds after the commit, whether quiet or blocked. M02 replaces a pending M01,
but never interrupts a passage to start another motif. Treatment interrupts any active
music for foreground priority; that interrupted passage starts the normal cooldown,
so that treatment opportunity is discarded. A motif is optional, never promised.
M02 does not require a new spatial radius or new success query.

Common rules:

- At most one opportunity and one playing passage. No queue, backlog, random selection,
  catch-up, seek or resume. A pending opportunity is consumed on start or discarded on
  expiry. Expiry wins at the exact 45-second boundary.
- Starts during an active passage, its fade-out or its cooldown are discarded. Any
  natural finish, interruption or failed start creates a 180-second cooldown from actual
  stop/failure time. Cooldown expiry creates no event; a fresh later trigger is required.
- A blocker resets quiet time while an existing opportunity ages toward its deadline.
  Start only when dwell/displacement (M01), foreground hold (M02), quiet interval and
  cooldown checks all pass in the same update. No user setting change creates a trigger.
- Master/Music mute or zero stops active music immediately and discards pending music.
  Events while inaudible are discarded. Unmute, slider changes, mono/preset/caption changes,
  defaults and preference reload do not replay or resume a passage.
- Use finite nonnegative process deltas, unrelated to disease/world time. A delta above
  0.25 seconds or window focus loss immediately stops/clears music and imposes the
  180-second cooldown; do not process suspended time as quiet time. Invalid deltas fail
  quietly without advancing schedule. Resume requires a fresh later event.
- Fresh scene startup has no initial cooldown and no initial opportunity. Successful
  restore stops immediately, clears opportunities, treatment identities, displacement
  and selection baseline, then imposes 180 seconds. A position discontinuity over 96
  world pixels in one valid physics sample does the same. Ordinary displacement below
  that threshold is not a music interruption. Teardown stops and unsubscribes.

## Foreground interruption and output

Visible dialogue/settings/journal/map/audio/credits/history, GUI focus on a visible Control,
active Seeker search, awaiting/speaking turn, generation, Running participation and
the eight-second committed-action holds are blockers. A committed grinding event
starts an eight-second hold; treatment does likewise before considering M02.
A `ReplyRecorded` event interrupts only when its captured event audience already
admits the Seeker under the existing ordinary/whisper hearing policy. Music code never
widens 192/48 hearing, reveals speaker/source identity or changes reply delivery.
The reply also starts a three-second hold so a completed turn cannot immediately
restart music under its still-live caption. Continuous blockers extend suppression
until they clear, then a full twelve quiet seconds must pass for a pending event.

Reading, search, dialogue and committed actions fade active music from its current
gain to zero over 0.6 seconds, then explicitly stop it. No fade-in reversal or motif
swap during that fade. Mute/zero, load, teleport, focus loss, invalid owners, missing
bus and teardown stop immediately. Every stop records cooldown itself: Godot's
`Finished` notification is for natural completion, not explicit stop.
See [AudioStreamPlayer 4.6](https://docs.godotengine.org/en/4.6/classes/class_audiostreamplayer.html).

One non-positional `AudioStreamPlayer`, `Bus=Music`, `Autoplay=false`, `MaxPolyphony=1`,
`PitchScale=1`, starting at frame zero, owns both motifs. Replace its stream only while
stopped. Disable WAV looping and all importer edits/compression. The accepted files
already contain two-second edge fades; normal playback adds no fade or gain bake.
Player base amplitude is 1; only transient interruption reduces that amplitude.
Existing category/Master settings and existing A05 ducking apply once through the bus.
The music controller creates or clears **no ducking leases**. It must explicitly
check the Music bus exists: the engine otherwise falls back to Master. Missing
resources/owners/bus fail silently for gameplay and report a bounded diagnostic once.
Native WAV decoding can reduce PCM24 to the engine's working format; verify decoded
parity using the existing PCM conversion convention, without claiming native 24-bit output.
See [AudioStreamWAV 4.6](https://docs.godotengine.org/en/4.6/classes/class_audiostreamwav.html).

## Resources, rights and accessible controls

Copy exactly the complete accepted `material-audition/clips/M01.wav` and `M02.wav`
to `game/assets/audio/music/`. Both are stereo PCM24, 48 kHz, 1,200,000 frames,
25 seconds. Whole-file SHA-256 values:

| Clip | SHA-256 | Retained source |
| --- | --- | --- |
| M01 | `8e012ba16247ec94dac2088bbcffb1ed6ccda62062666492afdf34a764cd9eb6` | Sunset Plains by Yoiyami; CC0-1.0 |
| M02 | `c706dd03b3e730227ce7ec383a237f4c8f785b1f44ecaa314f04993ff5776511` | Shakuhachi flute play by gonzooka; CC0-1.0 |

The inventory records PCM hashes and all source/capture/license bindings. No extraction,
new waveform, gain, fade, resampling, channel conversion, edit, download or audition is
proposed. Retain the recorded M02 limitation: its original is 16 kHz mono; resampling
does not restore bandwidth. Terminal whole-audition acceptance supersedes its historical
pending fields. The rejected Revelation guitar is not an M02 source for this installation.

Package `CREDITS.txt`, `provenance.json` and the byte-identical retained CC0 notice with
the two WAVs/imports. Credit both current contributors and their original source URLs,
the already accepted excerpt/resampling/fade/gain lineage, and the byte-exact runtime
copy. Append this directory's credits to the existing Audio source view and add its
TXT/JSON/HTML include filters to the three existing development export presets. Keep
other notices/assets unchanged. No new licence decision or original-source redistribution.

Use the existing readable Music and Master sliders, independent mute, true zero,
Save/defaults, mono and Reduced range. Add a brief Audio-panel explanation:
“Music plays occasionally in quiet moments. Mute Music for a music-free game.”
Add a read-only `IsReading` property for Audio including its credits/history children.
Preserve keyboard/controller focus order and panel fit at both retained viewports.
No new settings document/schema, gameplay menu, caption line or sound-history entry.
The motifs carry no required information; existing dialogue/action text and independently
owned captions/history stay available with music or all audio muted.

## Exact implementation boundary after approval

The structured inventory lists each path individually. Permitted existing edits are
only `game/Main.tscn` (new controller and smoke-check references/nodes),
`game/AudioSettingsPanel.cs` (explanation and read-only reading state),
`game/MaterialAudioCreditsPanel.cs` (append music credits) and
`game/export_presets.cfg` (music notices). Existing material, footstep, positional,
Seeker, recorder, action, overlay, settings/storage/mixer and world code need no edits.

Add the music controller with UID, pure Application schedule with meaningful tests,
ordinary Main smoke check with UID, a stopped-by-default output proof with UID/scene,
and exactly seven shipping resources: two WAVs, two imports and three notices.
Add a copy/provenance preparation script, an independent successor boundary validator
and an actual Master-output analysis script. Record the actual grant in
`music-runtime-authority.json` before implementation; retain finite evidence in
`music-runtime-verification.json` and `music-runtime-review/`. Maintain the ledger,
handover and roadmap only for this named scope. Frozen predecessor validators and
acceptance artifacts remain unchanged. Product snapshot comparison permits only the
listed edits/additions; preserve/exclude local `game/project.godot` and `.DS_Store`.

## Finite verification and completion

Planning checks: all 82 retained audition bindings; accepted policy/reference metadata;
two full WAV/PCM inventories and rights bindings; inspected owner hashes and complete
protected product snapshot; continuation self-test; authored whitespace/diff review.
The positional successor validator passed before these proposal files were created.
Reuse its passing 5,117 unit tests, retained Godot suite and product evidence for this
documentation-only task. No new product or playback test is claimed by planning.

After one new runtime approval, completion requires:

1. Exact two-file copy/header/PCM/import/native decode parity, seven music resources and
   actual package inventory including all 31 predecessor streams and retained notices.
   Reject a changed sample, missing/currently wrong contributor, compressed/edited/looping
   import, extra WAV, duplicate player/route and forbidden product mutation.
2. Pure schedule checks at every threshold, including 12-second quiet/dwell, 24-pixel
   movement, 45-second expiry, 8/3-second foreground holds, 0.6-second stop and
   180-second cooldown; priority, finite deltas, same-bed/revealed changes, null baseline,
   foreign/duplicate binding, stopped startup, expiry-versus-admission tie and no backlog.
3. Actual ordinary Main regional traversal and committed treatment fixtures; failed,
   duplicate/foreign and restored actions; heard/unheard ordinary/whisper reply events;
   hidden dialogue while generating, reading child views, search, focus loss, teleport,
   successful/failed load, missing assets/bus and teardown. Assert predecessor cue/caption/
   history and A05 lease behavior without rewriting their frozen validation allowlists.
4. Enabled, muted, missing-resource and captions-off runs with identical full save roots,
   transcripts/events, perception, time/disease, actions and movement. Preference changes
   cannot create an event; unmute/load/return cannot replay historical music.
5. Actual final Master output for M01/M02 alone and a five-minute mixed traversal/action/
   reply/quiet sequence: count starts and silent gaps, verify no overlapping motifs,
   stop latency/envelope and no catch-up. Exercise Music/Master gain/half/zero/mute,
   Default/Reduced range, stereo/mono, missing bus and A05 ducking/cancellation. Record
   measured loudness/peaks and deviations; tune no accepted bytes to hide them.
6. Ordinary Main Audio explanation/controls/credits/focus and unaffected four caption
   lines/recent cues at 1280×720 and 1600×900; meaningful pure tests, formatting,
   builds/four unit projects, ScenarioValidator and retained Godot smoke suite plus
   the new scoped probes/output checks. Standalone proof starts stopped. Preserve known
   missing mono export template/solution lookup and device-listening limitations.

Close only the named M01/M02 integration when these finite checks pass. This can satisfy
the restrained-music roadmap item at its stated scope; it cannot complete broader
material/positional audio, rest gameplay, voice or the whole opening-region identity.
No numerical loudness, device-listening or executable-release compliance is assumed.

The only pending decision is the **new M01/M02 runtime scheduling, two byte-exact
installations, reading-state/credits changes and finite checks above**. Authority is
required by [AGENTS.md](../AGENTS.md), “A genuinely new runtime/layout/physical scope
still needs the applicable authority.” Standing commit/push authorization already
applies to this completed proposal; no repeated listening or publication approval is sought.
