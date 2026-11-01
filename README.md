# GDF Localization Sheets

Bridge between a CSV / Google Sheets data source and a localization backend. Default binding
pulls strings from a **public** Google Sheet via `gviz/csv` (no OAuth) and writes them into a
Unity Localization `StringTableCollection`. Backends are pluggable — Lean Localization, a
custom game service, or a remote CDN can be used instead by implementing one small interface.

Package name: `com.gdf.localization-sheets`
Assembly name: `GDF.LocalizationSheets`

## Features

- `ILocalizationBackend` bridge interface — one method, writes `(key -> value)` pairs for a locale.
- `UnityLocalizationBackend` default — writes into a Unity Localization `StringTableCollection`; creates missing `StringTable`s for registered locales automatically.
- `LocalizationSheetsImporter` ScriptableObject pairs a `com.gdf.config-import` data source with a list of `(LocaleCode, SheetName)` entries. Configurable key / value column names (default `Key` / `Value`).
- Zero-OAuth path for prototypes: paste a public Google Sheet URL into `GoogleSheetsDataSource`, press `Reload`, Unity Localization populates.
- Production path: swap the data source for `GoogleSheetsOAuthDataSource` when the spreadsheet goes private — all other wiring stays the same.
- Inspector `Reload` and `Open in browser` buttons; `Tools/GDF/Reload All Localization` menu runs every importer at once.

## Requirements

- Unity `6000.0+`
- `com.gdf.config-import` `1.0.0+` (the CSV / Google Sheets pipeline)
- `com.unity.localization` `1.5.5+` (the default backend target)

## Installation (UPM via Git)

Add this dependency to `Packages/manifest.json`:

```json
"com.gdf.localization-sheets": "https://github.com/AlexPyriel/LocalizationSheets.git#v1.0.0"
```

For development tracking (not recommended for production), use `#main` instead of a tag.

You can also install it from Unity Package Manager:

- `Window -> Package Manager -> + -> Add package from git URL...`
- `https://github.com/AlexPyriel/LocalizationSheets.git#v1.0.0`

## Usage

### 1. Prepare Unity Localization

Open `Edit -> Project Settings -> Localization` and either create a `LocalizationSettings`
asset or let the first interaction create one. Register the locales you need (`en`, `ru`, …).
Create a `StringTableCollection` from `Window -> Asset Management -> Localization Tables`
(`Table Collection Name` can be anything — e.g. `UI`).

### 2. Create a public Google Sheet

A single spreadsheet with one tab per locale. First row = header. Columns = at least `Key`
and `Value`. Share with `Anyone with the link - Viewer`.

Example `en` tab:

| Key | Value |
|---|---|
| menu.play | Play |
| menu.settings | Settings |

### 3. Author the importer

`Create -> GDF -> Localization -> Localization Sheets Importer`. Fill in:

- `Data Source Asset`: a `GoogleSheetsDataSource` asset configured with your spreadsheet id.
- `Collection`: the `StringTableCollection` from step 1.
- `Locales`: one entry per `(LocaleCode, SheetName)` pair (`en` -> `en`, `ru` -> `ru`, …).
- `Key Column Name` / `Value Column Name`: defaults are `Key` / `Value`.

Click `Reload` — the string tables fill in. Click `Open in browser` to inspect the sheet.

### 4. Use Unity Localization in game code

Everything downstream stays Unity Localization:

```csharp
LocalizedString greeting = new LocalizedString(tableReference: "UI", tableEntryReference: "menu.play");
string resolved = greeting.GetLocalizedString();
```

### 5. Custom backend

Target Lean Localization or a game-local service by implementing `ILocalizationBackend` and
calling `importer.Reload(myBackend)` manually. The default `Reload()` keeps using
`UnityLocalizationBackend`.

## Assembly Definition Notes

Two assemblies:

- `GDF.LocalizationSheets` (runtime, platform-independent) — exposes `ILocalizationBackend` for
  code that chooses to receive localization strings at runtime through a custom backend.
- `GDF.LocalizationSheets.Editor` (editor-only) — holds the importer, the Unity Localization
  backend and the inspector tooling. Depends on `GDF.ConfigImport.Editor` and `Unity.Localization.Editor`.

## License

MIT. See `LICENSE.md`.
