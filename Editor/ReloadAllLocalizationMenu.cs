using UnityEditor;
using UnityEngine;

namespace LocalizationSheets.Editor
{
    /// <summary>
    /// Scans the project for every <see cref="LocalizationSheetsImporter"/> asset and reloads each.
    /// Convenient for designers refreshing the whole translation set after editing Google Sheets.
    /// </summary>
    public static class ReloadAllLocalizationMenu
    {
        private const string MENU_PATH = "Tools/GDF/Reload All Localization";

        [MenuItem(MENU_PATH)]
        private static void ReloadAll()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(LocalizationSheetsImporter)}");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning("[LocalizationSheets] No LocalizationSheetsImporter assets found.");
                return;
            }

            int failed = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                LocalizationSheetsImporter importer = AssetDatabase.LoadAssetAtPath<LocalizationSheetsImporter>(path);
                if (importer == null)
                    continue;

                try
                {
                    importer.Reload();
                }
                catch (System.Exception ex)
                {
                    failed++;
                    Debug.LogError($"[LocalizationSheets] '{importer.name}' reload threw: {ex.Message}");
                }
            }

            Debug.Log($"[LocalizationSheets] Reload All — importers={guids.Length}, failed={failed}.");
        }
    }
}
