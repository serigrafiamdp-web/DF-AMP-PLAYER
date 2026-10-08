# DF-AMP PLAYER — Current PASS State

**Date:** 2026-10-07  
**Current baseline:** `MASTER_DFAMP_09G_07_OCT_2026.zip`  
**Status:** PASS / MASTER  
**Previous historical master:** 07N

## Project identity

Original playback engine: **Dynamic Music** by Numidium3rd / numidium.  
Extended player project: **DF-AMP PLAYER BY RICO**.

## Source archival status

The exact compiled core embedded in the current MASTER 09G `DF-AMP.dfmod` has been extracted, SHA-256 verified, and decompiled with ILSpy 11.1.

- Embedded core SHA-256: `01dd2d8df9900b8a4fe54ceb0698bd1ad00966e8e30fe11fccdc6803e7d7734a`
- Recovered core source: `src/Core/DynamicMusic.cs`
- Recovered song-player source: `src/Core/DynamicSongPlayer.cs`
- Recovery notes: `src/Core/DECOMPILATION.md`
- Untouched upstream source remains preserved under `upstream/DynamicMusic/`

The compiled MASTER 09G core is the byte-level authority. Decompiled source is the semantic source archive because the original modified source/PDB was not present in the MASTER package.

## Validated playback contract

- A dedicated WALKMAN-style custom playlist is used outside the Start Menu.
- Sequential playback is validated when RANDOM is off.
- RANDOM can be toggled from the mod settings.
- Native FadeOut → next track → FadeIn auto-next behavior is preserved.
- Custom command `dmnx` advances to the next track using the same fade behavior.
- NEXT in the DF-AMP player invokes `dmnx`.

## UI contract

- Native in-game UI, not an external WinForms window.
- Horizontally centered and attached to the bottom edge.
- Overall scale: 75%.
- Uses Daggerfall Unity's native cursor.
- ENTER toggles DF-AMP outside the ESC pause menu.
- ENTER is blocked while the native ESC menu is active.
- Dynamic numeric display is 000–999.
- Display reads the current playlist object's internal `index` and shows `index + 1`.
- Display refresh interval: 0.2 seconds.
- `currentCustomTrack` is a string and must not be treated as the numeric index.
- Track filename changes are announced through `DaggerfallUI.AddHUDText(..., 4)`.

## ESC / Continue / Fast Travel contract

- ESC waits for the real `GameManager.IsGamePaused` state before synchronizing DF-AMP.
- Closing the pause menu with ESC or Continue closes DF-AMP.
- During Travel Options / Fast Travel, transient travel-state changes caused by the ESC menu must not be interpreted as a real travel exit.
- A real Fast Travel exit closes DF-AMP and releases `PlayerMouseLook.cursorActive` when needed.
- If a path-following travel session was active before ESC and Continue leaves travel inactive, DF-AMP invokes `FollowPath()` by reflection to resume the session.
- This behavior was validated in TEST 07N and remains part of MASTER 09G.

## Critical 09G debug/NEXT bridge contract

`DefaultCommands.showDebugStrings` is part of the inherited signal bridge used by `tdbg -> dmnx`.

Therefore:

- Do **not** permanently force `showDebugStrings = false`.
- Do **not** remove its toggle.
- Doing so breaks NEXT / `dmnx`.

MASTER 09G keeps the internal toggle intact but forces the value distributed to visual debug destinations to false.

Validated binary patch in `Assembly-CSharp.dll`:

- File offset: `0x1582F3`
- Original bytes: `7E F7 02 00 04`
- MASTER 09G bytes: `16 00 00 00 00`

Result:

- NEXT works.
- `dmnx` works.
- FadeOut/FadeIn works.
- Dynamic display works.
- Native HUD track name works.
- Residual debug overlays no longer appear.

## PASS milestones

- Dynamic Music / DF-AMP core 08A — PASS
- UI 03 native cursor — functional PASS
- UI 04 vertical flip correction — PASS
- UI 05 bottom 75% + NEXT — PASS
- UI 06B dynamic display — PASS
- UI 07N Fast Travel resume integration — PASS / historical master
- UI 09B native HUD track name — PASS
- UI 09G signal kept / visual debug off — PASS / current master

## Discarded branches

09C / 09D / 09E / 09F must not be used as new baselines. They attempted to suppress the debug toggle itself or interfere with its signal and caused UI/NEXT regressions.

## Development rule

New work branches from MASTER 09G unless an explicit decision documents another base. Every promoted release should keep this project-state document updated.
