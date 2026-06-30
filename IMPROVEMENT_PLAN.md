# SnowBoarder Improvement Plan

Updated: 2026-06-20

## Current Direction

- Keep scene-authored objects stable: do not move trees, rocks, or decorations at runtime.
- Avoid generated snowflakes, checkpoints, ramps, and trick zones until the map layout is final.
- Keep runtime generation limited to safe systems: powerups, HUD, camera follow, weather, music, and scene flow.
- If a feature causes visual bugs in Scene/Game sync, remove it or convert it to hand-placed scene objects.

## Priority 1 - Safe Gameplay Fixes

- Tune boost energy drain/recharge after playtesting if Space feels too strong or too weak.
- Tune speed per level after playtesting: Level1 easy, Level2 faster, Level3 more challenging but still controllable.
- Keep rocks as non-lethal bump hazards: slow/reset combo only, never remove lives.

## Priority 2 - UI And Feedback

- Add a pause overlay with Resume, Restart, Main Menu, and Quit.
- Optionally polish the current boost energy bar with stronger colors or icon labels.
- Add click sound for Menu/EndGame buttons.
- Add clearer messages for rock bump, boost, invincibility, trick landing, finish, and game over.

## Priority 3 - Scene Polish

- Polish Level2 and Level3 manually in the Scene view, then save with `Ctrl+S` before pressing Play.
- Use hand-placed collectibles/checkpoints only if needed; do not regenerate them by code.
- Add decorations only as no-collision background objects unless they are intentionally part of gameplay.
- If ramps are needed for Lab scoring, create them manually in the scene so Scene view and Game view match.

## Priority 4 - Submission Quality

- Run through Menu -> Level1 -> Level2 -> Level3 -> EndGame once before submission.
- Confirm Menu Quit and EndGame Quit stop Play Mode in the Unity Editor.
- Confirm trees do not move and rocks do not cost lives.
- Confirm no generated snowflakes/checkpoints/ramps appear.
- Keep `progress.md`, `LAB2_CHECKLIST.md`, and `SUBMISSION_BRIEF.md` consistent with the final build.
- Record a short gameplay demo showing menu, movement, boost, rock bump, tricks, level transition, endgame, and quit.
