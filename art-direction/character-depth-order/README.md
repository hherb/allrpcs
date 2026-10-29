# Character depth order

The owner chose this slice on 2026-10-29 ([authority.json](authority.json)). It fixes two draw-order
defects that appeared once the tall Healer and Sage art was installed (PR #478):

- [#479](https://github.com/hherb/allrpcs/issues/479): Main drew its characters in tree order, so
  the Seeker always drew on top, even when it stood behind the Healer or the Sage.
- [#477](https://github.com/hherb/allrpcs/issues/477): the greybox water cask at (2200, 900) drew
  over the legs of the Sage, who stands south of it at (2240, 950).

## What changed

- `src/Application/Presentation/DepthDrawOrder.cs` is a pure function. It ranks items by the Y of
  their feet; the item furthest south gets the highest rank, and equal Y keeps list order.
  `tests/Application.Tests/DepthDrawOrderTests.cs` covers the ordering, ties and an empty list.
- `game/CharacterDepthOrderPresentation.cs` is a node in Main. Every frame it ranks
  `SageHearingPosition`, `InteractionCandidates/WaterCask`, `Healer` and `Seeker` by global Y
  and sets their `z_index` to `1 + rank` (so 1 to 4).
- `game/Main.tscn`: the Sage's "VILLAGE SAGE · K" label was relative z 2, which kept it above the
  characters while the Sage sat at z 0. It is now absolute z 5, so a character ranked up to 4 never
  covers it.

No position, movement, navigation, collision, interaction range, dialogue or save changes. The
sprites themselves (`WalkAnimation`, `Figure`) keep `z_index` 0, so their own checks are unchanged.

## Why not y-sort Main

Godot's `y_sort_enabled` sorts a node's children by their own Y. Main's characters are direct
children of Main, so the only native option was y-sorting the whole Main node. That reorders every
child drawn at z 0 by its origin, and the scene sources are positioned at their sites. For example, the
sickhouse scene source sits at y 1390, so its bed and patient could draw over the Seeker where they overlap
in the cutaway. Moving the characters under a shared y-sorted parent would change node
paths used by about 280 files. Ranking only these four is the smallest change that fixes both issues.

Every other world drawing in Main is either earlier in the tree at z 0 or at negative z (ground,
channel, structures and cutaway floors), so starting at z 1 keeps all four drawn above it, as the
Healer and Seeker already were by tree order. The Sage and cask, earlier in the tree, now also draw
above later z-0 scene props; the Sage's label is the one positive z in Main and is lifted above all four.

## Checks and evidence

- `scripts/check-character-depth-order.gd` (in `scripts/smoke-godot.sh`) loads ordinary Main,
  holds both characters, and checks that the four have distinct z 1..4 and that the Sage draws in
  front of the cask. It then places the Seeker north and south of the Healer and north of the
  Sage, and checks each time that the one further south draws in front. Finally it places both
  characters just south of the Sage and checks that its name label still draws above them. It fails on Main without
  the presenter (`[0, 0, 0, 0]`).
- `scripts/capture-character-depth-order.gd` writes the two `captures/` and
  [capture-evidence.json](capture-evidence.json). Run it without `--headless`.
  - `captures/seeker-behind-sage-at-cask.png`: the Sage in front of the cask and of the Seeker.
  - `captures/seeker-behind-healer.png`: the Healer in front of the Seeker 72 px north of it,
    where a follow stops.

Results are in [verification.json](verification.json).

## Known limits

- Only these four are depth-ordered. Other markers, scene-source props (bed, preparation table,
  herb beds) and buildings keep their existing order relative to the characters.
- The cask's origin is its centre, not its base, so a character whose feet are between the cask's
  centre and its front edge (y 900–922) draws in front of it.
- The cask's label and focus ring inherit its z, so they now draw under the Sage figure where they
  overlap, as the cask itself does (visible in `captures/seeker-behind-sage-at-cask.png`).
