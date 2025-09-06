namespace Waldhari.Core.Localization
{
    public interface ILanguageService
    {
        void Load(string modName, string languageCode = null);

        string GetMessage(string id);

        string CurrentLanguage { get; }
    }
}