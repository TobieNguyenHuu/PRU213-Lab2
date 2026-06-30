# SnowBoarder Lab 2 Submission Brief

## Game Concept

SnowBoarder is a 2D arcade snowboarding game where the player rides downhill, controls speed, performs flips, uses powerups, and clears three mountain levels before the endgame screen.

## Design Decisions

- The game uses Rigidbody2D, gravity, low-friction snow material, and SurfaceEffector2D to create downhill movement.
- `A/D` or arrow keys steer and rotate the boarder. `W` increases speed, `S` brakes on ground or dives downward in air, and holding `Space` spends boost energy that regenerates when released.
- Rocks are low trigger bump hazards instead of large blocking walls. They slow the player without removing lives, keeping obstacle gameplay fair.
- Trees are treated as scene-authored background scenery with collision disabled so they do not interfere with gameplay.
- Generated jump ramps were disabled after playtesting because they did not visually fit the hand-authored scene layout.
- Tricks, powerups, speed, and finish bonuses contribute to score.
- Combo and multiplier systems reward consecutive successful actions.
- The game includes Menu, Level1, Level2, Level3, and EndGame scenes.

## Implemented Features

- Main menu with Start, Options, and Quit.
- In-game HUD with score, speed, lives, combo, multiplier, and status messages.
- Endgame screen with final result, score, best combo, retry, main menu, and quit.
- Three playable levels with campaign flow: Menu -> Level1 -> Level2 -> Level3 -> EndGame.
- Powerups, finish line, falling snow weather, and camera follow.
- Powerups are placed from a shared track guide so they line up better above the slope.
- Powerups include speed boost, invincibility, and shortcut impulse.
- Leaving the terrain bounds ends the run as a loss.
- Background images are used for Menu and EndGame scenes.
- Background music changes by scene: menu music, gameplay music, and endgame music are separate.

## Challenges Faced

- Movement needed several tuning passes because the character initially felt too slow and air braking made the boarder fall too slowly.
- Runtime-generated objects were convenient but made manual scene editing confusing, especially for Level2 and Level3.
- Runtime snowflakes and checkpoints were disabled after playtesting because their generated placement caused visual bugs.
- Large tree and rock sprites could block the slope too much, so tree collision was disabled and rocks were reduced to low trigger bump hazards.
- Finish and endgame flow needed adjustment after adding multiple levels.

## Additional Features

- Multiple mountain scenes were added as optional extra content.
- Falling snow creates a winter atmosphere.
- Menu and EndGame scenes use full-screen background images.
- Separate music tracks are used for menu, gameplay, and endgame flow.

## Future Improvements

- Add more sound effects and volume controls if more polish time is available.
- Add hand-placed collectibles/checkpoints later if the scene layout is finalized.
- Add more hand-designed ramp layouts and landing-zone visuals.
- Polish the current boost energy bar and add a pause menu overlay.
