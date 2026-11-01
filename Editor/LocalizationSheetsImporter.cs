using System.Collections.Generic;
using GDF.ConfigImport;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace LocalizationSheets.Editor
{
    /// <summary>
    /// ScriptableObject importer that pulls localization CSV sheets through the GDF ConfigImport
    /// pipeline (public Google Sheets or local TextAssets, no OAuth required) and writes them into
    /// any <see cref="ILocalizationBackend"/>. Default binding targets a Unity Localization
    /// <see cref="UnityEditor.Localization.StringTableCollection"/> through
    /// <see cref="UnityLocalizationBackend"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "GDF/Localization/Localization Sheets Importer", fileName = "LocalizationSheetsImporter", order = 0)]
    public sealed class LocalizationSheetsImporter : ScriptableObject
    {
        [SerializeField]
        [SerializeReference]
        [Tooltip("Pluggable data source (Google Sheets public, OAuth, or local CSV TextAsset). Assign a ScriptableObject implementing GDF.ConfigImport.IDataSource.")]
        private ScriptableObject _dataSourceAsset;

        [SerializeField]
        [Tooltip("Unity Localization string-table collection that receives the imported entries.")]
        private StringTableCollection _collection;

        [SerializeField]
        [Tooltip("Sheets to import. One per locale.")]
        private List<LocaleSheet> _locales = new List<LocaleSheet>();

        [SerializeField]
        [Tooltip("CSV column holding the localization key. Default 'Key'.")]
        private string _keyColumnName = "Key";

        [SerializeField]
        [Tooltip("CSV column holding the localized text. Default 'Value'.")]
        private string _valueColumnName = "Value";

        /// <summary>Data source (CSV reader).</summary>
        public IDataSource DataSource => _dataSourceAsset as IDataSource;

        /// <summary>Target Unity Localization collection.</summary>
        public StringTableCollection Collection => _collection;

        /// <summary>Locale sheets configured on this importer.</summary>
        public IReadOnlyList<LocaleSheet> Locales => _locales;

        /// <summary>CSV key column name.</summary>
        public string KeyColumnName => string.IsNullOrEmpty(_keyColumnName) ? "Key" : _keyColumnName;

        /// <summary>CSV value column name.</summary>
        public string ValueColumnName => string.IsNullOrEmpty(_valueColumnName) ? "Value" : _valueColumnName;

        /// <summary>
        /// Pulls every configured sheet, parses CSV via <see cref="CsvParser"/>, and writes the
        /// (key, value) pairs into <see cref="UnityLocalizationBackend"/>.
        /// </summary>
        public void Reload()
        {
            Reload(new UnityLocalizationBackend(_collection));
        }

        /// <summary>Variant that writes into a custom backend (Lean Localization, custom service, …).</summary>
        public void Reload(ILocalizationBackend backend)
        {
            if (backend == null)
            {
                Debug.LogError($"[LocalizationSheets] '{name}' has no backend.");
                return;
            }

            IDataSource source = DataSource;
            if (source == null)
            {
                Debug.LogError($"[LocalizationSheets] '{name}' has no data source assigned.");
                return;
            }

            foreach (LocaleSheet locale in _locales)
            {
                if (string.IsNullOrEmpty(locale.LocaleCode))
                {
                    Debug.LogWarning($"[LocalizationSheets] '{name}' entry has an empty locale code; skipped.");
                    continue;
                }

                string csv = source.Fetch(locale.SheetName, out string error);
                if (csv == null)
                {
                    Debug.LogError($"[LocalizationSheets] '{name}' / '{locale.LocaleCode}' fetch failed: {error}");
                    continue;
                }

                Dictionary<string, string> entries = ParseLocaleCsv(csv);
                if (entries.Count == 0)
                {
                    Debug.LogWarning($"[LocalizationSheets] '{name}' / '{locale.LocaleCode}' parsed to zero entries.");
                    continue;
                }

                backend.Write(locale.LocaleCode, entries);
            }
        }

        private Dictionary<string, string> ParseLocaleCsv(string csv)
        {
            Dictionary<string, string> entries = new Dictionary<string, string>();
            List<List<string>> rows = CsvParser.Parse(csv);
            if (rows.Count == 0)
                return entries;

            List<string> header = rows[0];
            int keyIndex = header.IndexOf(KeyColumnName);
            int valueIndex = header.IndexOf(ValueColumnName);
            if (keyIndex < 0 || valueIndex < 0)
            {
                Debug.LogError($"[LocalizationSheets] Header must contain '{KeyColumnName}' and '{ValueColumnName}'. Actual: {string.Join(",", header)}");
                return entries;
            }

            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                if (keyIndex >= row.Count || valueIndex >= row.Count)
                    continue;

                string key = row[keyIndex];
                if (string.IsNullOrEmpty(key))
                    continue;

                entries[key] = row[valueIndex];
            }

            return entries;
        }

        /// <summary>Per-locale sheet entry.</summary>
        [System.Serializable]
        public struct LocaleSheet
        {
            [Tooltip("ISO 639-1 (optionally with region, e.g. 'en', 'en-US', 'ru'). Must match a Locale registered under Edit -> Project Settings -> Localization.")]
            public string LocaleCode;

            [Tooltip("Sheet identifier passed to the data source. For Google Sheets — tab name.")]
            public string SheetName;
        }
    }
}
