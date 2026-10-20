# Phase 4 item 1: new-game flow

Owner authority: `owner-playable-phase4-2026-10-19` in `CONTINUATION.json` ("please continue
working towards a playable version of this game"), under program Phase 4 item 1 in
[the program plan](2026-10-17-code-health-and-playable-slice.md).

## Measured state (at `codex/phase3-thin-main`)

- Customization is complete and in `Main.tscn`: `SeekerCustomizationMenuController`
  (`SeekerCustomizationOverlay/Panel`) collects name, pronouns, appearance and background, and
  `SeekerCustomizationApplicationCoordinator.TryApply` installs the profile, the route variation and
  every background access token atomically. Selection is one-shot.
- The background already gates a route-access consumer at runtime: Forester opens the closed
  forest path (`OpeningRegionClosedForestPathSceneSource.TryInstallAccess` disables the `ClosedGate`
  collision and hides its line and marker).
- Gap: the menu opens only on `open_seeker_customization` (U), so a new player can skip slice
  step 1 entirely.

## Change

1. `SeekerCustomizationMenuController.Open()` holds the existing open path (show, suspend the
   Seeker for the modal, focus the name field); the U handler calls it unchanged.
2. `Main._Ready` ends by opening the menu for a new game when Main is the current scene and no
   SmokeCheckHost gate is requested (`SmokeCheckHost.SuppressesNewGameStart`). Proof scenes and
   `--script` runners that embed Main, and every gated check, keep their current start state.
   Exports never attach checks, so a shipped build always opens it.
3. `NewGameCustomizationStartSmokeCheck` (host entry, `ALLRPCS_NEW_GAME_CUSTOMIZATION_CHECK`, the
   only gate exempt from suppression) proves a plain start shows the menu with the Seeker suspended
   and the name field focused, and that confirming Forester hides the menu, resumes the Seeker and
   opens the closed forest gate. Registered in `scripts/smoke-godot.sh`.

Out of scope: saving customization and continue-on-launch (item 2), the three stored-but-unread
access tokens, any save-schema change.

## Verification

`dotnet format`/`build`/the four test projects, the new check, the customization, background and
modal-input launches, and then the full smoke suite.
