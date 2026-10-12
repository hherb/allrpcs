# Cooling confirmation source — listening pending

The owner approved the concrete source/event/runtime scope on 2026-10-12 with
“approved”. [Authority](../remedy-cooling-runtime-authority.json) retains the grant
and the frozen [proposal](../remedy-cooling-owner-scope.md).

Selected original: Joseph SARDIN, **Finger clashes**, sound #0483. Catalogue:
https://bigsoundbank.com/finger-clashes-s0483.html . Exact original:
https://bigsoundbank.com/UPLOAD/bwf-en/0483.wav . Retrieved 2026-10-12; retained as
[originals/0483.wav](originals/0483.wav). Source-specific catalogue and licence pages
explicitly declare CC0; the original BWF copyright metadata also declares CC0 /
WTFPL / public domain. [Catalogue](rights/catalogue.html.gz), [licence](rights/licence.html.gz)
and [CC0 deed](rights/deed.html) are retained with their exact capture hashes.
Catalogue/licence captures use deterministic gzip to preserve their unmodified
bytes, including trailing whitespace; [storage metadata](rights/capture-storage.json)
binds compressed and original hashes.
The source is free and requires no account or purchase. Future runtime packaging
will retain creator/title/source/deed and edit notices without implying endorsement.

The downloaded original is lossless mono PCM16, 48 kHz, 752,448 frames. Catalogue
“light finger snaps” is a selection clue, not acoustic qualification. The first
measured transient occurs inside the selected contiguous 0.10–0.85-second window.
No agent listening/acoustic suitability or fresh-device compliance is claimed.

[Provenance](provenance.json) binds the original, decoded samples, exact window,
tools/versions, attenuation, fades and final candidate. Reproduce with:

```sh
python3 scripts/prepare-remedy-cooling-passage.py
```

The separate [candidate](../remedy-cooling-runtime-review/source/C01-candidate.wav)
is 0.75 seconds / 36,000 frames, 48 kHz stereo PCM24. Edits: exact contiguous trim,
10-ms onset / 50-ms ending linear fades, one attenuation-only gain of approximately
−17.870155 dB, and identical mono duplication. No resampling is needed. Measured
sample peak −30.099985 dBFS, integrated loudness −57.0 LUFS. Short transient loudness
is descriptive, not a delivery-standard or audibility claim.

The [offline context preview](../remedy-cooling-runtime-review/source/H01-S01-C01-context.wav)
illustrates rapid heating at 0 seconds, steeping at 0.6 seconds and cooling at
1.2 seconds. Its inputs are byte-identical accepted H01/S01 and exact C01 at unity
gain/pitch; [the context record](../remedy-cooling-runtime-review/source/context-preview.json)
binds inputs, offsets and output. It is an offline review illustration, with no
scene installation, native-output or ordinary-play evidence. The C01 derivative
itself is never layered.

The exact isolated candidate and context preview were presented inline for owner
listening. Acceptance is pending. Technical format/mono/fades/peak/reproduction
checks pass, and `--install` refuses unqualified audio before writing any product
bytes. No `game/assets/audio/remedy-cooling` directory or cooling runtime exists.
The existing recipe, station, art, accepted audio, product/CI inputs and local
`game/project.godot` editor state remain unchanged. Do not install, choose new
window bytes or record qualification from silence; wait for the owner's exact
candidate response. A rejection requires another source/window within the grant,
not relaxation of its contract. Full runtime quality remains a dependent step.
