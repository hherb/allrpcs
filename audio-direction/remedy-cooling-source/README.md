# Cooling confirmation source — qualified

The owner approved the concrete source/event/runtime scope with **approved**, accepted the
padded audible reference with **Approved**, then chose **The louder version is more appropriate**.
[Authority](../remedy-cooling-runtime-authority.json) binds those decisions and the exact final
C01. Unity original gain replaces the provisional −30dBFS ceiling; the final −12.229845dBFS
sample peak is 0.13dB below the approved reference.

The retained original is Joseph SARDIN's **Finger clashes**, sound #0483:
[catalogue](https://bigsoundbank.com/finger-clashes-s0483.html),
[original WAV](https://bigsoundbank.com/UPLOAD/bwf-en/0483.wav), retrieved 2026-10-12.
[originals/0483.wav](originals/0483.wav) is lossless mono PCM16 at 48kHz, 752,448 frames,
SHA256 `337c1de855dafb8fbd528675e85393e0bd58d40ae563644934c6fcc251d5a6c5`.
Source-specific catalogue/licence and original BWF copyright metadata declare CC0 /
WTFPL / public domain. The source was free, with no account or purchase.

[Catalogue](rights/catalogue.html.gz), [licence](rights/licence.html.gz) and
[CC0 deed](rights/deed.html) retain exact capture hashes. Deterministic gzip preserves
original catalogue/licence bytes including trailing whitespace;
[storage metadata](rights/capture-storage.json) binds raw and stored hashes.
Delivered credits preserve creator/title/source/deed/edit notices without implying endorsement.

[Provenance](provenance.json) binds original and decoded samples, the exact contiguous
0.10–0.85s window, tools/versions, unity gain, 10ms onset and 50ms ending linear fades,
identical mono duplication and final stereo PCM24. There is no resampling, normalization,
loop, synthetic layer or amplification. Final C01 is 36,000 frames at 48kHz,
SHA256 `0eaca55cb79f4747694089fa5f84a4a34650e959debc0941c236d21d4aadb121`.
Installed [C01.wav](../../game/assets/audio/remedy-cooling/C01.wav) equals the qualified
candidate exactly. Native import is uncompressed PCM16, with no normalize/trim/loop.

Reproduce with `python3 scripts/prepare-remedy-cooling-passage.py`; `--install` requires
recorded qualification of this exact derivative/window and the explicit louder unity-level
choice. Measurements establish technical properties; source suitability comes from the
owner's listening and level decisions. No agent acoustic review or fresh-device/ordinary-play
loudness compliance is claimed.

The first −30.1dBFS candidate was reported empty and was not accepted. Its exact bytes survive
as [C01-rejected-quiet.wav](../remedy-cooling-runtime-review/source/C01-rejected-quiet.wav).
The approved +18dB PCM16 reference has 0.5s lead-in and two-second padding for audition only;
it is not the delivered game asset. The original offline H01/S01/quiet-C01 context and its
metadata are preserved as historical evidence: original candidate references resolve against
commit `d9a7d644` and the archived rejected quiet WAV. Final actual rapid H01/S01/C01 overlap
is measured in [native runtime evidence](../remedy-cooling-runtime-review/output/measurements.json).
