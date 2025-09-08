using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Waldhari.Core.Logging;

namespace Waldhari.Core.Tests.Logging
{
    [TestFixture]
    [TestOf(typeof(TsvLogService))]
    public class TsvLogServiceTest
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
        public void Constructor_CreatesLogsDirectory_WhenNotExists()
        {
            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");
            Assert.False(Directory.Exists(logsDir));
            
            var logger = new TsvLogService(ModName);

            Assert.True(Directory.Exists(logsDir));
            Assert.AreEqual(LogLevel.Debug, logger.Level);
        }

        [Test]
        public void Constructor_SetsLevelProperly()
        {
            var logger = new TsvLogService(ModName, LogLevel.Warn);

            Assert.AreEqual(LogLevel.Warn, logger.Level);
        }

        [Test]
        public void Constructor_DefaultValues_Work()
        {
            var logger = new TsvLogService(ModName);

            var expectedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari",
                ModName+".log");
            Assert.AreEqual(expectedFile, TestsHelper.GetPrivateField<string>(logger, "_logFilePath"));
            Assert.AreEqual(LogLevel.Debug, logger.Level);
        }

        [Test]
        public void Constructor_HandlesEmptyModName()
        {
            var logger = new TsvLogService(string.Empty);

            var expectedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari", ".log");
            Assert.AreEqual(expectedFile, TestsHelper.GetPrivateField<string>(logger, "_logFilePath"));
        }

        [Test]
        public void Constructor_DirectoryAlreadyExists_DoesNotThrow()
        {
            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");
            Directory.CreateDirectory(logsDir);

            // ReSharper disable once ObjectCreationAsStatement
            Assert.DoesNotThrow(() => new TsvLogService("ExistingMod"));
        }

        [Test]
        public void CleanString_NullInput_ReturnsEmptyQuoted()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger, "CleanString");

            string result = (string)method.Invoke(logger, new object[] { null });
            Assert.AreEqual("\"\"", result);
        }

        [Test]
        public void CleanString_EmptyString_ReturnsEmptyQuoted()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"CleanString");

            string result = (string)method.Invoke(logger, new object[] { string.Empty });
            Assert.AreEqual("\"\"", result);
        }

        [Test]
        public void CleanString_NoSpecialCharacters_ReturnsQuoted()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"CleanString");

            string input = "Hello World";
            string result = (string)method.Invoke(logger, new object[] { input });

            Assert.AreEqual("\"Hello World\"", result);
        }

        [Test]
        public void CleanString_Tab_ReplacedWithSpace()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"CleanString");

            string input = "Hello\tWorld";
            string result = (string)method.Invoke(logger, new object[] { input });

            Assert.AreEqual("\"Hello World\"", result);
        }

        [Test]
        public void CleanString_Quotes_EscapedProperly()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"CleanString");

            string input = "She said \"Hello\"";
            string result = (string)method.Invoke(logger, new object[] { input });

            Assert.AreEqual("\"She said \"\"Hello\"\"\"", result);
        }

        [Test]
        public void CleanString_MixedSpecialCharacters()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"CleanString");

            string input = "Line1\nLine2\t\"Quote\"";
            string result = (string)method.Invoke(logger, new object[] { input });

            Assert.AreEqual("\"Line1\nLine2 \"\"Quote\"\"\"", result);
        }

        [Test]
        public void Write_DebugMessage_WritesToFile_WhenLevelAllows()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"Write");

            method.Invoke(logger, new object[] { LogLevel.Debug, "Debug message", null });

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string content = File.ReadAllText(logPath);
            Assert.True(content.Contains("Debug\t\"Debug message\""));
        }

        [Test]
        public void Write_InfoMessage_WritesToFile_WhenLevelAllows()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"Write");

            method.Invoke(logger, new object[] { LogLevel.Info, "Info message", null });

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string content = File.ReadAllText(logPath);
            Assert.True(content.Contains("Info\t\"Info message\""));
        }

        [Test]
        public void Write_WarnMessage_WritesToFile_WhenLevelAllows()
        {
            var logger = new TsvLogService(ModName, LogLevel.Info);
            var method = TestsHelper.GetPrivateMethod(logger,"Write");

            method.Invoke(logger, new object[] { LogLevel.Warn, "Warning message", null });

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string content = File.ReadAllText(logPath);
            Assert.True(content.Contains("Warn\t\"Warning message\""));
        }

        [Test]
        public void Write_ErrorMessageWithException_WritesToFile()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger,"Write");

            var ex = new InvalidOperationException("Test exception");
            method.Invoke(logger, new object[] { LogLevel.Error, "Error message", ex });

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string content = File.ReadAllText(logPath);

            Assert.True(content.Contains("Error\t\"Error message\""));
            Assert.True(content.Contains("System.InvalidOperationException: Test exception"));
        }

        [Test]
        public void Write_MessageBelowLevel_DoesNotWrite()
        {
            var logger = new TsvLogService(ModName, LogLevel.Warn);
            var method = TestsHelper.GetPrivateMethod(logger,"Write");

            method.Invoke(logger, new object[] { LogLevel.Info, "Should not appear", null });

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string content = File.Exists(logPath) ? File.ReadAllText(logPath) : string.Empty;

            Assert.False(content.Contains("Should not appear"));
        }

        [Test]
        public void Write_ConcurrentWrites_DoesNotThrow()
        {
            var logger = new TsvLogService(ModName);
            var method = TestsHelper.GetPrivateMethod(logger, "Write");

            Parallel.For(0, 10, i =>
            {
                method.Invoke(logger, new object[] { LogLevel.Debug, $"Message {i}", null });
            });

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string[] lines = File.ReadAllLines(logPath);
            Assert.AreEqual(10, lines.Length, "Expected 10 log lines");

            int[] counts = new int[10];
            foreach (var line in lines)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (line.Contains($"Message {i}"))
                        counts[i]++;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(1, counts[i], $"Message 'Message {i}' should appear exactly once but appears {counts[i]} times.");
            }
        }
        
        [Test]
        public void Debug_WritesMessage()
        {
            var logger = new TsvLogService(ModName);
            logger.Debug("Debug message");

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string[] lines = File.ReadAllLines(logPath);

            Assert.AreEqual(1, lines.Length);
            Assert.True(lines[0].Contains("Debug message"));
            Assert.True(lines[0].Contains("Debug"));
        }

        [Test]
        public void Info_WritesMessage()
        {
            var logger = new TsvLogService(ModName);
            logger.Info("Info message");

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string[] lines = File.ReadAllLines(logPath);

            Assert.AreEqual(1, lines.Length);
            Assert.True(lines[0].Contains("Info message"));
            Assert.True(lines[0].Contains("Info"));
        }

        [Test]
        public void Warn_WritesMessage()
        {
            var logger = new TsvLogService(ModName);
            logger.Warn("Warn message");

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string[] lines = File.ReadAllLines(logPath);

            Assert.AreEqual(1, lines.Length);
            Assert.True(lines[0].Contains("Warn message"));
            Assert.True(lines[0].Contains("Warn"));
        }

        [Test]
        public void Error_WritesMessage_AndException()
        {
            var logger = new TsvLogService(ModName);
            var ex = new InvalidOperationException("Test exception");
            logger.Error("Error message", ex);

            string logPath = TestsHelper.GetPrivateField<string>(logger, "_logFilePath");
            string[] lines = File.ReadAllLines(logPath);

            Assert.AreEqual(1, lines.Length);
            Assert.True(lines[0].Contains("Error message"));
            Assert.True(lines[0].Contains("Error"));
            Assert.True(lines[0].Contains("Test exception"));
        }

    }
}