# Lab 2 Checklist - Snow Boarder

- [x] Player controls a snowboarder with left/right rotation.
- [x] Speed control supports brake input and energy-based Space boost.
- [x] Rigidbody2D, gravity, collision, surface effector, low-friction snow material, and slope assist are used for downhill physics.
- [x] Rocks are configured as low trigger bump hazards that slow the player without costing lives; trees keep their scene-authored positions and have no collision.
- [ ] Jump ramps are disabled because the generated ramp visuals did not fit the scene layout.
- [ ] Collectible snowflakes are disabled because generated placement caused visual bugs.
- [x] Landing rotation logic adds trick score.
- [x] Runtime HUD shows score, speed, lives, combo, multiplier, and messages.
- [x] Separate menu scene includes Start, Options, and Quit buttons.
- [x] Separate endgame scene displays result, score, best combo, and retry/menu/quit actions.
- [x] Head/ground crashes cost lives; rock trigger bumps only slow the boarder and reset combo.
- [x] Leaving the playable terrain bounds ends the run as a loss.
- [x] Trigger pickups include speed boost, invincibility, and shortcut impulse.
- [x] Score is based on speed over time, powerups, tricks, and finish bonus.
- [x] Combo multiplier grows from consecutive successful powerups/tricks and times out.
- [x] Falling snow weather effect is created at runtime.
- [x] Background music is assigned per scene: menu, gameplay levels, and endgame.
- [x] Camera follows the player even without Cinemachine installed.

Optional items still suitable for future expansion:

- [ ] Multiple playable characters with unique stats.
- [x] Multiple mountain scenes.
- [ ] Time-trial leaderboard mode.
- [ ] Custom boards and outfits.
