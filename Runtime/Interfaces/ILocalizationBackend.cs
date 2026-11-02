using System.Collections.Generic;

namespace GDF.LocalizationSheets
{
    /// <summary>
    /// Target storage for localization strings pulled from an external source (Google Sheets, CSV).
    /// The default binding writes into Unity Localization <c>StringTable</c>s; custom implementations
    /// can target other systems (Lean Localization, a game-local service, remote CDN).
    /// </summary>
    public interface ILocalizationBackend
    {
        /// <summary>Backend name shown in logs and inspector feedback.</summary>
        public string Name { get; }

        /// <summary>
        /// Writes a batch of (key, value) pairs into the backend for a specific locale. Implementations
        /// should treat the batch as the complete content for the locale and may clear previous entries.
        /// </summary>
        /// <param name="localeCode">ISO 639-1 (optionally with region, e.g. "en", "en-US", "ru").</param>
        /// <param name="entries">Key -> text pairs to persist for this locale.</param>
        public void Write(string localeCode, IReadOnlyDictionary<string, string> entries);
    }
}
