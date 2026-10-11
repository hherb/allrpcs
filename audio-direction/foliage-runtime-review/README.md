# Fixed garden foliage runtime review

The owner approved the exact ledger-selected implementation on 2026-10-11 and
confirmed a qualifying new leaf-rustle passage at 0:08. The
[grant](../foliage-runtime-authority.json) records both decisions and the ongoing
permission to search/download sounds for this game. The frozen
[proposal](../foliage-owner-scope.md) remains unchanged.

One new independently owned Ambience player is fixed at `(2232,1328)`, bound to the
existing garden broadleaf without changing it. It uses the existing Seeker
listener inside an exclusive 192-pixel range and supported exterior listener
positions. The first start and every later start occur after twelve active
seconds, with a three-second passage. Rejected starts are consumed; approaching,
unmuting or restoring resources never causes mid-passage replay. Pause, focus,
overlays, invalid deltas, prerequisite drift and teleport cancel/rebase only this
owner. Successful loads clear only its transient state; failed loads leave it
unchanged. No gameplay hearing, inventory, provider, persistence, time, weather,
geometry, accepted artwork or previous audio owner was changed.

The new anonymous tenth caption is “Leaves rustling” with the existing optional
screen bearing. Its separate sixteen-entry, twelve-second-limited history appears
under **Nearby foliage cues** and remains accessible with muted/missing audio or
captions disabled. Existing cue ownership, capacities, focus and return behavior
are preserved.

The [source record](../foliage-source/README.md) binds the one CC0 original,
creator/deed captures, owner listening and the exact 8–11 second derivative.
F01 is stereo PCM24 at 48 kHz, with 144,000 frames, 100 ms linear endpoint fades
and -5 dB uniform gain. Sample peak is -18.36765 dBFS; integrated analysis is
-34.69 LUFS. Godot's new import explicitly disables compression and looping.
[Preparation evidence](source/preparation-checks.json) proves exact byte
reproduction and rejects seven incorrect source/preparation inputs.

The [complete root quality report](quality/summary.json) passes **82 stages and
5,287 unit tests**, retaining all 72 prior stages. All seven fresh foliage modes
also retain the three accessible records (zero for disabled) before lifecycle tests.
The runner self-tests pass seven cases and continuation probes pass eleven.
Stage logs are retained verbatim; the native launch log is gzip-compressed to
preserve its trailing blank line, as mapped in [log storage](quality/log-storage.json).

Evidence:

- [Seven Main modes](live/default.json) use identical actual input and
  post-collision motion. First six consume/present three starts; the disabled
  baseline presents none. Complete durable saves, transcripts and serialized
  protected state match. Setup establishes ordinary garden map visitation before
  capture; it freezes the root party/turn presenter and leaves movement live.
- [Native Master measurements](output/measurements.json) independently check 46
  cases covering all
  named distance, pan/mono/zoom, settings, ducking/recovery, missing resource/bus,
  range entry/exit, pause/focus/overlay/text, load, teleport, identity/support and
  retained-owner overlap cases. Native support overrides isolate fixed-player
  distance/pan from stationary scenery navigation; ordinary support is separately
  verified in Main. Center source matching checks the first 139,200 frames within
  eight PCM16 least-significant bits; the recorded final 100 ms fade is measured
  separately because the mixer releases at the three-active-second deadline. Full
  source conversion is checked byte for byte in the mounted archive. Stopped
  fixtures never inject into an automatic live owner.
- [Native UI](views/native.json) covers actual off-screen wording, all ten
  simultaneous caption lines at 1280×720 and 1920×1080, independently bounded
  selectable/scrolling history and keyboard/return focus. Prior motion/caption
  writers are frozen only for simultaneous diagnostic line inspection.
- [Resource archive](packaging/inventory.json) contains 37 streams and preserves
  all 95 prior audio entries byte for byte. [Mounted decoding](packaging/native-decode.json)
  matches every F01 byte to the recorded signed PCM24 arithmetic shift to PCM16.
  All three retained export presets include its credits, provenance and deed.
- [Verification](../foliage-runtime-verification.json) binds installed and review
  evidence and seven boundary mutation rejection groups. Root quality retains all
  72 prior stages, pins unchanged Healer validators/output checks to `ae06c9ab`,
  and adds the new boundary, seven modes and output reanalysis.

The proposal's expected 24 individual notice files was an arithmetic mismatch:
its explicit permitted deliverables add three files (credits, provenance, deed)
to the 23 retained files. The actual archive has **26 notice files in 18 groups**.
All required files are included; no old file was edited or omitted. The frozen
proposal has not been changed to hide that discrepancy.

Controlled source/output measurements establish neither fresh-device listening
nor ordinary-play loudness compliance. Resource packaging is not an executable
release. Current review fixtures start stopped unless opted in; all failed
fixture attempts were repaired before passing evidence was recorded. The unrelated
recorded `game/project.godot` rewrite remains untouched and excluded.
