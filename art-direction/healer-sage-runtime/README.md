# Healer and Village Sage runtime art

29 October 2026 (Australia/Sydney). After closing the sickhouse bed/patient slice, the owner
asked for the Healer and Village Sage (Orin) characters next ([authority.json](authority.json)).
This slice installs their owner-approved characters-batch artwork in ordinary Main. No new art
was generated.

![Healer and Sage at rest in Main](captures/village-square-at-rest.png)

## Which frames

The 96 runtime PNGs under `game/art/runtime/characters/{healer,orin}/<direction>/` are byte
copies of the exports that [the characters-batch acceptance](../characters-batch/owner-acceptance.json)
lists in `active_assets`. For each role and each of south/west/east/north there are:

- `idle-01..02` and `inspect-01..02` from `characters-batch/exports/` (approved 2026-10-04);
- `walk-01..08` from `characters-batch/walk-rig-polish/exports/`, the approved 8-phase polished
  walk. The 4-frame `*-walk-*` studies in `characters-batch/exports/` are **not** used: that
  README records they keep the same foot leading and must not be promoted.

## Wiring

Each role has an `animations.tres` SpriteFrames with the twelve animations the Seeker uses:
`idle_*` (600 ms per frame, looping), `walk_*` and `interact_*` (the inspect frames, 160 ms,
not looping). Walks run 90 ms per frame, giving the
[reference gait model](../reference-gait-model.md)'s 720 ms cycle (the same cycle as the
Seeker's runtime walk). The frames themselves stay the approved 8-phase cutout gait. This slice
did not re-author them to the reference model's 16 side poses, because that would be new art the
owner did not request.

- `Healer/WalkAnimation` replaces the `Body` and `Satchel` polygons. It reuses
  `SeekerWalkAnimationPresenter`. `Healer.cs` passes it the chosen `Velocity` before
  `MoveAndSlide`, as `Seeker.cs` does, so the Healer's own movement picks the facing and the
  walk/idle loop.
- `SageHearingPosition/Figure` replaces the Sage `Body` polygon. The Sage never moves, so it
  autoplays `idle_south`. Its label is unchanged.
- Both sprites use offset (0, −96), which puts the feet (the bottom row of the 128×192 frames)
  on the node origin, as the Seeker's are. Positions, collision, navigation, interaction ranges,
  dialogue, commands and saves are unchanged. The Healer still starts the game hidden, away on
  its authored task.

The interaction gesture is not triggered for the Healer: nothing in the game signals a Healer
interaction yet, and adding one would be new behaviour.

![Healer walking east](captures/healer-walking-east.png)

## Checks

- `scripts/validate-healer-sage-runtime.py` checks each runtime frame against the hash in the
  characters acceptance record, checks their lossless import settings, and fails on any extra
  frame under the two runtime folders. It checks only these files (#471).
- `scripts/check-healer-sage-runtime.gd` checks that the placeholders are gone and both nodes
  are unmoved. It checks every animation's frames, timing and looping, and that the Sage is
  breathing. It moves the Seeker away so the Healer walks east, then back so it idles east. It
  fails on the pre-slice Main and when the `Healer.cs` presenter call is removed.
- `OpeningRegionSageConsultationInputSmokeCheck` now checks that `Figure` is visible instead of
  the removed `Body` polygon.
- `scripts/capture-healer-sage-runtime.gd` writes the two `captures/` and
  [capture-evidence.json](capture-evidence.json). Run it without `--headless`.

Results are in [verification.json](verification.json).

## Known limit

The greybox water-cask marker at (2200, 900) draws over the Sage's legs, because the new art is
much taller than the old polygon. Replacing that marker is outside this slice; it is tracked as
[#477](https://github.com/hherb/allrpcs/issues/477).
