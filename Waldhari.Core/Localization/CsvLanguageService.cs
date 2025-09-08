using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Waldhari.Core.Localization
{
    /// <summary>
    /// CSV-based implementation of the language service for GTA5 mods.
    /// Provides localization support by loading translation data from CSV files
    /// organized by language and feature sets.
    /// </summary>
    /// <remarks>
    /// The service expects a directory structure: GTA5 directory/scripts/Waldhari/{ModName}/{LanguageCode}/{Feature}.csv
    /// Each CSV file contains key-value pairs separated by semicolons for translation data.
    /// </remarks>
    public class CsvLanguageService : ILanguageService
    {
        private readonly string _localizationDir;
        private readonly Dictionary<string, string> _messages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _features;

        /// <summary>
        /// Gets the currently loaded language code (e.g., "en-US", "fr-FR").
        /// </summary>
        public string CurrentLanguage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CSV language service.
        /// Sets up the localization directory structure and loads initial language data.
        /// </summary>
        /// <param name="modName">Name of the mod for organizing localization files (required)</param>
        /// <param name="languageCode">Optional language code to load initially. Uses system culture if null</param>
        /// <param name="features">Optional list of feature names to load. Defaults to "General" if null</param>
        /// <remarks>
        /// Features allow organizing translations into logical groups (e.g., "UI", "Messages", "Errors")
        /// Each feature corresponds to a separate CSV file for better maintainability.
        /// </remarks>
        public CsvLanguageService(string modName, string languageCode = null, IEnumerable<string> features = null)
        {
            // Set up the base localization directory structure
            _localizationDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", modName);
            if (!Directory.Exists(_localizationDir))
                Directory.CreateDirectory(_localizationDir);

            // Initialize feature list - features help organize translations by functional areas
            _features = features != null ? new List<string>(features) : new List<string> { "General" };

            Core.Logger.Info($"Initializing CsvLanguageService for mod '{modName}' with language '{languageCode ?? CultureInfo.CurrentCulture.Name}'");
            Core.Logger.Debug($"Features to load: {string.Join(", ", _features)}");

            Load(modName, languageCode);
        }

        /// <summary>
        /// Loads or reloads localization data for the specified mod and language.
        /// Clears existing messages and populates from CSV files in the language directory.
        /// </summary>
        /// <param name="modName">The mod name to load localization for</param>
        /// <param name="languageCode">Optional language code. Uses system culture if null</param>
        /// <remarks>
        /// This method enables runtime language switching by clearing current data
        /// and loading fresh translations from the target language directory.
        /// </remarks>
        public void Load(string modName = "Waldhari.Core", string languageCode = null)
        {
            _messages.Clear();
            CurrentLanguage = DetermineLanguage(languageCode);
            Core.Logger.Info($"Loading language '{CurrentLanguage}' for mod '{modName}'");

            var langDir = Path.Combine(_localizationDir, CurrentLanguage);
            if (!Directory.Exists(langDir))
            {
                Core.Logger.Warn($"Language directory not found: {langDir}");
                return;
            }

            LoadFeatureFiles(langDir);

            Core.Logger.Info($"Language '{CurrentLanguage}' loaded with {_messages.Count} messages");
        }
        
        /// <summary>
        /// Loads translation files for all configured features from the language directory.
        /// Each feature corresponds to a separate CSV file for modular organization.
        /// </summary>
        /// <param name="langDir">The language-specific directory containing feature CSV files</param>
        private void LoadFeatureFiles(string langDir)
        {
            // Process each feature as a separate CSV file for better organization
            foreach (var feature in _features)
            {
                var filePath = Path.Combine(langDir, $"{feature}.csv");
                if (!File.Exists(filePath))
                {
                    Core.Logger.Warn($"Language file not found: {filePath}");
                    continue;
                }

                Core.Logger.Debug($"Loading file: {filePath}");
                LoadMessagesFromFile(filePath);
            }
        }

        /// <summary>
        /// Parses a CSV file and loads key-value translation pairs into the message dictionary.
        /// Handles basic CSV parsing with semicolon separators and comment line filtering.
        /// </summary>
        /// <param name="filePath">Path to the CSV file containing translations</param>
        /// <remarks>
        /// Expected CSV format: KEY;VALUE per line
        /// Lines starting with '#' are treated as comments and ignored.
        /// Duplicate keys are logged as warnings and skipped to maintain data integrity.
        /// </remarks>
        private void LoadMessagesFromFile(string filePath)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                // Skip empty lines and comment lines (starting with #)
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // Parse the key-value pair separated by semicolon
                var parts = line.Split(new[] { ';' }, 2);
                if (parts.Length != 2)
                {
                    Core.Logger.Error($"Unexpected part length '{parts.Length}' (>2) detected in {filePath} for line: {line}");
                    continue;
                }

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                // Prevent duplicate keys to maintain translation consistency
                if (_messages.ContainsKey(key))
                {
                    Core.Logger.Warn($"Duplicate key '{key}' detected in {filePath}");
                    continue;
                }

                _messages.Add(key, value);
            }
        }

        /// <summary>
        /// Determines the appropriate language code to use for localization.
        /// Falls back to system culture if no specific language code is provided.
        /// </summary>
        /// <param name="languageCode">The requested language code</param>
        /// <returns>The language code to use for loading translations</returns>
        private string DetermineLanguage(string languageCode)
        {
            // Use system culture as fallback for better user experience
            return string.IsNullOrEmpty(languageCode) 
                ? CultureInfo.CurrentCulture.Name 
                : languageCode;
        }

        /// <summary>
        /// Retrieves a localized message by its unique identifier.
        /// Returns the key itself if the translation is not found (graceful degradation).
        /// </summary>
        /// <param name="id">The unique identifier for the message to retrieve</param>
        /// <returns>The localized message string, or the key as fallback if not found</returns>
        /// <remarks>
        /// The fallback behavior ensures the mod continues functioning even with missing translations,
        /// displaying the message key instead of breaking the user experience.
        /// </remarks>
        public string GetMessage(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Core.Logger.Warn("GetMessage called with null or empty id");
                return string.Empty;
            }

            // Return translated message if found, otherwise use key as fallback
            if (_messages.TryGetValue(id, out var value))
            {
                Core.Logger.Debug($"Message retrieved for key '{id}'");
                return value;
            }

            // Graceful degradation: return the key itself if translation missing
            Core.Logger.Warn($"Message key '{id}' not found, returning key as fallback");
            return id;
        }
    }
}