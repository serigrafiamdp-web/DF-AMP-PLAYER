# DF-AMP UI TEST 11 — FAIL

Status: **FAIL — technical packaging failure**

Base: **MASTER_DFAMP_10_ESC_ONLY_08_OCT_2026 = PASS**

## Intended change

Render DF-AMP only while the top window was `DaggerfallPauseOptionsWindow`.

## Failure

Daggerfall Unity did not start.

## Confirmed cause

The direct UnityFS patch increased the uncompressed source TextAsset length. The outer UnityFS block sizes were rebuilt, but the serialized asset layout was no longer byte-stable.

This test must **not** be reused or promoted.

The corrected implementation is **TEST 11B**, which preserves the original internal node/TextAsset size and hides DF-AMP specifically while the Controls window is the top window.
