# Dedicated foliage source

One original, **WIND AND LEAVES** by SamuelGremaud, was downloaded from its
[creator catalogue](https://freesound.org/people/SamuelGremaud/sounds/544844/)
on 2026-10-11 through the existing authenticated browser. The catalogue declares
CC0 and links the CC0-1.0 deed. Trailing line whitespace was removed from the
catalogue HTML; both retained captures and their exact hashes are recorded in
[provenance.json](provenance.json). No credentials were inspected, no account was
created and no other original was acquired.

The owner listened to the recording and confirmed a qualifying clean three-second
passage starting at 0:08. The recorded assessment excludes voices, wildlife, rain,
footsteps, handling and machinery. The original has 814,685 stereo PCM24 frames at
48 kHz with zero clipped samples. The agent verified samples and rights; the
listening assessment is the owner's.

F01 uses exactly frames `[384000,528000)`, retains the original channels/rate, adds
100 ms linear fades and applies one uniform -5 dB gain. Its sample peak is
-18.36765 dBFS and integrated analysis is -34.69 LUFS. Preparation makes no other
edits and performs no normalization or boost. The provisional sample-peak ceiling
is separate from true-peak and ordinary-play/device compliance. Run
`python3 scripts/prepare-foliage-passage.py` to inspect, or
`python3 scripts/prepare-foliage-passage.py --start-frame 384000 --gain-db -5`
to reproduce the exact derivative. Matching original, rights and listening
records are required; seven rejected-source/preparation probes and exact byte
reproduction are recorded in the runtime review.

Only F01 is installed. Source-specific attribution, derivative changes, provenance
and the deed are packaged in its own notice group; all prior PCM and notices stay
unchanged. The creator's invitation to hear use of the recording is retained as
an acknowledgement; no external message was sent.
