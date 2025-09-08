namespace Waldhari.Core.Localization
{
    /// <summary>
    /// Provides localization services for GTA5 mods, enabling multi-language support
    /// through message retrieval and dynamic language loading capabilities.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Loads localization data for the specified mod and language.
        /// This method initializes or reloads the language resources, allowing for
        /// runtime language switching in the mod.
        /// </summary>
        /// <param name="modName">The name of the mod to load localization for</param>
        /// <param name="languageCode">Optional language code (e.g., "en-US", "fr-FR"). 
        /// If null, uses the system's current culture</param>
        void Load(string modName, string languageCode = null);

        /// <summary>
        /// Retrieves a localized message by its unique identifier.
        /// This is the primary method for accessing translated strings in the mod.
        /// </summary>
        /// <param name="id">The unique identifier for the message to retrieve</param>
        /// <returns>The localized message string, or the key itself if not found (fallback behavior)</returns>
        string GetMessage(string id);

        /// <summary>
        /// Gets the currently loaded language code.
        /// Useful for displaying current language to users or for conditional logic
        /// based on the active language.
        /// </summary>
        string CurrentLanguage { get; }
    }
}