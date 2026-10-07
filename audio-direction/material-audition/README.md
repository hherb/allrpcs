# Material audio collective audition

The owner approved acquisition, editing, qualification listening and this isolated
audition on 2026-10-07. [The grant](acquisition-authority.json) binds the unchanged
[source plan](../material-audition-source-plan.md). The original sixteen-source acquisition is complete. The owner then accepted fifteen
excerpts and requested only an M02 rest/healing revision to quiet flute music.
[Exact partial acceptance](accepted-fifteen.json) freezes those fifteen clips, edits
and routing; [the revision request](m02-flute-revision/owner-request.json) authorizes
the replacement source without changing the frozen source plan. Seventeen containers
are now retained, including the rejected rest source: 227,891,777 original bytes,
fifteen declared CC0 sources and two attribution sources. Eight Freesound originals
were downloaded through the owner's authenticated browser. No credentials were collected.

The complete sixteen-excerpt series is in [candidates.json](candidates.json).
All sixteen are owner-accepted, including revised M02; the other fifteen remain unchanged. All have
explicit source hashes/windows, sample timing, filters, gains, fades and
PCM24 exports at 48 kHz. Three weather loops and one wind/rain/creak loop are thirty
seconds each; four ten-second walking drafts preserve six separated source contacts
each. Six eight-second action/feedback drafts and two twenty-five-second music drafts
complete the series. A06 keeps its Effects and Dialogue components separate for
single routing. Material and anatomical ownership are unassigned.

[Provenance](provenance.json) retains original containers, catalogue captures, licence
deeds, exact archive members and actual codec/depth metadata. Archive paths and
notices were inspected without executing contents. [Credits and edit notices](CREDITS.txt)
accompany both attribution sources and list all source authors. Conversion from MP3
or Ogg to PCM does not restore lost source information. Cotton is actually 32-bit
float; mortar is 32-bit integer. [Decoder checks](source-decoding.json) cover all
twenty-two used direct recordings or pack members; unused pack members have metadata
inspection only. Near-full-scale samples are reported rather than interpreted as an
audible clipping diagnosis.

The renderer uses a disposable project and a byte-identical copy of the accepted
`game/AudioBusLayout.tres`. Four category tracks follow the source plan's five-minute
timeline, with Music omitted only in the music-free version. Reduced range enables
the existing Master compressor; it introduces no new preset. Speech is absent.
Review exports use PCM24; Godot loads the category WAV inputs as 16-bit internally,
captures final Master output as PCM32, then ffmpeg converts it to PCM24 without gain
processing. This conversion and each raw capture hash are recorded.

Final Default measures −24.99 LUFS, 7.5 LU LRA and −6.63 dBTP. Reduced range measures
−37.72 LUFS, 4.8 LU LRA and −19.04 dBTP. Music-free measures −25.10 LUFS, 7.6 LU LRA
and −6.63 dBTP. Default loudness and reduced-range LRA meet their provisional bands;
Default LRA is 0.5 LU below its provisional minimum, and music-free is 0.1 LU below
the Default loudness reference band. Reduced range is substantially quieter at the
same gains. The review page offers an optional +12.7 dB preview gain to compare its
texture at approximately matched loudness; the retained WAV, preset and measurements
are unchanged. This control starts off and does not start playback.

[Measurements](measurements.json) distinguish numerical evidence from hearing.
Excerpt peaks, four-times oversampled peak estimates, mono fold-down, seam deltas,
and literal contact gaps are recorded. The oversampled excerpt estimate is not a
certified true-peak meter. Full-mix LUFS-I, LRA and true peaks use ffmpeg loudnorm's
input fields on actual final output; no normalization filter output replaces the
review files. Targets remain provisional; deviations must be retained honestly.
No short transient is normalized to an integrated LUFS target.

[Nineteen real material output checks](material-controls/measurements.json) pass:
four populated category buses each at unity, half gain, zero and mute; Master zero
and mute; and normalized mono output. These use a disposable fixture with actual
source-derived PCM. Settings persistence and optional/required effect ducking reuse
the unchanged accepted product evidence; no new settings/controller implementation
is claimed. [Final comparison](review/comparison-checks.json) confirms bit-identical
nonmusic output before 4:15 in Default/music-free and exact music-free silence after
that point. The final source references match all twenty-two earlier decoder inputs.

