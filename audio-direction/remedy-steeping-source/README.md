# Remedy steeping source candidate

The owner approved the finite source/runtime scope with “approved.” on 2026-10-12.
The first Water Waves candidate was rejected: “S01 candidate sounded more like someone
wading in water or entering a river.” Its original, derivative and measurements are
retained in `../remedy-steeping-runtime-review/source/rejected-wave02-*`.

The replacement is Joseph SARDIN’s **Water bubbles (#0150)**, a real mono studio
recording made with a straw in a mug, downloaded without an account from
https://bigsoundbank.com/UPLOAD/bwf-en/0150.wav. The exact source page and licence
page document CC0 1.0; captured pages and the CC0 deed are under `rights/`.
The lossless original is PCM16, 44.1 kHz, mono, 26.099796 seconds. A WAV container
alone is not the qualification evidence: ffprobe metadata, embedded PCM coding history,
source description and hash are retained in `provenance.json` and the authority record.

The exact contiguous 0–3s window decodes without clipped window samples, resamples to
48 kHz, duplicates mono identically to stereo, receives 100ms linear fades and one
attenuation-only gain, and is delivered as three-second non-looping PCM24. Reproduce
it with `python3 scripts/prepare-remedy-steeping-passage.py`.

The owner listened to the exact replacement presented inline and accepted it with
**“Yes, this is suitable”** on 2026-10-12. The runtime authority binds that acceptance
to the exact derivative SHA256 `90576dd39afd078609ce5535cd6319edc79c99f30b197a54d4622288e9c73b9a`
and 0–3s window. Measurements establish technical format/length/peak only; no agent
acoustic or fresh-device assessment is claimed. The script checks that exact listening
binding before `--install`. Installed `game/assets/audio/remedy-steeping/S01.wav`
is byte-identical to the owner-accepted candidate. No source processing beyond the
approved trim, resample, identical mono duplication, fades and attenuation was added.
