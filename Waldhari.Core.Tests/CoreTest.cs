using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Waldhari.Core.Logging;
using Waldhari.Core.Tests.Logging;

namespace Waldhari.Core.Tests
{
    [TestFixture]
    [TestOf(typeof(Core))]
    public class CoreTest
    {
        [SetUp]
        public void Setup()
        {
            var loggerField = typeof(Core).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(loggerField != null, nameof(loggerField) + " != null");
            loggerField.SetValue(null, null); // réinitialise à null avant chaque test
        }
        
        [Test]
        public void Logger_DefaultLogger_IsNotNull()
        {
            var logger = Core.Logger;

            Assert.NotNull(logger);
            Assert.IsInstanceOf<TsvLogService>(logger);
        }


        [Test]
        public void SetLogger_ReplacesLogger()
        {
            var fakeLogger = new StubLogService();
            Core.SetLogger(fakeLogger);

            Assert.AreSame(fakeLogger, Core.Logger);
        }

        [Test]
        public void SetLogger_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Core.SetLogger(null));
        }

        [Test]
        public void Logger_OnceSet_ReturnsSameInstance()
        {
            var firstLogger = Core.Logger;
            var secondLogger = Core.Logger;

            Assert.AreSame(firstLogger, secondLogger);
        }

        [Test]
        public void Logger_AfterSetLogger_ReturnsNewInstance()
        {
            var originalLogger = Core.Logger;

            var fakeLogger = new StubLogService();
            Core.SetLogger(fakeLogger);

            var currentLogger = Core.Logger;
            Assert.AreSame(fakeLogger, currentLogger);
            Assert.AreNotSame(originalLogger, currentLogger);
        }
    }
}