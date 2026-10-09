# DF-AMP UI TEST 11B — CONTROLS HIDE SAFE

Status: **PASS**

Base: **MASTER_DFAMP_10_ESC_ONLY_08_OCT_2026 = PASS**

## Why TEST 11 failed

TEST 11 changed the uncompressed length of the source TextAsset inside the UnityFS bundle.
The outer bundle sizes were rebuilt, but the serialized asset's internal layout was no longer byte-stable.
Result: Daggerfall Unity did not start.

TEST 11 is therefore **FAIL (technical packaging failure)** and must not be promoted.

## TEST 11B single functional change

DF-AMP is not rendered while:

`DaggerfallUI.UIManager.TopWindow == DaggerfallUI.Instance.ControlsWindow`

Expected behavior:

- ESC main menu: DF-AMP visible.
- Controls: DF-AMP hidden.
- Return from Controls to ESC main menu: DF-AMP visible again.
- ENTER remains free.
- NEXT / fades / HUD / Fast Travel / playback core remain unchanged.

## Safe runtime patch contract

The TEST 11B dfmod preserves the exact internal uncompressed UnityFS node size from MASTER 10:

- MASTER 10 node size: **8,656 bytes**
- TEST 11B node size: **8,656 bytes**

No serialized TextAsset length change is introduced.

## Source

Branch: `test/controls-hide-safe`

Functional source commit: `699b3763558cd006901d0ef2881acaba77915b4d`

## Test binary

`DF-AMP UI.dfmod`

- Size: **3,900 bytes**
- SHA-256: `9eebd1aa61178130cdda0d02822f9875c186416c88892a428fe382ac396d19c0`

Test ZIP:

`DF-AMP_UI_TEST_11B_CONTROLS_HIDE_SAFE.zip`

- SHA-256: `32f3acf51eda8e09571bf8166fa719eb4bb77a609c5a38e8e4c2658f1ebbb029`

## Runtime validation

PASS only if:

1. DFU starts normally.
2. ESC main menu shows DF-AMP.
3. Controls hides DF-AMP.
4. Returning from Controls shows DF-AMP again.
5. ENTER remains free.
6. NEXT and Fast Travel remain unchanged.


## Promotion

Promoted to:

`MASTER_DFAMP_11B_CONTROLS_HIDE_SAFE_08_OCT_2026.zip`

- MASTER ZIP SHA-256: `dd9d956860feeb1343e8d812f57354b34ce8a8d3c930cfea7a6c63aef6eebf47`
- `DF-AMP UI.dfmod` SHA-256: `9eebd1aa61178130cdda0d02822f9875c186416c88892a428fe382ac396d19c0`

Runtime confirmed by the user:
- DFU starts normally.
- ESC shows DF-AMP.
- Controls hides DF-AMP.
