using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using Waldhari.Core.Localization;
using Waldhari.Core.Tests.Logging;

namespace Waldhari.Core.Tests.Localization
{
    [TestFixture]
    [TestOf(typeof(CsvLanguageService))]
    public class CsvLanguageServiceTest
    {
        private const string ModName = "TestMod";
        
        [SetUp]
        public void Setup()
        {
            Core.SetLogger(new StubLogService());
        }

        [TearDown]
        public void Cleanup()
        {
            var scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");

            if (Directory.Exists(scriptsPath))
            {
                Directory.Delete(scriptsPath, recursive: true);
            }
        }

        [Test]
        public void Constructor_DefaultValues_Works()
        {
            var service = new CsvLanguageService(ModName);

            var expectedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari",
                ModName);
            Assert.True(Directory.Exists(expectedDir));
            CollectionAssert.AreEqual(new List<string> { "General" },
                TestsHelper.GetPrivateField<List<string>>(service, "_features"));
            Assert.AreEqual(CultureInfo.CurrentCulture.Name, service.CurrentLanguage);
        }

        [Test]
        public void Constructor_CustomModNameAndLanguage_Works()
        {
            var languageCode = "fr-FR";
            var service = new CsvLanguageService(ModName, languageCode);

            var expectedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", ModName);
            Assert.True(Directory.Exists(expectedDir));
            Assert.AreEqual(languageCode, service.CurrentLanguage);
            CollectionAssert.AreEqual(new List<string> { "General" },
                TestsHelper.GetPrivateField<List<string>>(service, "_features"));
        }

        [Test]
        public void Constructor_CustomFeatures_Works()
        {
            const string languageCode = "en-US";
            var features = new List<string> { "Menu", "Mission" };
            var service = new CsvLanguageService(ModName, languageCode, features);

            CollectionAssert.AreEqual(features, TestsHelper.GetPrivateField<List<string>>(service, "_features"));
            Assert.AreEqual(languageCode, service.CurrentLanguage);
        }

        [Test]
        public void Constructor_EmptyModName_Works()
        {
            var service = new CsvLanguageService(string.Empty);

            var expectedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", string.Empty);
            Assert.True(Directory.Exists(expectedDir));
            CollectionAssert.AreEqual(new List<string> { "General" },
                TestsHelper.GetPrivateField<List<string>>(service, "_features"));
        }

        [Test]
        public void Constructor_NullFeatures_DefaultsToGeneral()
        {
            var service = new CsvLanguageService(ModName, features: null);

            CollectionAssert.AreEqual(new List<string> { "General" },
                TestsHelper.GetPrivateField<List<string>>(service, "_features"));
        }


