# Allrpcs reference gait model

The owner accepted the complete Seeker cloak gait on 2026-10-07:
“very good - accepted. This is our reference gait model henceforth”.
The [terminal acceptance](seeker-cloak-reference-gait-revision/owner-acceptance.json)
closes the whole gait correction. [Model metadata](reference-gait-model.json) binds
the exact accepted candidate, rig, SpriteFrames resource and acceptance hashes.
Historical “pending” text in frozen packages is superseded by that acceptance.

Use this model for future human walking animation work:

- Pivot both thighs at the painted anatomical pelvis centre; preserve leg identity.
- Allow knees to straighten and flex backward, with fixed authored bone lengths.
- Show heel contact/loading, planted stance, trailing toe-off and lifted bent swing.
- Advance the swing knee first, then the foot as the knee extends for contact.
- Exchange both knees' and feet's leading/trailing roles each half-cycle.
- Ground supporting soles, roll heel to toe and include brief double support.

The accepted cycle is 720ms: sixteen 45ms poses per side and eight 90ms poses each
for the already accepted front/back. Opposite legs are half a cycle apart; the
60/40 stance/swing split is an art timing choice. Future authored cadence and
proportion adaptations use this benchmark while retaining its phase order and
anatomical constraints. These timing values are not clinical measurements.

[Normal-speed imported production preview](seeker-cloak-reference-gait-revision/review/production-sides-clean-2x.webp)
and [half-speed persistent-leg tracking](seeker-cloak-reference-gait-revision/review/knee-foot-transfer-slow-2x.webp)
show the exact accepted gait. The package also retains all 48 editable masters,
actual native/2× Godot captures and passing gait/import/playback evidence.

Preserve the accepted artwork and predecessor evidence. The designation does not
initiate revisions of other characters or Main integration. The gait correction
needs no additional unchanged-view selection, proof or acceptance gate.
