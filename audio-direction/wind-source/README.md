# Retained wind source preparation

The owner approved the active wind runtime implementation on 2026-10-12. See
[grant](../wind-runtime-authority.json) and the unchanged
[proposal](../wind-owner-scope.md). This source package preserves the nominated
original by reference and exact SHA256; no new original was acquired or modified.

“Mild Wind Background Noise” by Bashar3A is listed as CC0-1.0 in the retained
creator capture and was checked live on 2026-10-12. Preserve the supplied credit
“Bashar from Skirmish.io”. Captured catalogue and deed are in `rights/`.

The catalogue warns of random bag sounds. The 12–16 second window is a review
candidate selected by lower, steadier signal measurements. No listening pass is
claimed. The owner's required assessment is pending; see
[review](../wind-runtime-review/README.md) and `provenance.json`.

`python3 scripts/prepare-wind-passage.py --preview` reproduces the review-only
four-second stereo 48 kHz PCM24 WAV: exact contiguous source frames 529200–705600
at 44.1 kHz, deterministic resampling, one -4 dB attenuation and 100 ms linear
endpoint fades. No filtering, denoising, layering or time/pitch changes occur.
Measured sample peak is -24.3426 dBFS; integrated loudness is -42.5 LUFS. These
measurements do not establish ordinary-play or device listening compliance.

Without `--preview`, preparation refuses runtime installation until provenance
contains a passed, matching, hash-bound owner listening assessment. Preview
bytes, source originals and review measurements remain outside runtime exports.
