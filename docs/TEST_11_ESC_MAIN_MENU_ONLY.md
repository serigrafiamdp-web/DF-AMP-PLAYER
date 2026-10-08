# DF-AMP UI TEST 11 — ESC MAIN MENU ONLY

Status: **PENDING PASS/FAIL**

Base: **MASTER_DFAMP_10_ESC_ONLY_08_OCT_2026 = PASS**

## Single functional change

DF-AMP is rendered only when the current top UI window is the native ESC pause menu:

`DaggerfallPauseOptionsWindow`

Expected behavior:

- ESC opens the native pause menu and DF-AMP appears.
- Clicking **Controls** hides DF-AMP while the controls configuration window is open.
- Returning from Controls to the main ESC pause menu makes DF-AMP appear again.
- The same rule applies to any other submenu pushed above the pause menu.
- ENTER remains completely free for normal Daggerfall Unity use.
- NEXT / `dmnx`, fades, display, track-name HUD, Fast Travel coordination, and playback core remain unchanged.

## Source

Branch: `test/esc-main-menu-only`

Functional source commit: `eae4e541ab26fd60d32d068249d21a3a144c2168`

## Test binary

`DF-AMP UI.dfmod`

- Size: **3,934 bytes**
- SHA-256: `8cc933dc9bcedbd01d2c3582cc5ac907bf2b560813cafa39cbdfbd815343aac1`

Test ZIP:

`DF-AMP_UI_TEST_11_ESC_MAIN_MENU_ONLY.zip`

- SHA-256: `eb1592c1b1e43248650ad86576171d10f31065b4db087167f3bdb276ff62325b`

## Runtime validation

Mark PASS only if:

1. ESC main menu shows DF-AMP.
2. Controls hides DF-AMP.
3. Returning from Controls shows DF-AMP again.
4. ENTER remains free.
5. NEXT and Fast Travel remain unchanged.
