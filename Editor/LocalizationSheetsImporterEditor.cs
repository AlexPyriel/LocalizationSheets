using UnityEditor;
using UnityEngine;

namespace LocalizationSheets.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="LocalizationSheetsImporter"/>: adds <c>Reload</c> and
    /// <c>Open in browser</c> buttons that drive the pipeline from the editor.
    /// </summary>
    [CustomEditor(typeof(LocalizationSheetsImporter))]
    public sealed class LocalizationSheetsImporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LocalizationSheetsImporter importer = (LocalizationSheetsImporter)target;
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = importer.DataSource != null && importer.Collection != null;
                if (GUILayout.Button("Reload", GUILayout.Height(28)))
                {
                    importer.Reload();
                    Debug.Log($"[LocalizationSheets] '{importer.name}' reload complete.");
                }

                string browseUrl = importer.DataSource?.GetBrowseUrl();
                GUI.enabled = !string.IsNullOrEmpty(browseUrl);
                if (GUILayout.Button("Open in browser", GUILayout.Height(28)))
                    Application.OpenURL(browseUrl);

                GUI.enabled = true;
            }
        }
    }
}
