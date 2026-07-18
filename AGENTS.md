# AGENTS.md

## What this is

A BepInEx (Harmony) mod DLL for the Unity game **NGU IDLE**. This repo is a
**fork of jshepler's full mod pack, trimmed to a curated subset** of mods.
The upstream author's full source (every mod they've ever written) lives in
`source/unused/` for reference; only a subset is actually compiled into the
shipped DLL.

## Structure

- `source/jshepler.ngu.mods.csproj` — single project, single output DLL
  (`jshepler.ngu.mods.dll`), `net48`, references `Assembly-CSharp.dll` +
  `UnityEngine.UI.dll` from the game install (`GameFolder` property — a
  **Windows** Steam path, e.g. `E:\SteamLibrary\...`).
- `source/*.cs` + `source/{AutoAllocator,CapCalculators,Popups,GameData,Resources}/`
  — the **active** mod set. Everything here is compiled.
- `source/unused/` — the rest of upstream's mods, kept as reference/backlog.
  Excluded from the build via `<Compile Remove="unused/**/*.cs" />` and
  `<EmbeddedResource Remove="unused/**/*.resx" />` in the csproj.
- `source/Plugin.cs` — the BepInEx plugin entry point. Defines shared events
  (`OnUpdate`, `OnSaveLoaded`, `OnOfflineProgressionComplete`, etc.) that
  individual mod files subscribe to instead of patching Unity lifecycle
  methods directly. Also holds shared helpers (`ShowNotification`,
  `ButtonColor_*`, `ShiftIsDown`/`AltIsDown`/`ControlIsDown`).
- `source/Options.cs` — all BepInEx config (`ConfigEntry<T>`) in one static
  class, grouped into nested static classes per feature (e.g. `Options.WishList.Enabled`).
- Each mod is generally **one `[HarmonyPatch]` class per file**, patching
  game types (`Character`, `WishesController`, etc.) via Harmony
  prefix/postfix/transpiler attributes.

## Key thing to know before touching `unused/`

**This repo intentionally does NOT have a `ModSave` subsystem.** Upstream's
`unused/ModSave/` hijacks the game's save serialization (transpiles
`ImportExport.gameStateToData`/`loadData`) to persist a custom
`Dictionary<string, object>` (`ModSave.Data.*`) inside the save file. That
subsystem was deliberately left out of this fork as too invasive/risky to
carry over lightly. **Any mod pulled from `unused/` that references
`ModSave.Data.*` needs its persistence dropped** — replace with a plain
static field/list (session-only, resets on game restart) rather than
pulling in the whole ModSave system. See `WishList.cs` and
`AutoAllocator/Allocators.cs` for the pattern already applied.

Similarly, `unused/AutoSaves.cs` (auto-save on various triggers) pulls in
`Hardcore.cs`, `OfflineTime.cs`, `TrackAPGained.cs`, and `ModSave` — treat
calls into it the same way (drop the call rather than porting the whole
chain), unless the AutoSaves mod itself is what's being added.

## Adding a mod from `unused/`

1. Read the target file's `using jshepler.ngu.mods.*` lines and any
   unqualified references to other custom types — that's its dependency
   graph. Cross-file references within the *same* namespace need no
   `using` (C# resolves outward through enclosing namespaces), which can
   hide dependencies on a quick skim — check actual type usage, not just
   `using` statements.
2. Check whether shared "framework" files it depends on (e.g.
   `AutoAllocator/`, `CapCalculators/`) are all-or-nothing: `AutoAllocator.cs`
   switches on `Allocators.Feature` and indexes a dictionary — if you bring
   in only some of the allocator subclasses, the switch cases for
   not-included features throw `KeyNotFoundException` at runtime. Either
   bring in the whole subsystem (verified self-contained, see below) or
   trim those switch cases.
3. `git mv` the file(s) out of `unused/` — don't copy-and-leave-a-duplicate.
4. If a dependency (ModSave, AutoSaves, etc.) is being deliberately dropped,
   **comment out the original code in place** (don't delete/rewrite over
   it) and add the replacement right next to it, with a one-line comment
   saying what's missing and why. Keeps the original logic visible without
   needing git archaeology.
5. Wire up any `Options.*` config entries the file needs (add to
   `Options.cs`; don't port entries the fork doesn't use).
6. You generally **cannot build/test locally** on a non-Windows dev
   machine (no game DLLs, `GameFolder` is a Windows path) — verify by
   careful reading, not `dotnet build`. Flag this to whoever tests on the
   real machine.

## Subsystems already verified self-contained (safe to pull wholesale)

- `AutoAllocator/` (all bar types) + `CapCalculators/` — only external
  touchpoints are `Options.Experimental.LoadoutSwapKeepsAutoAllocators`
  and (now dropped) `ModSave.Data.Enabled*IDs` persistence.
- `GameExtensionMethods.cs` — big grab-bag of extension methods, only
  depends on `Menu.cs`.
- `Popups/BasePopup.cs` + `Popups/UIScaler.cs` — generic IMGUI popup
  framework, depends on `Options.Experimental.ModPopupScaling`.

## Conventions

- One class per mod file, `internal class`, named after the feature.
- Harmony patch method naming: `TypeName_methodName_prefix/postfix/transpiler`.
- Shared input-modifier checks (`ShiftIsDown` etc.) live on `Plugin`, not
  duplicated per file (though some `unused/` files predate this and
  duplicate them locally — don't worry about de-duping on migration).
