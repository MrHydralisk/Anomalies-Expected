# Building Anomalies Expected for RimWorld 1.7

RimWorld 1.7 runs on .NET 10 (`RimWorld.runtimeconfig.json` → `"tfm": "net10.0"`,
`System.Runtime, Version=10.0.0.0`), so the 1.5/1.6 assemblies cannot be reused and the
source has to be recompiled against the game's own assemblies.

## What broke on 1.7

`Pawn_MutantTracker` is no longer a `Verse.Thing` in 1.7. The 1.6 build patched
`Pawn_MutantTracker::GetGizmos()` with a postfix whose `__instance` was declared as
`Verse.Thing`:

```
System.ArgumentException: Unrelated classes cannot describe the same non-null object:
  original IEnumerable<Gizmo> RimWorld.Pawn_MutantTracker::GetGizmos(),
  patch static void AnomaliesExpected.HarmonyPatches::T_GetGizmos_Postfix(IEnumerable<Gizmo>& __result, Verse.Thing __instance)
```

Harmony throws while `AnomaliesExpected.HarmonyPatches` is being statically initialized, so
the type initializer stays broken for the rest of the session and every later call
(`RaceProperties.IsAnomalyEntity` etc.) rethrows `TypeInitializationException`. That is why
world generation, faction leaders and starting pawns all fail.

The fix (already in `Source/Anomalies Expected/HarmonyPatches.cs`) splits the patch in two:
a `Pawn_MutantTracker` overload that forwards `__instance.ConstantCaster` to the original
`Thing`-typed method.

## Build recipe (no .NET SDK needed)

Tested with the Roslyn compiler from
`Microsoft.Net.Compilers.Toolset` 4.11.0 (`tasks/net472/csc.exe`) — it runs on the .NET
Framework that ships with Windows.

1. Collect references: **every managed DLL in the RimWorld root folder** (filter out native
   ones — `AssemblyName::GetAssemblyName` failing is a good filter) plus `0Harmony.dll`
   from the Harmony workshop item. Do **not** reference the .NET Framework reference
   assemblies: that produces
   `CS0012 ... defined in an assembly that is not referenced ... System.Runtime 10.0.0.0`.
2. Compile with `/nostdlib+` so the game's `System.Private.CoreLib`/`System.Runtime` are
   used, e.g.:

```
csc.exe /target:library /langversion:latest /optimize+ /deterministic /define:TRACE /nostdlib+ ^
  /out:"1.7\Assemblies\Anomalies Expected.dll" ^
  /reference:<every managed game dll> /reference:0Harmony.dll ^
  "Source\Anomalies Expected\*.cs" (recursive)
```

Expected result on `f295f100` + the 1.7 patch: 0 errors, 7 warnings (unused fields).