The owner's listening acceptance covers fifteen original excerpts. The revised
M02 motif and complete series are now accepted in [terminal owner acceptance](owner-acceptance.json):
“Default range seems good. The shakuhachi track is suitable. All accepted”. Default
range is selected. No listening gate remains and accepted sounds must not be reviewed again. The agent has not assessed incidental people/modern noise,
material fit, audible seams or device listening. No specific headphones/speakers,
stereo/mono or low-volume assessment is inferred from the owner's general acceptance. Speech
masking/intelligibility remains unassessed. Accepted policy, buses, artwork, gait and
ordinary runtime files are carried forward unchanged.

The M02 replacement uses [Shakuhachi flute play by gonzooka](https://freesound.org/people/gonzooka/sounds/486487/),
declared CC0 on its retained catalogue capture. The original is 16 kHz, 16-bit mono;
resampling to 48 kHz does not restore bandwidth. Instrument identity is uploader-labelled,
without independent acoustic authentication. The contiguous 30–55 second passage uses
two-second edge fades and a uniform −28 dBFS RMS target, with no pitch change, synthesis,
reverb or composition. The edited excerpt measures −28.00 dBFS RMS and approximately
−12.92 dBFS four-times oversampled peak. Mono is centred identically in both channels.
The old M02 excerpt and prior manifests/scripts remain in `m02-flute-revision/`.

[Scoped revision verification](m02-flute-revision/verification.json) checks all fifteen
accepted excerpt bytes, edits/routing and unchanged measurement records. Three nonmusic
tracks, the music-free final capture and meter, and all nineteen material-control probes
reuse their exact preceding evidence. Only the final Music segment at 4:38 and the
Default/Reduced range captures are updated. The Music track before 4:38 was checked
against the original encoded PCM before replacement. No product or CI inputs changed.

The following commands document the completed revision. The slice is now closed;
do not rerun preparation or renders over accepted material without a new owner edit request:

```sh
python3 audio-direction/material-audition/revise-rest-flute.py
python3 audio-direction/material-audition/render-audition.py --revise-m02
python3 audio-direction/material-audition/measure-mixes.py --revise-m02
python3 audio-direction/material-audition/verify-flute-revision.py
python3 audio-direction/material-audition/build-review.py
python3 audio-direction/material-audition/serve-review.py
```

The original whole-series preparation is guarded against overwriting accepted clips.
The retained `executed-render-audition.py` is the exact source of the captured run; the
current renderer relaxes only its Music-file retry guard while retaining the encoded
prefix check and exact nonmusic guards.

The final command serves [the listening page](http://127.0.0.1:8777/) on loopback only,
with byte ranges for working timeline seeking. The page verifies nineteen audio
players with expected durations and no media errors; shared seeking and optional
preview-gain restoration were checked without claiming human listening. The page
and preview do not write an owner decision. Do not rerun preparation over an accepted candidate selection without a new edit request. The review builder preserves the existing owner-selection record.

The graphical Godot renderer needs the host; its sandbox attempt aborted with
SIGABRT. The four-second tooling check retained a 164-sample initial latency and
0.99999994 aligned correlation to the actual input. Godot logs an ObjectDB cleanup
warning on exit; it reported no script or loading errors. PNG frames are disposable
transport and removed after each successful capture. Movie frame statistics skip
unchanged frames; actual retained WAV duration is independently checked.

Downloads retain 227.9 MB of originals. A conservative allowance for browser retries
brings the accounted upper bound to 299.4 MB, below the 500 MB source-download cap;
browser retry traffic is not independently metered. Package storage is checked
against the 2 GB cap. New Git publication is outside this acquisition grant.

Whole-slice terminal acceptance supersedes historical pending/false fields in the frozen
technical candidate, measurement and revision records. Those fields are preparation history,
not new selection gates. Runtime integration needs a separately bounded implementation
scope; the current grant and acceptance do not install any sound in Main.
