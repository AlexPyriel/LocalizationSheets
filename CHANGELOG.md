# Changelog

## [1.0.0] - 2026-11-02

Initial public release.

- `ILocalizationBackend` bridge for localization storage. Default binding `UnityLocalizationBackend` writes into a Unity Localization `StringTableCollection`; custom backends can target Lean Localization, a game-local service, or a remote CDN.
- `LocalizationSheetsImporter` ScriptableObject: pairs a `com.gdf.config-import` data source with a list of `(LocaleCode, SheetName)` entries. Configurable key / value column names (default `Key` / `Value`).
- Inspector buttons: `Reload` (pulls every locale and writes into the active backend) and `Open in browser` (opens the data source URL in the system browser — zero-OAuth Google Sheets).
- `Tools/GDF/Reload All Localization` menu runs every importer in the project.
- Reuses the existing `com.gdf.config-import` CSV pipeline — public Google Sheets through the `gviz/csv` endpoint, OAuth for private spreadsheets, local `TextAsset`.
- Non-destructive integration: the Unity Localization editor UI, Smart Strings, `LocalizedString` serializable fields, platform-locale selectors — everything remains Unity's. The bridge only populates string tables.
