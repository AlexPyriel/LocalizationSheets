using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace GDF.LocalizationSheets.Editor
{
    /// <summary>
    /// Default <see cref="ILocalizationBackend"/>: writes entries into a Unity Localization
    /// <c>StringTableCollection</c>. One collection carries every locale; the backend picks (or
    /// creates) the <c>StringTable</c> matching the <paramref name="localeCode"/>.
    /// </summary>
    public sealed class UnityLocalizationBackend : ILocalizationBackend
    {
        private readonly StringTableCollection _collection;

        /// <summary>Creates a backend bound to a specific <see cref="StringTableCollection"/> asset.</summary>
        /// <param name="collection">Target collection. Must be non-null.</param>
        public UnityLocalizationBackend(StringTableCollection collection)
        {
            _collection = collection != null ? collection : throw new System.ArgumentNullException(nameof(collection));
        }

        /// <inheritdoc />
        public string Name => $"UnityLocalization({_collection.name})";

        /// <inheritdoc />
        public void Write(string localeCode, IReadOnlyDictionary<string, string> entries)
        {
            if (string.IsNullOrEmpty(localeCode))
            {
                Debug.LogWarning("[LocalizationSheets] Write called with an empty locale code; skipped.");
                return;
            }

            StringTable target = FindOrCreateTable(localeCode);
            if (target == null)
            {
                Debug.LogError($"[LocalizationSheets] No StringTable for locale '{localeCode}' and failed to create one.");
                return;
            }

            target.Clear();
            foreach (KeyValuePair<string, string> entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Key))
                    continue;

                target.AddEntry(entry.Key, entry.Value ?? string.Empty);
            }

            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(_collection);
            AssetDatabase.SaveAssets();
        }

        private StringTable FindOrCreateTable(string localeCode)
        {
            foreach (StringTable table in _collection.StringTables)
            {
                if (table == null || table.LocaleIdentifier.Code == null)
                    continue;

                if (string.Equals(table.LocaleIdentifier.Code, localeCode, System.StringComparison.OrdinalIgnoreCase))
                    return table;
            }

            // No table for this locale yet — try to add one if the project has the locale registered.
            foreach (UnityEngine.Localization.Locale locale in UnityEditor.Localization.LocalizationEditorSettings.GetLocales())
            {
                if (!string.Equals(locale.Identifier.Code, localeCode, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                _collection.AddNewTable(locale.Identifier);
                return (StringTable)_collection.GetTable(locale.Identifier);
            }

            Debug.LogError($"[LocalizationSheets] Locale '{localeCode}' is not registered in LocalizationSettings. Add it via Edit -> Project Settings -> Localization first.");
            return null;
        }
    }
}
