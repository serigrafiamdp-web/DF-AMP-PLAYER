# DF-AMP UI TEST 10 — ESC ONLY

Status: **PASS**

Base: exact MASTER 09G PASS.

## Single functional change

The DF-AMP UI companion no longer listens to `KeyCode.Return`.

Expected behavior:

- ENTER is completely free for normal Daggerfall Unity use.
- DF-AMP appears only after ESC has opened the native pause menu and `GameManager.IsGamePaused` becomes true.
- Closing the pause menu with ESC or Continue hides DF-AMP.
- Existing Travel Options / Fast Travel coordination remains unchanged.
- NEXT / `dmnx`, display 000-999, fades, native track-name HUD, and playback core remain unchanged.

## Source

Branch: `test/esc-only-ui`

Functional source commit: `42abb41d74c6ee79e53b89b83d3ad5fd53d773ad`

Metadata commit: `b40ddc8f8a2414cd8146737c99ef71f42ec840b1`

## Test binary

`DF-AMP UI.dfmod`

- Size: 3,885 bytes
- SHA-256: `669ea54b953bb0300813569b3de3eadd2921fc37aad43e59a5e1c99021e3253b`

Test ZIP:

`DF-AMP_UI_TEST_10_ESC_ONLY.zip`

- SHA-256: `1fe27c20681e92333ad944913fdabba0739d01d7ab642a0cb237241012f94e8f`

## Runtime validation

Runtime validation passed for all five checks:

1. ENTER performs its normal game function and never opens/closes DF-AMP.
2. ESC opens the native menu and DF-AMP appears with it.
3. ESC or Continue closes the menu and DF-AMP.
4. NEXT still advances one track with the existing fade behavior.
5. Fast Travel behavior remains identical to MASTER 09G.


## Promotion

Promoted to:

`MASTER_DFAMP_10_ESC_ONLY_08_OCT_2026.zip`

- MASTER ZIP SHA-256: `dfdf6f1555ffd178e0857601f6900b35a4fa174742e7580f49256b429f9c8168`
- MASTER runtime `DF-AMP UI.dfmod` SHA-256: `669ea54b953bb0300813569b3de3eadd2921fc37aad43e59a5e1c99021e3253b`
