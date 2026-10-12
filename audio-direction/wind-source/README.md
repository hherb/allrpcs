# Qualified G01 wind passage

The owner approved this runtime slice on 2026-10-12 and qualified the exact four-second
review candidate with **“sounds right”**. The nominated original's catalogue warns of
bag noise; qualification is the owner's listening assessment, not a signal-only claim.
The exact accepted candidate is SHA256
`01805d78cd1b781cfcf0c7530c9e9b1193d8a3991360a0f36848dde9ebf04788`.

[Mild Wind Background Noise](https://opengameart.org/content/mild-wind-background-noise)
by Bashar3A is listed as CC0-1.0; preserve the supplied credit “Bashar from Skirmish.io”.
[Provenance](provenance.json) binds the retained original, exact creator/deed captures,
owner assessment and installed derivative. No new original was acquired and no prior
B04/F01 excerpt or acceptance was changed.

G01 uses only original frames 529200–705600 (12–16 seconds at 44.1 kHz), deterministic
48-kHz resampling, one -4 dB attenuation and 100 ms linear endpoint fades. It is
192000 stereo PCM24 frames, four seconds, nonlooping. The sample peak is -24.343 dBFS,
RMS -37.113 dBFS and integrated loudness -42.5 LUFS. No filtering, denoising, layering,
pitch change or time stretching occurs. Native import is uncompressed stereo PCM16,
without normalization, trimming or looping; the mounted archive verifies every converted byte.

Reproduce with `python3 scripts/prepare-wind-passage.py`; installation requires the
exact recorded owner-qualified candidate/window. `--preview` is for reproduction of
that same source passage. The review retains decode, resample, PCM and import hashes.
Original and review/decode audio are outside runtime resource exports; only G01 and
its three source-specific notices are added. Controlled measurements do not establish
ordinary-play or fresh-device loudness compliance.
