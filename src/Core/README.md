# DF-AMP Core

The DF-AMP core is derived from **Dynamic Music** by Numidium3rd / numidium.

Original upstream source:

- Repository: `numidium/dfu-mods`
- Folder: `DynamicMusic`
- Daggerfall Unity target: 1.1.1
- Upstream license: MIT

## Current MASTER 09G source

The exact compiled .NET core embedded inside the validated `DF-AMP.dfmod` from **MASTER_DFAMP_09G_07_OCT_2026.zip** was extracted and independently verified.

Embedded core assembly SHA-256:

`01dd2d8df9900b8a4fe54ceb0698bd1ad00966e8e30fe11fccdc6803e7d7734a`

That exact assembly was decompiled with ILSpy 11.1 and its recovered C# is archived here:

- `DynamicMusic.cs`
- `DynamicSongPlayer.cs`
- `DECOMPILATION.md`

The compiled assembly remains the byte-level authority for MASTER 09G. Decompiled C# preserves program semantics, but original source comments, formatting, and some local variable names cannot be recovered without the original source/PDB.

The untouched upstream Dynamic Music source is separately preserved under `upstream/DynamicMusic/` for provenance and comparison.

Future DF-AMP source changes should be committed before building new runtime binaries.