        [Test]
        public void DetermineLanguage_ReturnsSystemCulture_WhenLanguageCodeIsNull()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "DetermineLanguage");

            var result = method.Invoke(service, new object[] { null }) as string;

            Assert.AreEqual(CultureInfo.CurrentCulture.Name, result);
        }

        [Test]
        public void DetermineLanguage_ReturnsSystemCulture_WhenLanguageCodeIsEmpty()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "DetermineLanguage");

            var result = method.Invoke(service, new object[] { string.Empty }) as string;

            Assert.AreEqual(CultureInfo.CurrentCulture.Name, result);
        }

        [Test]
        public void DetermineLanguage_ReturnsProvidedLanguageCode_WhenNotNullOrEmpty()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "DetermineLanguage");

            var result = method.Invoke(service, new object[] { "fr-FR" }) as string;

            Assert.AreEqual("fr-FR", result);
        }

        [Test]
        public void DetermineLanguage_ReturnsProvidedLanguageCode_WhenWhitespace()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "DetermineLanguage");

            var result = method.Invoke(service, new object[] { "   " }) as string;

            Assert.AreEqual("   ", result);
        }

        [Test]
        public void LoadMessagesFromFile_LoadsValidLines()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "LoadMessagesFromFile");

            var tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, new[]
            {
                "greeting;Hello",
                "farewell;Goodbye"
            });

            method.Invoke(service, new object[] { tempFile });

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual("Hello", messages["greeting"]);
            Assert.AreEqual("Goodbye", messages["farewell"]);

            File.Delete(tempFile);
        }

        [Test]
        public void LoadMessagesFromFile_IgnoresEmptyOrCommentLines()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "LoadMessagesFromFile");

            var tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, new[]
            {
                "",
                "   ",
                "# This is a comment",
                "key;value"
            });

            method.Invoke(service, new object[] { tempFile });

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("value", messages["key"]);

            File.Delete(tempFile);
        }

        [Test]
        public void LoadMessagesFromFile_LogsErrorOnInvalidLine()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "LoadMessagesFromFile");

            var tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, new[]
            {
                "invalid_line_without_separator"
            });

            method.Invoke(service, new object[] { tempFile });

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(0, messages.Count);

            File.Delete(tempFile);
        }

        [Test]
        public void LoadMessagesFromFile_LogsWarningOnDuplicateKey()
        {
            var service = new CsvLanguageService(ModName);
            var method = TestsHelper.GetPrivateMethod(service, "LoadMessagesFromFile");

            var tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, new[]
            {
                "key;value1",
                "key;value2"
            });

            method.Invoke(service, new object[] { tempFile });

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("value1", messages["key"]);

            File.Delete(tempFile);
        }


        [Test]
        public void LoadFeatureFiles_AllFilesExist_LoadsAllMessages()
        {
            var service = new CsvLanguageService(ModName);
            var langDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(langDir);

            var features = TestsHelper.GetPrivateField<List<string>>(service, "_features");
            foreach (var feature in features)
            {
                File.WriteAllText(Path.Combine(langDir, $"{feature}.csv"), $"{feature};Value");
            }

            var method = TestsHelper.GetPrivateMethod(service, "LoadFeatureFiles");
            method.Invoke(service, new object[] { langDir });

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            foreach (var feature in features)
            {
                Assert.True(messages.ContainsKey(feature));
                Assert.AreEqual("Value", messages[feature]);
            }

            Directory.Delete(langDir, true);
        }

        [Test]
        public void LoadFeatureFiles_SomeFilesMissing_LoadsOnlyExisting()
        {
            var service = new CsvLanguageService(ModName);
            var langDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(langDir);

            var features = TestsHelper.GetPrivateField<List<string>>(service, "_features");

            File.WriteAllText(Path.Combine(langDir, $"{features[0]}.csv"), $"{features[0]};Value");

            var method = TestsHelper.GetPrivateMethod(service, "LoadFeatureFiles");
            method.Invoke(service, new object[] { langDir });

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.True(messages.ContainsKey(features[0]));
            Assert.AreEqual("Value", messages[features[0]]);

            Directory.Delete(langDir, true);
        }

        [Test]
        public void LoadFeatureFiles_FeaturesEmpty_NoExceptionAndNoMessages()
        {
            var service = new CsvLanguageService(ModName);
            var features = TestsHelper.GetPrivateField<List<string>>(service, "_features");
            features.Clear();

            var langDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(langDir);

            var method = TestsHelper.GetPrivateMethod(service, "LoadFeatureFiles");
            Assert.DoesNotThrow(() => method.Invoke(service, new object[] { langDir }));

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(0, messages.Count);

            Directory.Delete(langDir, true);
        }

        [Test]
        public void LoadFeatureFiles_DirectoryDoesNotExist_NoException()
        {
            var service = new CsvLanguageService(ModName);
            var langDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var method = TestsHelper.GetPrivateMethod(service, "LoadFeatureFiles");
            Assert.DoesNotThrow(() => method.Invoke(service, new object[] { langDir }));

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(0, messages.Count);
        }

        [Test]
        public void Load_LanguageDirExists_LoadsMessages()
        {
            var service = new CsvLanguageService(ModName);
            var langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", ModName,
                CultureInfo.CurrentCulture.Name);
            Directory.CreateDirectory(langDir);

            const string feature = "General";
            File.WriteAllText(Path.Combine(langDir, $"{feature}.csv"), $"{feature};Value");

            service.Load(ModName);

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.True(messages.ContainsKey(feature));
            Assert.AreEqual("Value", messages[feature]);
        }

        [Test]
        public void Load_LanguageDirDoesNotExist_NoExceptionAndMessagesEmpty()
        {
            var service = new CsvLanguageService(ModName);

            Assert.DoesNotThrow(() => service.Load(ModName));

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(0, messages.Count);
        }

        [Test]
        public void Load_LanguageCodeSpecified_LoadsCorrectLanguage()
        {
            const string languageCode = "fr-FR";
            var service = new CsvLanguageService(ModName);
            var langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", ModName,
                languageCode);
            Directory.CreateDirectory(langDir);

            const string feature = "General";
            File.WriteAllText(Path.Combine(langDir, $"{feature}.csv"), $"{feature};Bonjour");

            service.Load(ModName, languageCode);

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.True(messages.ContainsKey(feature));
            Assert.AreEqual("Bonjour", messages[feature]);

        }

        [Test]
        public void Load_FeaturesEmpty_NoExceptionAndMessagesEmpty()
        {
            var service = new CsvLanguageService(ModName);
            var features = TestsHelper.GetPrivateField<List<string>>(service, "_features");
            features.Clear();

            service.Load(ModName);

            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            Assert.AreEqual(0, messages.Count);
        }

        [Test]
        public void Load_NullLanguageCode_UsesSystemCulture()
        {
            var service = new CsvLanguageService(ModName);

            service.Load(ModName);

            Assert.AreEqual(CultureInfo.CurrentCulture.Name, service.CurrentLanguage);
        }
        
        [Test]
        public void GetMessage_KeyExists_ReturnsValue()
        {
            var service = new CsvLanguageService(ModName);
            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            messages["greeting"] = "Hello";

            var result = service.GetMessage("greeting");

            Assert.AreEqual("Hello", result);
        }

        [Test]
        public void GetMessage_KeyDoesNotExist_ReturnsKey()
        {
            var service = new CsvLanguageService(ModName);
            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            messages.Clear();

            var result = service.GetMessage("missingKey");

            Assert.AreEqual("missingKey", result);
        }

        [Test]
        public void GetMessage_NullId_ReturnsEmptyString()
        {
            var service = new CsvLanguageService(ModName);

            var result = service.GetMessage(null);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void GetMessage_EmptyId_ReturnsEmptyString()
        {
            var service = new CsvLanguageService(ModName);

            var result = service.GetMessage(string.Empty);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void GetMessage_KeyExists_CaseInsensitive()
        {
            var service = new CsvLanguageService(ModName);
            var messages = TestsHelper.GetPrivateField<Dictionary<string, string>>(service, "_messages");
            messages["Greeting"] = "Hello";

            var result = service.GetMessage("greeting"); // lowercase

            Assert.AreEqual("Hello", result);
        }

    }
}