using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Waldhari.Core.Localization
{
    public class CsvLanguageService : ILanguageService
    {
        private readonly string _localizationDir;
        private readonly Dictionary<string, string> _messages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _features;

        public string CurrentLanguage { get; private set; }

        public CsvLanguageService(string modName = "Waldhari.Core", string languageCode = null, IEnumerable<string> features = null)
        {
            _localizationDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", modName);
            if (!Directory.Exists(_localizationDir))
                Directory.CreateDirectory(_localizationDir);

            _features = features != null ? new List<string>(features) : new List<string> { "General" };

            Core.Logger.Info($"Initializing CsvLanguageService for mod '{modName}' with language '{languageCode ?? CultureInfo.CurrentCulture.Name}'");
            Core.Logger.Debug($"Features to load: {string.Join(", ", _features)}");

            Load(modName, languageCode);
        }


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
        
        private void LoadFeatureFiles(string langDir)
        {
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

        private void LoadMessagesFromFile(string filePath)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split(new[] { ';' }, 2);
                if (parts.Length != 2)
                {
                    Core.Logger.Error($"Unexpected part length '{parts.Length}' (>2) detected in {filePath} for line: {line}");
                    continue;
                }

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (_messages.ContainsKey(key))
                {
                    Core.Logger.Warn($"Duplicate key '{key}' detected in {filePath}");
                    continue;
                }

                _messages.Add(key, value);
            }
        }

        private string DetermineLanguage(string languageCode)
        {
            return string.IsNullOrEmpty(languageCode) 
                ? CultureInfo.CurrentCulture.Name 
                : languageCode;
        }



        public string GetMessage(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Core.Logger.Warn("GetMessage called with null or empty id");
                return string.Empty;
            }

            if (_messages.TryGetValue(id, out var value))
            {
                Core.Logger.Debug($"Message retrieved for key '{id}'");
                return value;
            }

            Core.Logger.Warn($"Message key '{id}' not found, returning key as fallback");
            return id;
        }

    }
}
