# DF-AMP Core

The DF-AMP core is derived from **Dynamic Music** by Numidium3rd / numidium.

Original upstream source:

- Repository: `numidium/dfu-mods`
- Folder: `DynamicMusic`
- Daggerfall Unity target: 1.1.1
- Upstream license: MIT

## Current MASTER 09G source

The exact compiled .NET core embedded in the validated `DF-AMP.dfmod` from
**MASTER_DFAMP_09G_07_OCT_2026.zip** was extracted and verified before
decompilation.

Embedded core assembly:

- Size: **35,840 bytes**
- SHA-256: `01dd2d8df9900b8a4fe54ceb0698bd1ad00966e8e30fe11fccdc6803e7d7734a`

The exact PASS assembly payload is preserved losslessly as Base64 in:

- `tools/core-full.b64`

Its editable ILSpy reconstruction is preserved under:

- `src/Core/Decompiled/DynamicMusic/DynamicMusic.cs`
- `src/Core/Decompiled/DynamicMusic/DynamicSongPlayer.cs`
- `src/Core/Decompiled/DF-AMP_Core.csproj`
- `src/Core/Decompiled/Properties/AssemblyInfo.cs`
- `src/Core/Decompiled/DECOMPILED_FROM_MASTER_09G.md`

The compiled assembly remains the byte-level authority for MASTER 09G.
Decompiled C# preserves program semantics, but original comments, formatting,
and some local variable names cannot be recovered from a compiled assembly.

The untouched upstream Dynamic Music source is separately preserved under
`upstream/DynamicMusic/` for provenance and comparison.

Extended project credit: **DF-AMP PLAYER BY RICO**.

Future DF-AMP changes should branch from the latest validated PASS state and be
committed as source before building a new runtime binary.
